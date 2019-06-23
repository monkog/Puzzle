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

			max = mw.unionList.Max(i => i.Count);

			if (mw.gd.Points == mw.gd.PuzzleCount)
				WonLabel.Content = $"You won! You connected all {mw.gd.Points} puzzles. Your time:";
			else
				WonLabel.Content = $"You connected only {max} puzzle(s). Your time:";
		}

		private void OkButtonClick(object sender, RoutedEventArgs e)
		{
			var mw = Owner as MainWindow;
			var name = NameTextBox.Text;

			if (name == string.Empty)
				name = "John Doe";

			switch (mw.gd.Difficulty)
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

			if (mw.gd.Points == mw.gd.PuzzleCount)
				NewGame(mw);
			else
				mw.timer.Start();

			Close();
		}

		private static void NewGame(MainWindow mw)
		{
			mw.start = true;
			mw.startButton.Content = "Start Game";
			mw.pauseButton.IsEnabled = false;
			mw.stream.Close();
			mw.image.Background = null;
			mw.timer.Stop();
			mw.timerLabel.Visibility = Visibility.Hidden;
			mw.unionList.Clear();
			mw.image.Children.Clear();
		}
	}
}
