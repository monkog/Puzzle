using System;
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

		/// <summary>
		/// Adds the high score result.
		/// </summary>
		/// <param name="difficulty">Game difficulty.</param>
		/// <param name="highScore">Result to add.</param>
		public void Add(Difficulty difficulty, HighScoreRecord highScore)
		{
			switch (difficulty)
			{
				case Difficulty.Hard:
					HardHighScores.Add(highScore);
					break;
				case Difficulty.Medium:
					MediumHighScores.Add(highScore);
					break;
				case Difficulty.Easy:
					EasyHighScores.Add(highScore);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, null);
			}
		}
	}
}
