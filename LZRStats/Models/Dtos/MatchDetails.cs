using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LZRStats.Models.Dtos
{
    public class MatchDetails
    {
        public int Round { get; set; }
        public int MatchNumber { get; set; }
        public DateTime? PlayedOn { get; set; }
    }
}