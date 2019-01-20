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
        public virtual ICollection<Team> Teams { get; set; }
    }
}