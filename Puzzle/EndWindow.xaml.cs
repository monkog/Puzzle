using System;
using System.Linq;
using System.Windows;
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

			if (mw.gd.currentGameCounter == mw.maxCount)
				WonLabel.Content = $"You won! You connected all {mw.gd.currentGameCounter} puzzles. Your time:";
			else
				WonLabel.Content = $"You connected only {max} puzzle(s). Your time:";
		}

		private void OkButtonClick(object sender, RoutedEventArgs e)
		{
			var mw = Owner as MainWindow;
			var name = NameTextBox.Text;

			if (name == string.Empty)
				name = "John Doe";

			switch (mw.gd.gameLevel)
			{
				case 'h':
					mw.hardList.Add(new listItems { name = name, counter = max, time = mw.seconds });
					break;
				case 'm':
					mw.mediumList.Add(new listItems { name = name, counter = max, time = mw.seconds });
					break;
				case 'e':
					mw.easyList.Add(new listItems { name = name, counter = max, time = mw.seconds });
					break;
			}

			NewGame(mw);
			Close();
		}

		private void CancelButtonClick(object sender, RoutedEventArgs e)
		{
			var mw = Owner as MainWindow;

			if (mw.gd.currentGameCounter == mw.maxCount)
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
