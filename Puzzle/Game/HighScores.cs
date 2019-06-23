using System.Collections.ObjectModel;

namespace Puzzle.Game
{
	public class HighScores
	{
		/// <summary>
		/// Gets the collection of high scores for easy difficulty.
		/// </summary>
		public ObservableCollection<HighScoreRecord> EasyHighScores { get; }

		/// <summary>
		/// Gets the collection of high scores for medium difficulty.
		/// </summary>
		public ObservableCollection<HighScoreRecord> MediumHighScores { get; }

		/// <summary>
		/// Gets the collection of high scores for hard difficulty.
		/// </summary>
		public ObservableCollection<HighScoreRecord> HardHighScores { get; }

		public HighScores()
		{
			EasyHighScores = new ObservableCollection<HighScoreRecord>();
			MediumHighScores = new ObservableCollection<HighScoreRecord>();
			HardHighScores = new ObservableCollection<HighScoreRecord>();
		}
	}
}
