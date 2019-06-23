using System;

namespace Puzzle.Game
{
	public class PuzzlePiece
	{
		private readonly int[] _angles = { 0, 90, 180, 270 };

		private static readonly Random Random = new Random();

		/// <summary>
		/// Gets the column of this piece of puzzle.
		/// </summary>
		public int Column { get; }

		/// <summary>
		/// Gets the row of this piece of puzzle.
		/// </summary>
		public int Row { get; }

		/// <summary>
		/// Gets the rotation angle of this piece.
		/// </summary>
		public int RotationAngle { get; private set; }

		public PuzzlePiece(int column, int row)
		{
			Column = column;
			Row = row;

			RotationAngle = _angles[Random.Next(3)];
		}

		/// <summary>
		/// Rotates the puzzle piece.
		/// </summary>
		public void Rotate()
		{
			RotationAngle = (RotationAngle + 90) % 360;
		}
	}
}
