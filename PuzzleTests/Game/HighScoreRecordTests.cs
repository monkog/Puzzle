using Microsoft.VisualStudio.TestTools.UnitTesting;
using Puzzle.Game;

namespace PuzzleTests.Game
{
	[TestClass]
	public class HighScoreRecordTests
	{
		[TestMethod]
		public void Ctor_ValidParameters_PropertiesAssigned()
		{
			const string name = "Alex";
			const int points = 15;
			const int seconds = 11;

			var unitUnderTest = new HighScoreRecord(name, points, seconds);

			Assert.AreEqual(name, unitUnderTest.Name);
			Assert.AreEqual(points, unitUnderTest.Points);
			Assert.AreEqual(seconds, unitUnderTest.Seconds);
		}
	}
}
