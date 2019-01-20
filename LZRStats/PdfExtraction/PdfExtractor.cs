using LZRStats.DAL;
using LZRStats.Models;
using Novacode;
using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LZRStats.PdfExtraction
{
    public class PdfExtractor
    {
        private static DatabaseContext db = new DatabaseContext();

        public static void ExtractFromPdf(string filePath)
        {
            DocX docx = DocX.Load(filePath);
            CreateDataFromFile(docx.Text);

        }

        public static void CreateDataFromFile(string data)
        {
            var withoutTabs = data.Replace('\t', ' ');
            var lines = withoutTabs.Split('\n');
            var gameData = lines[1].Replace("         ", " ").Split(' ');
            var teamName = gameData[3];
            var team = GetTeamForTeamName(teamName);
            var players = new List<Player>();
        }

        private static Team GetTeamForTeamName(string teamName)
        {
            var team = db.Teams.Where(t => t.Name.Equals(teamName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (team == null)
            {
                team = new Team();
                team.Name = teamName;
            }
            return team;
        }

        #region PlayerShootingStats
        private static void ExtractPlayerShootingStats(Player player, string[] playerShooting)
        {
            if (playerShooting[1].Length == 1)
            {
                player.FG2Made = 0;
                player.FG2Attempted = 0;
            }
            else
            {
                player.FG2Made = int.Parse(playerShooting[1].Split('/')[0]);
                player.FG2Attempted = int.Parse(playerShooting[1].Split('/')[1]);
            }
            if (playerShooting[2].Length == 1)
            {
                player.FG3Made = 0;
                player.FG3Attempted = 0;
            }
            else
            {
                player.FG3Made = int.Parse(playerShooting[2].Split('/')[0]);
                player.FG3Attempted = int.Parse(playerShooting[2].Split('/')[1]);
            }
            if (playerShooting[3].Length == 1)
            {
                player.FTMade = 0;
                player.FTAttempted = 0;
            }
            else
            {
                player.FTMade = int.Parse(playerShooting[3].Split('/')[0]);
                player.FTAttempted = int.Parse(playerShooting[3].Split('/')[1]);
            }
        }
        #endregion
    }
}