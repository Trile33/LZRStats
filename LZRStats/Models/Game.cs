using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LZRStats.Models
{
    public class Game
    {
        public int Id { get; set; }
        public DateTime PlayedOn { get; set; }
        public int Round { get; set; }
        public int MatchNumber { get; set; }
        public virtual ICollection<TeamGame> TeamGames { get; set; }
        public virtual ICollection<PlayerStats> PlayerStats { get; set; }
    }
}