
using Puzzle.Game;

namespace Puzzle.HelperClasses
{
    public class GameDetails
    {
		/// <summary>
		/// Gets the current game difficulty.
		/// </summary>
		public Difficulty Difficulty { get; }

        public char gameLevel { get; set; }
        public int gameMaxCounter { get; set; }
        public int currentGameCounter { get; set; }
    }
}
