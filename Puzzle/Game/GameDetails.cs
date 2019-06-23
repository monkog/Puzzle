using System;

namespace Puzzle.Game
{
	public class GameDetails
	{
		/// <summary>
		/// Gets the current game difficulty.
		/// </summary>
		public Difficulty Difficulty { get; }

		/// <summary>
		/// Gets the number of puzzles in the row.
		/// </summary>
		public int HorizontalPuzzleCount { get; private set; }

		/// <summary>
		/// Gets the number of puzzles in the column.
		/// </summary>
		public int VerticalPuzzleCount { get; private set; }

		/// <summary>
		/// Gets the overall number of puzzles.
		/// </summary>
		public int PuzzleCount { get; private set; }

		/// <summary>
		/// Gets the size of the puzzle.
		/// </summary>
		public int PuzzleSize { get; private set; }

		public GameDetails(Difficulty difficulty)
		{
			Difficulty = difficulty;
			InitializeGameBoard();
		}

		private void InitializeGameBoard()
		{
			switch (Difficulty)
			{
				case Difficulty.Easy:
					HorizontalPuzzleCount = 4;
					VerticalPuzzleCount = 3;
					PuzzleSize = 146;
					break;
				case Difficulty.Medium:
					HorizontalPuzzleCount = 8;
					VerticalPuzzleCount = 6;
					PuzzleSize = 71;
					break;
				case Difficulty.Hard:
					HorizontalPuzzleCount = 12;
					VerticalPuzzleCount = 8;
					PuzzleSize = 46;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			PuzzleCount = VerticalPuzzleCount * HorizontalPuzzleCount;
		}
	}
}
