using System;
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

namespace Puzzle
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		private const int Tolerance = 20;

		private readonly Random _random = new Random();

		private PuzzleCollection _puzzles;

		/// <summary>
		/// Gets the game details;
		/// </summary>
		public GameDetails GameDetails { get; private set; }

		/// <summary>
		/// Gets the collection of high scores.
		/// </summary>
		public HighScores HighScores { get; }

		public List<List<Thumb>> ConnectedPieces = new List<List<Thumb>>();
		public bool start = true;
		private int _zCoordinate = int.MinValue + 1;
		public Stream stream;
		private readonly DropShadowEffect _shadowEffect;

		public MainWindow()
		{
			HighScores = new HighScores();
			InitializeComponent();

			setTimers();
			SetSortDescriptions();

			_shadowEffect = new DropShadowEffect
			{
				Color = Colors.Black,
				Direction = 300,
				ShadowDepth = 25,
				BlurRadius = 10,
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
			ConnectedPieces.Clear();
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
			_puzzles = new PuzzleCollection();

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
			ConnectedPieces.Add(newList);

			SetPuzzleEventHandlers(puzzle);
			GameImage.Children.Add(puzzle);

			_puzzles.Add(puzzle, puzzlePiece);
		}

		private void SetPuzzleEventHandlers(Thumb puzzle)
		{
			puzzle.DragStarted += PuzzleDragStarted;
			puzzle.DragDelta += PuzzleDragDelta;
			puzzle.DragCompleted += PuzzleDragCompleted;
			puzzle.MouseRightButtonDown += PuzzleMouseRightButtonDown;
		}

		private void PuzzleMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			var thumb = (Thumb)sender;
			var connectedPieces = ConnectedPieces.Single(u => u.Contains(thumb));
			if (connectedPieces.Count != 1) return;

			var puzzlePiece = _puzzles[thumb];
			puzzlePiece.Rotate();
			var centerX = -1 + (GameDetails.PuzzleSize - 1) / 2;
			var centerY = -1 + (GameDetails.PuzzleSize - 1) / 2;

			thumb.Background.Transform = new RotateTransform(puzzlePiece.RotationAngle, centerX, centerY);
		}

		private void PuzzleDragStarted(object sender, DragStartedEventArgs e)
		{
			var puzzle = sender as Thumb;
			var connectedPieces = ConnectedPieces.Single(u => u.Contains(puzzle));

			foreach (var piece in connectedPieces)
				Panel.SetZIndex(piece, _zCoordinate);

			_zCoordinate++;
		}

		private void PuzzleDragDelta(object sender, DragDeltaEventArgs e)
		{
			var puzzle = sender as Thumb;
			var connectedPieces = ConnectedPieces.Single(u => u.Contains(puzzle));
			bool moveVertical = true, moveHorizontal = true;

			foreach (var piece in connectedPieces)
			{
				var top = Canvas.GetTop(piece) + e.VerticalChange;
				var left = Canvas.GetLeft(piece) + e.HorizontalChange;

				if (top <= 0 || top >= GameBackground.ActualHeight - GameDetails.PuzzleSize) moveVertical = false;
				if (left > GameBackground.ActualWidth - GameDetails.PuzzleSize || left <= 0) moveHorizontal = false;
			}

			foreach (var piece in connectedPieces)
			{
				if (moveVertical) Canvas.SetTop(piece, Canvas.GetTop(piece) + e.VerticalChange);
				if (moveHorizontal) Canvas.SetLeft(piece, Canvas.GetLeft(piece) + e.HorizontalChange);
				piece.Effect = _shadowEffect;
			}
		}

		private void PuzzleDragCompleted(object sender, DragCompletedEventArgs e)
		{
			var puzzle = sender as Thumb;
			var connectedPieces = ConnectedPieces.Single(u => u.Contains(puzzle));
			var puzzlePiece = _puzzles[puzzle];

			if (puzzlePiece.RotationAngle == 0)
				ConnectPuzzles(connectedPieces);

			foreach (var piece in connectedPieces)
				piece.Effect = null;

			if (ConnectedPieces.Count == 1) EndGame();
		}

		private void ConnectPuzzles(List<Thumb> connectedPieces)
		{
			for (var i = 0; i < connectedPieces.Count; i++)
			{
				var puzzle = connectedPieces[i];
				var puzzlePiece = _puzzles[puzzle];

				var left = Canvas.GetLeft(puzzle);
				var top = Canvas.GetTop(puzzle);

				if (puzzlePiece.Row < GameDetails.Rows - 1)
					if (TryConnectWithPuzzle(Direction.Down, puzzle, connectedPieces, puzzlePiece, left, top)) break;

				if (puzzlePiece.Row > 0)
					if (TryConnectWithPuzzle(Direction.Up, puzzle, connectedPieces, puzzlePiece, left, top)) break;

				if (puzzlePiece.Column > 0)
					if (TryConnectWithPuzzle(Direction.Left, puzzle, connectedPieces, puzzlePiece, left, top)) break;

				if (puzzlePiece.Column < GameDetails.Columns - 1)
					if (TryConnectWithPuzzle(Direction.Right, puzzle, connectedPieces, puzzlePiece, left, top)) break;
			}
		}

		private bool TryConnectWithPuzzle(Point direction, UIElement puzzle, List<Thumb> connectedPieces, PuzzlePiece puzzlePiece, double left, double top)
		{
			var checkPuzzle = _puzzles[puzzlePiece.Row + (int)direction.Y, puzzlePiece.Column + (int)direction.X];
			var checkThumb = checkPuzzle.Key;
			var checkPuzzlePiece = checkPuzzle.Value;

			if (checkPuzzlePiece.RotationAngle != 0) return false;
			var checkConnectedPieces = ConnectedPieces.Single(u => u.Contains(checkThumb));
			if (checkConnectedPieces == connectedPieces) return false;
			if (!AreCloseToEachOther(direction, puzzle, checkThumb)) return false;

			Canvas.SetLeft(puzzle, Canvas.GetLeft(checkThumb) - (int)direction.X * GameDetails.PuzzleSize);
			Canvas.SetTop(puzzle, Canvas.GetTop(checkThumb) - (int)direction.Y * GameDetails.PuzzleSize);

			var deltaX = Canvas.GetLeft(puzzle) - left;
			var deltaY = Canvas.GetTop(puzzle) - top;

			foreach (var piece in connectedPieces.Where(p => p!= puzzle))
			{
				Canvas.SetLeft(piece, Canvas.GetLeft(piece) + deltaX);
				Canvas.SetTop(piece, Canvas.GetTop(piece) + deltaY);
			}

			connectedPieces.AddRange(checkConnectedPieces);
			ConnectedPieces.Remove(checkConnectedPieces);

			return true;
		}

		private bool AreCloseToEachOther(Point direction, UIElement puzzle, UIElement checkThumb)
		{
			return Canvas.GetTop(checkThumb) < Canvas.GetTop(puzzle) + (int)direction.Y * GameDetails.PuzzleSize + Tolerance &&
				   Canvas.GetTop(checkThumb) > Canvas.GetTop(puzzle) + (int)direction.Y * GameDetails.PuzzleSize - Tolerance &&
				   Canvas.GetLeft(checkThumb) < Canvas.GetLeft(puzzle) + (int)direction.X * GameDetails.PuzzleSize + Tolerance &&
				   Canvas.GetLeft(checkThumb) > Canvas.GetLeft(puzzle) + (int)direction.X * GameDetails.PuzzleSize - Tolerance;
		}

		private void EndGame()
		{
			timer.Stop();
			var points = ConnectedPieces.Max(pieces => pieces.Count);
			var endWindow = new EndWindow(points, seconds, GameDetails.PuzzleCount) { Owner = this };
			var result = endWindow.ShowDialog();
			if (!result.HasValue || !result.Value)
			{
				timer.Start();
				return;
			}

			HighScores.Add(GameDetails.Difficulty, endWindow.HighScore);
			NewGame();
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
