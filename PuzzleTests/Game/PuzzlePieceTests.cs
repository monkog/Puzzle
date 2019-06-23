using Microsoft.VisualStudio.TestTools.UnitTesting;
using Puzzle.Game;

namespace PuzzleTests.Game
{
	[TestClass]
	public class PuzzlePieceTests
	{
		private readonly int[] _availableAngles = { 0, 90, 180, 270 };

		private PuzzlePiece _unitUnderTest;

		[TestInitialize]
		public void Initialize()
		{
			const int x = 5;
			const int y = 2;
			_unitUnderTest = new PuzzlePiece(x, y);
		}

		[TestMethod]
		public void Ctor_ValidValues_PropertiesAssigned()
		{
			const int x = 5;
			const int y = 2;

			var unitUnderTest = new PuzzlePiece(x, y);

			Assert.AreEqual(x, unitUnderTest.X);
			Assert.AreEqual(y, unitUnderTest.Y);
			CollectionAssert.Contains(_availableAngles, unitUnderTest.RotationAngle);
		}

		[TestMethod]
		public void Rotate_NoParams_Rotated()
		{
			var previousAngle = _unitUnderTest.RotationAngle;

			_unitUnderTest.Rotate();

			Assert.AreNotEqual(previousAngle, _unitUnderTest.RotationAngle);
			CollectionAssert.Contains(_availableAngles, _unitUnderTest.RotationAngle);
		}
	}
}
