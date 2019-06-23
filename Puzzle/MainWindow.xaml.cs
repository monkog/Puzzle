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
using System.Windows.Threading;
using Microsoft.Win32;
using Puzzle.Game;

namespace Puzzle
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		private const int Tolerance = 20;

		private readonly DropShadowEffect _shadowEffect;

		private readonly Random _random = new Random();

		private int _gameDurationInSeconds;

		private DispatcherTimer _gameTimer, _pauseTimer;

		private int _zCoordinate = int.MinValue + 1;

		private bool _isGameRunning;

		private PuzzleCollection _puzzles;

		private List<List<Thumb>> _connectedPieces;

		/// <summary>
		/// Gets the game details;
		/// </summary>
		public GameDetails GameDetails { get; private set; }

		/// <summary>
		/// Gets the collection of high scores.
		/// </summary>
		public HighScores HighScores { get; }

		public MainWindow()
		{
			HighScores = new HighScores();
			InitializeComponent();

			InitializeTimers();
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

		private void StartButtonClick(object sender, RoutedEventArgs e)
		{
			if (_isGameRunning)
			{
				EndGame();
				return;
			}

			var openFileDialog = new OpenFileDialog { Filter = Properties.Resources.ImageFilesExtensions };
			if (openFileDialog.ShowDialog() != true) return;

			var image = new BitmapImage(new Uri(openFileDialog.FileName));

			Difficulty difficulty;
			if (HardRadio.IsChecked == true) difficulty = Difficulty.Hard;
			else if (MediumRadio.IsChecked == true) difficulty = Difficulty.Medium;
			else difficulty = Difficulty.Easy;
			GameDetails = new GameDetails(difficulty);

			CreatePuzzle(image);
			ResetTimer();

			StartButton.Content = "End game";
			PauseButton.IsEnabled = true;
			_isGameRunning = true;
		}

		private void NewGame()
		{
			_isGameRunning = false;
			StartButton.Content = "Start Game";
			PauseButton.IsEnabled = false;
			_gameTimer.Stop();
			TimerLabel.Visibility = Visibility.Hidden;
		}

		private void CreatePuzzle(BitmapSource image)
		{
			GameImage.Background = null;
			GameImage.Children.Clear();

			_puzzles = new PuzzleCollection();
			_connectedPieces = new List<List<Thumb>>();

			for (var row = 0; row < GameDetails.Rows; row++)
				for (var column = 0; column < GameDetails.Columns; column++)
				{
					var bitmap = new CroppedBitmap(image
						, new Int32Rect(column * image.PixelWidth / GameDetails.Columns, row * image.PixelHeight / GameDetails.Rows
							, image.PixelWidth / GameDetails.Columns, image.PixelHeight / GameDetails.Rows));
					var imgBrush = new ImageBrush(bitmap);

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
			_connectedPieces.Add(newList);

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
			var connectedPieces = _connectedPieces.Single(u => u.Contains(thumb));
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
			var connectedPieces = _connectedPieces.Single(u => u.Contains(puzzle));

			foreach (var piece in connectedPieces)
				Panel.SetZIndex(piece, _zCoordinate);

			_zCoordinate++;
		}

		private void PuzzleDragDelta(object sender, DragDeltaEventArgs e)
		{
			var puzzle = sender as Thumb;
			var connectedPieces = _connectedPieces.Single(u => u.Contains(puzzle));
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
			var connectedPieces = _connectedPieces.Single(u => u.Contains(puzzle));
			var puzzlePiece = _puzzles[puzzle];

			foreach (var piece in connectedPieces)
				piece.Effect = null;

			if (!_isGameRunning) return;

			if (puzzlePiece.RotationAngle == 0)
				ConnectPuzzles(connectedPieces);

			if (_connectedPieces.Count == 1) EndGame();
		}

		private void ConnectPuzzles(List<Thumb> connectedPieces)
		{
			for (var i = 0; i < connectedPieces.Count; i++)
			{
				var puzzle = connectedPieces[i];
				var puzzlePiece = _puzzles[puzzle];

				if (puzzlePiece.Row < GameDetails.Rows - 1)
					TryConnectWithPuzzle(Direction.Down, puzzle, connectedPieces, puzzlePiece);

				if (puzzlePiece.Row > 0)
					TryConnectWithPuzzle(Direction.Up, puzzle, connectedPieces, puzzlePiece);

				if (puzzlePiece.Column > 0)
					TryConnectWithPuzzle(Direction.Left, puzzle, connectedPieces, puzzlePiece);

				if (puzzlePiece.Column < GameDetails.Columns - 1)
					TryConnectWithPuzzle(Direction.Right, puzzle, connectedPieces, puzzlePiece);
			}
		}

		private void TryConnectWithPuzzle(Point direction, UIElement puzzle, List<Thumb> connectedPieces, PuzzlePiece puzzlePiece)
		{
			var left = Canvas.GetLeft(puzzle);
			var top = Canvas.GetTop(puzzle);

			var checkPuzzle = _puzzles[puzzlePiece.Row + (int)direction.Y, puzzlePiece.Column + (int)direction.X];
			var checkThumb = checkPuzzle.Key;
			var checkPuzzlePiece = checkPuzzle.Value;

			if (checkPuzzlePiece.RotationAngle != 0) return;
			var checkConnectedPieces = _connectedPieces.Single(u => u.Contains(checkThumb));
			if (checkConnectedPieces == connectedPieces) return;
			if (!AreCloseToEachOther(direction, puzzle, checkThumb)) return;

			Canvas.SetLeft(puzzle, Canvas.GetLeft(checkThumb) - (int)direction.X * GameDetails.PuzzleSize);
			Canvas.SetTop(puzzle, Canvas.GetTop(checkThumb) - (int)direction.Y * GameDetails.PuzzleSize);

			var deltaX = Canvas.GetLeft(puzzle) - left;
			var deltaY = Canvas.GetTop(puzzle) - top;

			foreach (var piece in connectedPieces.Where(p => p != puzzle))
			{
				Canvas.SetLeft(piece, Canvas.GetLeft(piece) + deltaX);
				Canvas.SetTop(piece, Canvas.GetTop(piece) + deltaY);
			}

			connectedPieces.AddRange(checkConnectedPieces);
			_connectedPieces.Remove(checkConnectedPieces);
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
			_gameTimer.Stop();
			var points = _connectedPieces.Max(pieces => pieces.Count);
			var endWindow = new EndWindow(points, _gameDurationInSeconds, GameDetails.PuzzleCount) { Owner = this };
			var result = endWindow.ShowDialog();
			if (!result.HasValue || !result.Value)
			{
				_gameTimer.Start();
				return;
			}

			HighScores.Add(GameDetails.Difficulty, endWindow.HighScore);
			NewGame();
		}

		private void InitializeTimers()
		{
			_gameTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 1) };
			_gameTimer.Tick += GameTimerTick;

			_pauseTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 50) };
			_pauseTimer.Tick += PauseTimerTick;
		}

		private void ResetTimer()
		{
			TimerLabel.Visibility = Visibility.Visible;
			_gameTimer.Stop();
			_gameDurationInSeconds = 0;
			TimerLabel.Content = $"Time {(_gameDurationInSeconds / 3600):00}:{(_gameDurationInSeconds / 60):00}:{(_gameDurationInSeconds % 60):00}";
			_gameTimer.Start();
		}

		private void PauseImageMouseDown(object sender, MouseButtonEventArgs e)
		{
			_pauseTimer.Start();
		}

		private void GameTimerTick(object sender, EventArgs e)
		{
			_gameDurationInSeconds++;
			TimerLabel.Content = $"Time {(_gameDurationInSeconds / 3600):00}:{(_gameDurationInSeconds / 60):00}:{(_gameDurationInSeconds % 60):00}";
		}

		private void PauseTimerTick(object sender, EventArgs e)
		{
			PauseImage.Opacity -= 0.05;
			PauseImage.UpdateLayout();

			if (PauseImage.Opacity > 0.1) return;

			_pauseTimer.Stop();
			_gameTimer.Start();
			PauseImage.Visibility = Visibility.Hidden;
		}

		private void PauseButtonClick(object sender, RoutedEventArgs e)
		{
			_gameTimer.Stop();
			PauseImage.Opacity = 0.8;
			PauseImage.Visibility = Visibility.Visible;
		}
	}
}
