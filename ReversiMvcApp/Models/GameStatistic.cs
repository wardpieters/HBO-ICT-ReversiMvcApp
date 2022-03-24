using System.ComponentModel.DataAnnotations;

namespace ReversiMvcApp.Models
{
    public class GameStatistic
    {
        public string GameToken { get; set; }
        public string Player1Token { get; set; }
        public string Player2Token { get; set; }
        public string GameWinnerToken { get; set; }
        
        public int GamesWon { get; set; }
        public int GamesLost { get; set; }
        public int GamesDraw { get; set; }
    }
}