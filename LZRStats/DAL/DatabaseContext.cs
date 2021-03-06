﻿using LZRStats.Models;
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
        public DbSet<PlayerStats> PlayerStats { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Game> Games { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Entity<Team>()
                .HasMany(x => x.Players)
                .WithRequired(x => x.Team)
                .HasForeignKey(x => x.TeamId)
                .WillCascadeOnDelete(false);

            //modelBuilder.Entity<Team>().HasMany(x => x.TeamGames).WithMany(x => x.Teams).Map(cs =>
            //{
            //    cs.MapLeftKey("TeamRefId");
            //    cs.MapRightKey("GameRefId");
            //    cs.ToTable("GameTeam");
            //});


            modelBuilder.Entity<TeamGame>().HasKey(q =>
                new {
                    q.TeamId,
                    q.GameId
                });

            // Relationships
            modelBuilder.Entity<TeamGame>()
                .HasRequired(t => t.Team)
                .WithMany(t => t.TeamGames)
                .HasForeignKey(t => t.TeamId);

            modelBuilder.Entity<TeamGame>()
                .HasRequired(t => t.Game)
                .WithMany(t => t.TeamGames)
                .HasForeignKey(t => t.GameId);
    

        modelBuilder.Entity<Player>()
                .HasMany(x => x.PlayerStats)
                .WithRequired(x => x.Player)
                .HasForeignKey(x => x.PlayerId);

            modelBuilder.Entity<Game>()
                .HasMany(x => x.PlayerStats)
                .WithRequired(x => x.Game)
                .HasForeignKey(x => x.GameId);
        }


    }
}