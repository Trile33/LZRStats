using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LZRStats.Models
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int NumberOfWins { get; set; }
        public int NumberOfLoses { get; set; }
        public virtual ICollection<Player> Players { get; set; }
        public virtual ICollection<TeamGame> TeamGames { get; set; }
    }
}