using System.Windows;
using Puzzle.Game;

namespace Puzzle
{
	/// <summary>
	/// Interaction logic for EndWindow.xaml
	/// </summary>
	public partial class EndWindow
	{
		private readonly int _seconds;

		private readonly int _points;

		private readonly int _maxPoints;

		/// <summary>
		/// Gets the high score.
		/// </summary>
		public HighScoreRecord HighScore { get; private set; }

		public EndWindow(int points, int seconds, int maxPoints)
		{
			_points = points;
			_seconds = seconds;
			_maxPoints = maxPoints;

			InitializeComponent();
		}

		private void WindowLoaded(object sender, RoutedEventArgs e)
		{
			var hours = (_seconds / 3600).ToString("00");
			var minutes = (_seconds / 60).ToString("00");
			var seconds = (_seconds % 60).ToString("00");

			TimeElapsed.Content = $"{hours}:{minutes}:{seconds}";
			var message = _points == _maxPoints ? Properties.Resources.AllPiecesConnectedMessage : Properties.Resources.NotAllPiecesConnectedMessage;
			EndGameMessage.Content = string.Format(message, _points);

			CancelButton.IsEnabled = _points != _maxPoints;
		}

		private void OkButtonClick(object sender, RoutedEventArgs e)
		{
			var name = NameTextBox.Text;
			if (name == string.Empty) name = "John Doe";
			HighScore = new HighScoreRecord(name, _points, _seconds);
			DialogResult = true;
			Close();
		}

		private void CancelButtonClick(object sender, RoutedEventArgs e)
		{
			DialogResult = _points == _maxPoints;
			Close();
		}
	}
}
