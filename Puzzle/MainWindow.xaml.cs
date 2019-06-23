﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using Puzzle.Game;
using Puzzle.HelperClasses;

namespace Puzzle
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		#region variables

		private readonly Random _random = new Random();

		private Dictionary<Thumb, PuzzlePiece> _puzzles;

		/// <summary>
		/// Gets the game details;
		/// </summary>
		public GameDetails GameDetails { get; private set; }

		/// <summary>
		/// Gets the collection of high scores.
		/// </summary>
		public HighScores HighScores { get; }

		public List<List<Thumb>> UnionList = new List<List<Thumb>>();
		public bool start = true;
		private int _zCoordinate = int.MinValue + 1;
		public Stream stream;
		private readonly DropShadowBitmapEffect _shadowEffect;
		private Thumb[,] _thumbTab;
		private const int Toleration = 20;
		private readonly Point LEFT = new Point { X = -1, Y = 0 };
		private Point RIGHT = new Point { X = 1, Y = 0 };
		private Point UP = new Point { X = 0, Y = -1 };
		private Point DOWN = new Point { X = 0, Y = 1 };

		#endregion

		public MainWindow()
		{
			HighScores = new HighScores();
			InitializeComponent();

			setTimers();
			SetSortDescriptions();

			_shadowEffect = new DropShadowBitmapEffect
			{
				Color = Colors.Black,
				Direction = 300,
				ShadowDepth = 25,
				Softness = 1,
				Opacity = 0.5
			};
		}

		private void SetSortDescriptions()
		{
			EasyListView.Items.SortDescriptions.Add(new SortDescription("counter", ListSortDirection.Descending));
			EasyListView.Items.SortDescriptions.Add(new SortDescription("time", ListSortDirection.Ascending));
			MediumListView.Items.SortDescriptions.Add(new SortDescription("counter", ListSortDirection.Descending));
			MediumListView.Items.SortDescriptions.Add(new SortDescription("time", ListSortDirection.Ascending));
			HardListView.Items.SortDescriptions.Add(new SortDescription("counter", ListSortDirection.Descending));
			HardListView.Items.SortDescriptions.Add(new SortDescription("time", ListSortDirection.Ascending));
		}

		#region buttons
		private void StartButtonClick(object sender, RoutedEventArgs e)
		{
			if (start)
			{
				Difficulty difficulty;
				if (HardRadio.IsChecked.Value) difficulty = Difficulty.Hard;
				else if (MediumRadio.IsChecked.Value) difficulty = Difficulty.Medium;
				else difficulty = Difficulty.Easy;
				GameDetails = new GameDetails(difficulty);
				var dlg = new Microsoft.Win32.OpenFileDialog();
				dlg.Filter = "Image Files (*.bmp, *.jpg, *.png)|*.bmp;*.jpg;*.png";

				if (dlg.ShowDialog() == true)
				{
					var image = OpenChosenFile(dlg.FileName);
					if (image == null)
						return;

					CreatePuzzle(image);
					new_timer();

					StartButton.Content = "End game";
					PauseButton.IsEnabled = true;
					start = false;
				}
			}
			else
			{
				EndGame();
			}
		}

		private void NewGame()
		{
			start = true;
			StartButton.Content = "Start Game";
			PauseButton.IsEnabled = false;
			stream.Close();
			GameImage.Background = null;
			timer.Stop();
			TimerLabel.Visibility = Visibility.Hidden;
			UnionList.Clear();
			GameImage.Children.Clear();
		}

		private BitmapImage OpenChosenFile(string file)
		{
			try
			{
				var image = new BitmapImage();
				stream = File.Open(file, FileMode.Open);

				image.BeginInit();
				image.StreamSource = stream;
				image.EndInit();
				image.Freeze();
				return image;
			}
			catch (Exception exc)
			{
				stream.Close();
				GameImage.Children.Clear();
				MessageBox.Show("An problem occured while opening the file " + exc.Message);
			}
			return null;
		}

		private void CreatePuzzle(BitmapSource image)
		{
			_thumbTab = new Thumb[GameDetails.Rows, GameDetails.Columns];
			_puzzles = new Dictionary<Thumb, PuzzlePiece>();

			for (var row = 0; row < GameDetails.Rows; row++)
				for (var column = 0; column < GameDetails.Columns; column++)
				{
					var cb = new CroppedBitmap(image
						, new Int32Rect(column * image.PixelWidth / GameDetails.Columns, row * image.PixelHeight / GameDetails.Rows
							, image.PixelWidth / GameDetails.Columns, image.PixelHeight / GameDetails.Rows));
					var imgBrush = new ImageBrush(cb);

					CreateThumb(row, column, imgBrush);
				}
		}

		private void CreateThumb(int row, int column, TileBrush imgBrush)
		{
			var puzzlePiece = new PuzzlePiece(column, row);
			imgBrush.Transform = new RotateTransform(puzzlePiece.RotationAngle, -1 + (GameDetails.PuzzleSize - 1) / 2, -1 + (GameDetails.PuzzleSize - 1) / 2);
			imgBrush.Stretch = Stretch.Fill;

			var puzzle = new Thumb { Width = GameDetails.PuzzleSize, Height = GameDetails.PuzzleSize };
			Canvas.SetLeft(puzzle, _random.NextDouble() * (GameImage.ActualWidth - GameDetails.PuzzleSize));
			Canvas.SetTop(puzzle, _random.NextDouble() * (GameImage.ActualHeight - GameDetails.PuzzleSize));

			Panel.SetZIndex(puzzle, int.MinValue);
			puzzle.Background = imgBrush;
			var newList = new List<Thumb> { puzzle };
			UnionList.Add(newList);
			puzzle.Tag = new thumbTag { unionNr = row * GameDetails.Columns + column, listName = newList };

			_thumbTab[row, column] = puzzle;
			SetThumbEventHandlers(puzzle);
			GameImage.Children.Add(puzzle);

			_puzzles.Add(puzzle, puzzlePiece);
		}

		private void SetThumbEventHandlers(Thumb puzzle)
		{
			puzzle.DragStarted += PuzzleDragStarted;
			puzzle.DragDelta += PuzzleDragDelta;
			puzzle.DragCompleted += PuzzleDragCompleted;
			puzzle.MouseRightButtonDown += PuzzleMouseRightButtonDown;
		}

		private void PuzzleMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			var thumb = (Thumb)sender;
			var tt = (thumbTag)thumb.Tag;
			var puzzlePiece = _puzzles[thumb];

			if (tt.listName.Count != 1) return;
			var imgBrush = thumb.Background;

			puzzlePiece.Rotate();
			var centerX = -1 + (GameDetails.PuzzleSize - 1) / 2;
			var centerY = -1 + (GameDetails.PuzzleSize - 1) / 2;

			imgBrush.Transform = new RotateTransform(puzzlePiece.RotationAngle, centerX, centerY);
		}

		private void EndGame()
		{
			timer.Stop();

			var points = UnionList.Max(i => i.Count);

			var ew = new EndWindow(points, seconds, GameDetails.PuzzleCount) { Owner = this };
			var result = ew.ShowDialog();
			if (result.HasValue && result.Value)
			{
				HighScores.Add(GameDetails.Difficulty, ew.HighScore);
				NewGame();
			}
			else
			{
				timer.Start();
			}
		}

		private void PuzzleDragStarted(object sender, DragStartedEventArgs e)
		{
			var puzzle = sender as Thumb;
			var tt = (thumbTag)puzzle.Tag;

			foreach (var t in tt.listName)
			{
				Panel.SetZIndex(t, _zCoordinate);
			}
			_zCoordinate++;
		}

		private void PuzzleDragDelta(object sender, DragDeltaEventArgs e)
		{
			var puzzle = sender as Thumb;
			var tt = (thumbTag)puzzle.Tag;
			bool moveVertical = true, moveHorizontal = true;

			for (var i = 0; i < tt.listName.Count; i++)
			{
				if (Canvas.GetTop(tt.listName[i]) + e.VerticalChange <= 0
					|| Canvas.GetTop(tt.listName[i]) + e.VerticalChange >= GameBackground.ActualHeight - GameDetails.PuzzleSize)
					moveVertical = false;
				if (Canvas.GetLeft(tt.listName[i]) + e.HorizontalChange > GameBackground.ActualWidth - GameDetails.PuzzleSize
				|| Canvas.GetLeft(tt.listName[i]) + e.HorizontalChange <= 0)
					moveHorizontal = false;
			}

			for (var i = 0; i < tt.listName.Count; i++)
			{
				if (moveVertical)
					Canvas.SetTop(tt.listName[i], Canvas.GetTop(tt.listName[i]) + e.VerticalChange);
				if (moveHorizontal)
					Canvas.SetLeft(tt.listName[i], Canvas.GetLeft(tt.listName[i]) + e.HorizontalChange);
				tt.listName[i].BitmapEffect = _shadowEffect;
			}
		}

		private void PuzzleDragCompleted(object sender, DragCompletedEventArgs e)
		{
			var puzzle = sender as Thumb;
			var tTag = (thumbTag)puzzle.Tag;
			var puzzlePiece = _puzzles[puzzle];

			if (puzzlePiece.RotationAngle == 0)
				ConnectPuzzles(tTag, puzzlePiece);

			foreach (var t in tTag.listName)
				t.BitmapEffect = null;
		}

		private void ConnectPuzzles(thumbTag tTag, PuzzlePiece puzzlePiece)
		{
			for (var i = 0; i < tTag.listName.Count; i++)
			{
				var puzzle = tTag.listName[i];
				tTag = (thumbTag)puzzle.Tag;
				var left = Canvas.GetLeft(puzzle);
				var top = Canvas.GetTop(puzzle);
				if (puzzlePiece.Row < GameDetails.Rows - 1)
					CheckPuzzle(DOWN, puzzle, tTag, puzzlePiece, left, top);

				if (puzzlePiece.Row > 0)
					CheckPuzzle(UP, puzzle, tTag, puzzlePiece, left, top);

				if (puzzlePiece.Column > 0)
					CheckPuzzle(LEFT, puzzle, tTag, puzzlePiece, left, top);

				if (puzzlePiece.Column < GameDetails.Columns - 1)
					CheckPuzzle(RIGHT, puzzle, tTag, puzzlePiece, left, top);

				if (UnionList.Count == 1)
					EndGame();
			}
		}

		private void CheckPuzzle(Point direction, Thumb puzzle, thumbTag tTag, PuzzlePiece puzzlePiece, double left, double top)
		{
			var checkThumb = _thumbTab[puzzlePiece.Row + (int)direction.Y, puzzlePiece.Column + (int)direction.X];
			var checkPuzzlePiece = _puzzles[checkThumb];
			if (checkPuzzlePiece.RotationAngle != 0) return;
			var checkTTag = (thumbTag)checkThumb.Tag;
			if (checkTTag.listName != tTag.listName)
				if (puzzlePiece.RotationAngle == 0
					&& Canvas.GetTop(checkThumb) < Canvas.GetTop(puzzle) + (int)direction.Y * GameDetails.PuzzleSize + Toleration
					&& Canvas.GetTop(checkThumb) > Canvas.GetTop(puzzle) + (int)direction.Y * GameDetails.PuzzleSize - Toleration
					&& Canvas.GetLeft(checkThumb) < Canvas.GetLeft(puzzle) + (int)direction.X * GameDetails.PuzzleSize + Toleration
					&& Canvas.GetLeft(checkThumb) > Canvas.GetLeft(puzzle) + (int)direction.X * GameDetails.PuzzleSize - Toleration)
				{
					var l = checkTTag.listName;
					while (l.Count != 0)
					{
						tTag.listName.Add(l[l.Count - 1]);
						var ltt = (thumbTag)l[l.Count - 1].Tag;
						ltt.listName = tTag.listName;
						l.Remove(l[l.Count - 1]);
					}

					UnionList.Remove(l);

					Canvas.SetLeft(puzzle, Canvas.GetLeft(checkThumb) - (int)direction.X * GameDetails.PuzzleSize);
					Canvas.SetTop(puzzle, Canvas.GetTop(checkThumb) - (int)direction.Y * GameDetails.PuzzleSize);

					var deltaX = Canvas.GetLeft(puzzle) - left;
					var deltaY = Canvas.GetTop(puzzle) - top;

					for (var j = 0; j < tTag.listName.Count && tTag.listName[j] != puzzle && tTag.listName[j] != checkThumb; j++)
					{
						Canvas.SetLeft(tTag.listName[j], Canvas.GetLeft(tTag.listName[j]) - deltaX);
						Canvas.SetTop(tTag.listName[j], Canvas.GetTop(tTag.listName[j]) - deltaY);
					}
				}
		}

		private void PauseButtonClick(object sender, RoutedEventArgs e)
		{
			timer.Stop();
			StartButton.IsEnabled = false;
			PauseButton.IsEnabled = false;
			EasyRadio.IsEnabled = false;
			MediumRadio.IsEnabled = false;
			HardRadio.IsEnabled = false;
			PauseImage.Opacity = 0.8;
			PauseImage.Visibility = Visibility.Visible;
		}

		#endregion
	}
}
