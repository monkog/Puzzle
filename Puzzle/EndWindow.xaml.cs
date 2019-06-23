using System;
using System.Linq;
using System.Windows;
using Puzzle.Game;
using Puzzle.HelperClasses;

namespace Puzzle
{
	/// <summary>
	/// Interaction logic for EndWindow.xaml
	/// </summary>
	public partial class EndWindow : Window
	{
		public int max = 1;

		public EndWindow()
		{
			InitializeComponent();
			WindowStyle = WindowStyle.ToolWindow;
			ResizeMode = ResizeMode.NoResize;
		}

		private void Window_Loaded_1(object sender, RoutedEventArgs e)
		{
			var mw = Owner as MainWindow;
			var minutes = (mw.seconds / 3600).ToString("00");
			var seconds = (mw.seconds / 60).ToString("00");
			var milliseconds = (mw.seconds % 60).ToString("00");

			TimerLabel.Content = $"{minutes}:{seconds}:{milliseconds}";

			max = mw.UnionList.Max(i => i.Count);

			if (mw.GameDetails.Points == mw.GameDetails.PuzzleCount)
				WonLabel.Content = $"You won! You connected all {mw.GameDetails.Points} puzzles. Your time:";
			else
				WonLabel.Content = $"You connected only {max} puzzle(s). Your time:";
		}

		private void OkButtonClick(object sender, RoutedEventArgs e)
		{
			var mw = Owner as MainWindow;
			var name = NameTextBox.Text;

			if (name == string.Empty)
				name = "John Doe";

			switch (mw.GameDetails.Difficulty)
			{
				case Difficulty.Hard:
					mw.HighScores.HardHighScores.Add(new HighScoreRecord(name, max, mw.seconds));
					break;
				case Difficulty.Medium:
					mw.HighScores.MediumHighScores.Add(new HighScoreRecord(name, max, mw.seconds));
					break;
				case Difficulty.Easy:
					mw.HighScores.EasyHighScores.Add(new HighScoreRecord(name, max, mw.seconds));
					break;
			}

			NewGame(mw);
			Close();
		}

		private void CancelButtonClick(object sender, RoutedEventArgs e)
		{
			var mw = Owner as MainWindow;

			if (mw.GameDetails.Points == mw.GameDetails.PuzzleCount)
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
