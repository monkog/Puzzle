using Microsoft.VisualStudio.TestTools.UnitTesting;
using Puzzle.Game;

namespace PuzzleTests.Game
{
	[TestClass]
	public class GameDetailsTests
	{
		[DataTestMethod]
		[DataRow(Difficulty.Easy, 3, 4, 146)]
		[DataRow(Difficulty.Medium, 6, 8, 71)]
		[DataRow(Difficulty.Hard, 8, 12, 46)]
		public void Ctor_Difficulty_PropertiesAssigned(Difficulty difficulty, int rows, int columns, int puzzleSize)
		{
			var unitUnderTest = new GameDetails(difficulty);

			Assert.AreEqual(difficulty, unitUnderTest.Difficulty);
			Assert.AreEqual(puzzleSize, unitUnderTest.PuzzleSize);
			Assert.AreEqual(rows, unitUnderTest.VerticalPuzzleCount);
			Assert.AreEqual(columns, unitUnderTest.HorizontalPuzzleCount);
			Assert.AreEqual(rows * columns, unitUnderTest.PuzzleCount);
			Assert.AreEqual(0, unitUnderTest.Points);
		}
	}
}
