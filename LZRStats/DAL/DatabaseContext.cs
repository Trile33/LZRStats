using LZRStats.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace LZRStats.DAL
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext() : base("LZRStatistics")
        {

        }
        public DbSet<Player> Players { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Game> Games { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Entity<Team>().HasMany(x => x.Players).WithRequired(x => x.Team).HasForeignKey(x => x.TeamId).WillCascadeOnDelete(false);
            modelBuilder.Entity<Team>().HasMany(x => x.Games).WithMany(x => x.Teams).Map(cs =>
            {
                cs.MapLeftKey("TeamRefId");
                cs.MapRightKey("GameRefId");
                cs.ToTable("GameTeam");
            });
        }
    }
}