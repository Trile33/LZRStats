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
        public int GamesPlayed { get; set; }
        public virtual Team Team { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
        public virtual ICollection<PlayerStats> PlayerStats { get; set; }
    }
}