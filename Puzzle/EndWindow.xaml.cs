using System.Windows;
using Puzzle.Game;

namespace Puzzle
{
	/// <summary>
	/// Interaction logic for EndWindow.xaml
	/// </summary>
	public partial class EndWindow : Window
	{
		private readonly Difficulty _gameDifficulty;

		private readonly int _seconds;

		private readonly int _points;

		private readonly int _maxPoints;

		public EndWindow(Difficulty gameDifficulty, int points, int seconds, int maxPoints)
		{
			_gameDifficulty = gameDifficulty;
			_points = points;
			_seconds = seconds;
			_maxPoints = maxPoints;
			InitializeComponent();
		}

		private void Window_Loaded_1(object sender, RoutedEventArgs e)
		{
			var minutes = (_seconds / 3600).ToString("00");
			var seconds = (_seconds / 60).ToString("00");
			var milliseconds = (_seconds % 60).ToString("00");

			TimerLabel.Content = $"{minutes}:{seconds}:{milliseconds}";

			if (_points == _maxPoints)
				WonLabel.Content = $"You won! You connected all {_points} puzzles. Your time:";
			else
				WonLabel.Content = $"You connected only {_points} puzzle(s). Your time:";
		}

		private void OkButtonClick(object sender, RoutedEventArgs e)
		{
			var mw = Owner as MainWindow;

			var name = NameTextBox.Text;
			if (name == string.Empty) name = "John Doe";
			var highScore = new HighScoreRecord(name, _points, _seconds);

			switch (_gameDifficulty)
			{
				case Difficulty.Hard:
					mw.HighScores.HardHighScores.Add(highScore);
					break;
				case Difficulty.Medium:
					mw.HighScores.MediumHighScores.Add(highScore);
					break;
				case Difficulty.Easy:
					mw.HighScores.EasyHighScores.Add(highScore);
					break;
			}

			NewGame(mw);
			Close();
		}

		private void CancelButtonClick(object sender, RoutedEventArgs e)
		{
			var mw = Owner as MainWindow;

			if (_points == _maxPoints)
				NewGame(mw);
			else
				mw.timer.Start();

			Close();
		}

		private static void NewGame(MainWindow mw)
		{
			mw.start = true;
			mw.StartButton.Content = "Start Game";
			mw.PauseButton.IsEnabled = false;
			mw.stream.Close();
			mw.GameImage.Background = null;
			mw.timer.Stop();
			mw.TimerLabel.Visibility = Visibility.Hidden;
			mw.UnionList.Clear();
			mw.GameImage.Children.Clear();
		}
	}
}
