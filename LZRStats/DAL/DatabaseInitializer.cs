using LZRStats.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LZRStats.DAL
{
    public class DatabaseInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<DatabaseContext>
    {
        protected override void Seed(DatabaseContext context)
        {
            //var players = new List<Player>
            //{
            //new Player{FirstName="temp",LastName="temp", Assists =2 , Blocks =0, DefensiveRebounds = 3, FG2Attempted = 5, FG2Made = 3, FG3Attempted = 9, FG3Made=4, FTAttempted = 4,
            // FTMade = 4, GamesPlayed=1, JerseyNumber = 33, MinutesPlayed = new TimeSpan(0,24,12), OffensiveRebounds = 0,
            //    Payments =  new List<Payment> (){ new Payment() {  Debt = 1000, Month = "September", Payed = 1000m, } }
            // , Points = 12, Steals = 2, TotalRebounds =3 }

            //};


            //players.ForEach(s => context.Players.Add(s));
            //context.SaveChanges();

            //var teams = new List<Team>
            //{
            //    new Team {Name = "Skywalkers", NumberOfLoses = 0, NumberOfWins = 1 }
            //};
            //teams.ForEach(t => context.Teams.Add(t));
            //context.SaveChanges();

            //var games = new List<Game>
            //{
            //    new Game { FirstTeam = teams.First(), SecondTeam = teams.First()}
            //};

            //games.ForEach(g => context.Games.Add(g));
            //context.SaveChanges();

        }
    }
}