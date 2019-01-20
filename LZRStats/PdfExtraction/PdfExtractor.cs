using LZRStats.DAL;
using LZRStats.Models;
using Novacode;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace LZRStats.PdfExtraction
{
    public class PdfExtractor
    {
        private static DatabaseContext db = new DatabaseContext();

        public static List<string> ExtractFromFile(string filePath)
        {
            List<string> errors = new List<string>();
            string[] lines = GetFileDataLines(filePath);
            string error = CreateGameDataFromFile(lines[1]);
            if (!string.IsNullOrEmpty(error))
                errors.Add(error);

            db.SaveChanges();

            return errors.Count > 0 ? errors : null;
        }

        private static string[] GetFileDataLines(string filePath)
        {
            DocX docx = DocX.Load(filePath);
            string data = docx.Text;
            var withoutTabs = data.Replace('\t', ' ');
            var lines = withoutTabs.Split('\n');
            return lines;
        }

        public static string CreateGameDataFromFile(string gameDataLine)
        {
            string error = null;
            List<string> gameData = GetFormattedGameData(gameDataLine);
            var team = GetTeam(gameData);
            var opponent = GetOpponent(gameData);
            var gamePlayedOn = GetGameDate(gameData);

            bool teamStatsImportedForThisGame = team.Games?.Any(x => x.PlayedOn == gamePlayedOn) ?? false;
            bool opponentStatsImportedForThisGame = opponent?.Games?.Any(x => x.PlayedOn == gamePlayedOn) ?? false;
            if (teamStatsImportedForThisGame)
                return $" { team.Name } statistics for game played on {gamePlayedOn.Value.ToLongDateString()} have already been imported!";
            var game = GetOrCreateGame(team, opponent, gamePlayedOn, opponentStatsImportedForThisGame);

            return error;
        }

        private static Game GetOrCreateGame(Team team, Team opponent, DateTime? gamePlayedOn, bool opponentStatsImportedForThisGame)
        {
            Game game;
            if (opponentStatsImportedForThisGame)
            {
                game = opponent.Games.SingleOrDefault(x => x.PlayedOn == gamePlayedOn);
                game.Teams.Add(team);
            }
            else
            {
                game = new Game
                {
                    Teams = new List<Team>
                    {
                        team
                    },
                    PlayedOn = gamePlayedOn.Value
                };
                db.Games.Add(game);
            }

            return game;
        }

        #region GetTeam
        private static Team GetOpponent(List<string> gameData)
        {
            string opponentName = GetOpposingTeamName(gameData);

            return db.Teams.Where(t => t.Name.Equals(opponentName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            //return GetTeamForTeamName(opponentName);
        }

        private static Team GetTeam(List<string> gameData)
        {
            string teamName = GetTeamName(gameData);
            return GetTeamForTeamName(teamName);
        }

        private static string GetOpposingTeamName(List<string> gameData)
        {
            string lastLine = gameData.LastOrDefault();
            string opponentName = lastLine.Substring(0, lastLine.Length - 1);
            return opponentName;
        }

        private static string GetTeamName(List<string> gameData)
        {
            var teamNameStartIndex = gameData.IndexOf("-");
            var teamNameEndIndex = gameData.IndexOf("-", 2);
            var teamNameLines = gameData.GetRange(teamNameStartIndex + 1, teamNameEndIndex - 1);
            string teamName = "";
            teamNameLines.ForEach(x => teamName += x);
            return teamName;
        }

        private static Team GetTeamForTeamName(string teamName)
        {
            var team = db.Teams.Where(t => t.Name.Equals(teamName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (team == null)
            {
                team = new Team
                {
                    Name = teamName
                };
                db.Teams.Add(team);
            }
            return team;
        }
        #endregion

        #region FormattingGameData
        private static List<string> GetFormattedGameData(string gameDataLine)
        {
            var gameDataTemp = gameDataLine.Replace("         ", " ").Split(new char[] { ' ' }, options: StringSplitOptions.RemoveEmptyEntries);
            List<string> gameData = FixBuggableChars(gameDataTemp);
            return gameData;
        }

        private static List<string> FixBuggableChars(string[] gameDataTemp)
        {
            var gameData = new List<string>();
            foreach (var line in gameDataTemp)
            {
                var newLine = line.Replace('‐', '-');
                gameData.Add(newLine);
            }

            return gameData;
        }
        #endregion

        #region GameDate
        private static DateTime? GetGameDate(List<string> gameData)
        {
            foreach (var line in gameData)
            {
                DateTime? playedOn = IsDate(line);
                if (playedOn.HasValue)
                    return playedOn.Value;
            }
            //TODO - handle bad date
            return null;
        }

        private static DateTime? IsDate(string line)
        {
            DateTime myDate;
            if (DateTime.TryParseExact(line, "dd-MMM-yyyy",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out myDate))
            {
                //String has Date and Time
                return myDate;
            }
            else
            {
                return null;
            }
        }
        #endregion

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