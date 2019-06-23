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
		/// Gets the number of columns.
		/// </summary>
		public int Columns { get; private set; }

		/// <summary>
		/// Gets the number of rows.
		/// </summary>
		public int Rows { get; private set; }

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
					Columns = 4;
					Rows = 3;
					PuzzleSize = 146;
					break;
				case Difficulty.Medium:
					Columns = 8;
					Rows = 6;
					PuzzleSize = 71;
					break;
				case Difficulty.Hard:
					Columns = 12;
					Rows = 8;
					PuzzleSize = 46;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			PuzzleCount = Rows * Columns;
		}
	}
}
