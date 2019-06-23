using Microsoft.VisualStudio.TestTools.UnitTesting;
using Puzzle.Game;

namespace PuzzleTests.Game
{
	[TestClass]
	public class HighScoresTests
	{
		private HighScores _unitUnderTest;

		[TestInitialize]
		public void Initialize()
		{
			_unitUnderTest = new HighScores();
		}

		[TestMethod]
		public void Ctor_ValidParameters_PropertiesInitialized()
		{
			var unitUnderTest = new HighScores();

			Assert.IsNotNull(unitUnderTest.EasyHighScores);
			Assert.IsNotNull(unitUnderTest.MediumHighScores);
			Assert.IsNotNull(unitUnderTest.HardHighScores);
		}

		[TestMethod]
		public void AddHighScore_Easy_AddedToEasyList()
		{
			var highScore = new HighScoreRecord("Alex", 4, 12);

			_unitUnderTest.Add(Difficulty.Easy, highScore);

			CollectionAssert.Contains(_unitUnderTest.EasyHighScores, highScore);
			CollectionAssert.DoesNotContain(_unitUnderTest.MediumHighScores, highScore);
			CollectionAssert.DoesNotContain(_unitUnderTest.HardHighScores, highScore);
		}

		[TestMethod]
		public void AddHighScore_Medium_AddedToMediumList()
		{
			var highScore = new HighScoreRecord("Alex", 4, 12);

			_unitUnderTest.Add(Difficulty.Medium, highScore);

			CollectionAssert.DoesNotContain(_unitUnderTest.EasyHighScores, highScore);
			CollectionAssert.Contains(_unitUnderTest.MediumHighScores, highScore);
			CollectionAssert.DoesNotContain(_unitUnderTest.HardHighScores, highScore);
		}

		[TestMethod]
		public void AddHighScore_Hard_AddedToMediumList()
		{
			var highScore = new HighScoreRecord("Alex", 4, 12);

			_unitUnderTest.Add(Difficulty.Hard, highScore);

			CollectionAssert.DoesNotContain(_unitUnderTest.EasyHighScores, highScore);
			CollectionAssert.DoesNotContain(_unitUnderTest.MediumHighScores, highScore);
			CollectionAssert.Contains(_unitUnderTest.HardHighScores, highScore);
		}
	}
}
