using Microsoft.VisualStudio.TestTools.UnitTesting;
using Puzzle.Game;

namespace PuzzleTests.Game
{
	[TestClass]
	public class HighScoresTests
	{
		[TestMethod]
		public void Ctor_ValidParameters_PropertiesInitialized()
		{
			var unitUnderTest = new HighScores();

			Assert.IsNotNull(unitUnderTest.EasyHighScores);
			Assert.IsNotNull(unitUnderTest.MediumHighScores);
			Assert.IsNotNull(unitUnderTest.HardHighScores);
		}
	}
}
