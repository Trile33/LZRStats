using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LZRStats.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public virtual int PlayerId { get; set; }
        public decimal Debt { get; set; }
        public decimal Payed { get; set; }
        public string Month { get; set; }
        public Player Player { get; set; }
    }
}