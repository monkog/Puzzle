using System;

namespace Puzzle.Game
{
	public class PuzzlePiece
	{
		private readonly int[] _angles = { 0, 90, 180, 270 };

		private static readonly Random Random = new Random();

		/// <summary>
		/// Gets the X coordinate of this piece of puzzle.
		/// </summary>
		public int X { get; }

		/// <summary>
		/// Gets the Y coordinate of this piece of puzzle.
		/// </summary>
		public int Y { get; }

		/// <summary>
		/// Gets the rotation angle of this piece.
		/// </summary>
		public int RotationAngle { get; private set; }

		public PuzzlePiece(int x, int y)
		{
			X = x;
			Y = y;

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
