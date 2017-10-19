using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LZRStats.Models
{
    public class Player
    {
        public int Id { get; set; }
        public int TeamId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int JerseyNumber { get; set; }
        public int Points { get; set; }
        public int OffensiveRebounds { get; set; }
        public int DefensiveRebounds { get; set; }
        public int TotalRebounds { get; set; }
        public int Assists { get; set; }
        public int Steals { get; set; }
        public int Blocks { get; set; }
        public int FG2Attempted { get; set; }
        public int FG2Made { get; set; }
        public int FG3Attempted { get; set; }
        public int FG3Made { get; set; }
        public int FTAttempted { get; set; }
        public int FTMade { get; set; }
        public TimeSpan MinutesPlayed { get; set; }
        public int GamesPlayed { get; set; }
        public virtual Team Team { get; set; }
        public virtual List<Payment> Payments { get; set; }

    }
}