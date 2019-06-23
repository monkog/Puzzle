using System.Windows.Controls.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Puzzle.Game;

namespace PuzzleTests.Game
{
	[TestClass]
	public class PuzzleCollectionTests
	{
		private PuzzleCollection _unitUnderTest;

		[TestInitialize]
		public void Initialize()
		{
			_unitUnderTest = new PuzzleCollection();
		}

		[TestMethod]
		public void GetPuzzlePiece_Thumb_PuzzlePiece()
		{
			var thumb = new Thumb();
			var puzzlePiece = new PuzzlePiece(2, 3);

			_unitUnderTest.Add(thumb, puzzlePiece);
			var result = _unitUnderTest[thumb];

			Assert.AreEqual(puzzlePiece, result);
		}

		[TestMethod]
		public void GetPuzzlePiece_RowAndColumn_PuzzlePieceAndThumb()
		{
			const int row = 5;
			const int column = 9;
			var thumb = new Thumb();
			var puzzlePiece = new PuzzlePiece(column, row);

			_unitUnderTest.Add(thumb, puzzlePiece);
			var result = _unitUnderTest[row, column];

			Assert.AreEqual(thumb, result.Key);
			Assert.AreEqual(puzzlePiece, result.Value);
		}
	}
}
