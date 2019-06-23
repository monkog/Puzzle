namespace Puzzle.Game
{
	public class HighScoreRecord
	{
		/// <summary>
		/// Gets the name of the player.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Gets the number of gathered points.
		/// </summary>
		public int Points { get; }

		/// <summary>
		/// Gets the game duration in seconds.
		/// </summary>
		public int Seconds { get; }

		public HighScoreRecord(string name, int points, int seconds)
		{
			Name = name;
			Points = points;
			Seconds = seconds;
		}
	}
}