using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls.Primitives;

namespace Puzzle.Game
{
	public class PuzzleCollection
	{
		private readonly Dictionary<Thumb, PuzzlePiece> _puzzlePieces;

		public PuzzleCollection()
		{
			_puzzlePieces = new Dictionary<Thumb, PuzzlePiece>();
		}

		/// <summary>
		/// Adds the given puzzle piece for given thumb.
		/// </summary>
		/// <param name="thumb">Thumb corresponding to the puzzle piece.</param>
		/// <param name="puzzlePiece">Puzzle piece for the thumb.</param>
		public void Add(Thumb thumb, PuzzlePiece puzzlePiece)
		{
			_puzzlePieces.Add(thumb, puzzlePiece);
		}

		/// <summary>
		/// Gets the puzzle piece corresponding to the given thumb.
		/// </summary>
		/// <param name="thumb">Thumb that the puzzle piece is looked for.</param>
		/// <returns>Puzzle piece found for the thumb.</returns>
		public PuzzlePiece this[Thumb thumb] => _puzzlePieces[thumb];

		/// <summary>
		/// Gets the puzzle piece and thumb for given row and column combination.
		/// </summary>
		/// <param name="row">Row of the searched puzzle.</param>
		/// <param name="column">Column of the searched puzzle.</param>
		/// <returns>Pair of thumb and puzzle piece fitting in the given row and column.</returns>
		public KeyValuePair<Thumb, PuzzlePiece> this[int row, int column] => _puzzlePieces.Single(p => p.Value.Row == row && p.Value.Column == column);
	}
}
