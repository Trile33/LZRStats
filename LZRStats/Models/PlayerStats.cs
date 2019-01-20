using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LZRStats.Models
{
    public class PlayerStats
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public int GameId { get; set; }
        public int Points { get; set; }
        public int OffensiveRebounds { get; set; }
        public int DefensiveRebounds { get; set; }
        public int TotalRebounds { get; set; }
        public int Assists { get; set; }
        public int Steals { get; set; }
        public int Blocks { get; set; }
        public int Turnovers { get; set; }
        public int Efficiency { get; set; }
        public int FG2Attempted { get; set; }
        public int FG2Made { get; set; }
        public int FG3Attempted { get; set; }
        public int FG3Made { get; set; }
        public int FTAttempted { get; set; }
        public int FTMade { get; set; }
        public int MinutesPlayed { get; set; }
        public virtual Player Player { get; set; }
        public virtual Game Game { get; set; }
    }
}