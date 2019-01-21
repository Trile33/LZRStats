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
            List<string> gameData = GetFormattedGameData(lines[1]);
            var team = GetTeam(gameData);
            var opponent = GetOpponent(gameData);
            var gamePlayedOn = GetGameDate(gameData);
            var game = CreateGameDataFromFile(gameData, team, opponent, gamePlayedOn.Value);

            List<string> removeEmpty = CreatEmptyLinesList(lines);
            int playersCount = (removeEmpty.Count - 4) / 2;
            var finalData = removeEmpty.ToArray();
            CreatePlayerStats(team, playersCount, finalData, game);
            //TODO - update team win/loss stats
            db.SaveChanges();

            return errors.Count > 0 ? errors : null;
        }

        private static List<string> CreatEmptyLinesList(string[] lines)
        {
            var removeEmpty = new List<string>();
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (!trimmedLine.Equals(""))
                    removeEmpty.Add(trimmedLine);
            }

            return removeEmpty;
        }

        private static string[] GetFileDataLines(string filePath)
        {
            DocX docx = DocX.Load(filePath);
            string data = docx.Text;
            var withoutTabs = data.Replace('\t', ' ');
            var lines = withoutTabs.Split('\n');
            return lines;
        }

        private static void CreatePlayerStats(Team team, int playersCount, string[] finalData, Game game)
        {
            var counter = 0;
            var i = 2;
            while (counter < playersCount)
            {
                UpdatePlayerStats(team, finalData, game, i);

                i += 2;
                counter++;
            }
        }

        private static void UpdatePlayerStats(Team team, string[] finalData, Game game, int i)
        {
            var playerStats = finalData[i].Split(' ');
            var temp = playerStats.Where(x => x.Length > 0);
            playerStats = temp.ToArray();
            int jerseyNo = int.Parse(playerStats[0]);
            string firstName = playerStats[1];
            string lastName = playerStats[2];
            var dbPlayer = db.Players.Where(x => x.TeamId == team.Id && x.LastName == lastName && x.FirstName == firstName && x.JerseyNumber == jerseyNo)
                .SingleOrDefault();
            var player = dbPlayer ?? new Player();
            if (dbPlayer == null)
            {
                player = CreateNewPlayer(team, jerseyNo, firstName, lastName);
            }

            var stats = CreatePlayerStats(game, playerStats, player);
            player.PlayerStats = new List<PlayerStats>() { stats };

            player.GamesPlayed++;
            player.TeamId = team.Id;

            db.Players.Add(player);
        }

        private static Player CreateNewPlayer(Team team, int jerseyNo, string firstName, string lastName)
        {
            var player = new Player();
            player.FirstName = firstName;
            player.LastName = lastName;
            player.JerseyNumber = jerseyNo;
            player.Team = team;

            return player;
        }

        private static PlayerStats CreatePlayerStats(Game game, string[] playerStats, Player player)
        {
            return new PlayerStats
            {
                MinutesPlayed = GetStat(playerStats[3], playerStats[3].ElementAt(0)),
                Efficiency = GetStat(playerStats[4], playerStats[4].ElementAt(0)), //TODO - read negative efficiency (probably bad - sign)
                FG2Attempted = GetShootingStat(playerStats[6], playerStats[6].ElementAt(0), 1),
                FG2Made = GetShootingStat(playerStats[6], playerStats[6].ElementAt(0), 0),
                FG3Attempted = GetShootingStat(playerStats[7], playerStats[7].ElementAt(0), 1),
                FG3Made = GetShootingStat(playerStats[7], playerStats[7].ElementAt(0), 0),
                FTAttempted = GetShootingStat(playerStats[8], playerStats[8].ElementAt(0), 1),
                FTMade = GetShootingStat(playerStats[8], playerStats[8].ElementAt(0), 0),
                OffensiveRebounds = GetStat(playerStats[10], playerStats[10].ElementAt(0)),
                DefensiveRebounds = GetStat(playerStats[11], playerStats[11].ElementAt(0)),
                Assists = GetStat(playerStats[12], playerStats[12].ElementAt(0)),
                Turnovers = GetStat(playerStats[13], playerStats[13].ElementAt(0)),
                Steals = GetStat(playerStats[14], playerStats[14].ElementAt(0)),
                Blocks = GetStat(playerStats[15], playerStats[15].ElementAt(0)),
                Points = GetStat(playerStats[16], playerStats[16].ElementAt(0)),
                GameId = game.Id,
                Player = player
            };
        }

        private static int GetStat(string stat, char element)
        {
            if (!char.IsNumber(element))
                return 0;
            else
                return int.Parse(stat);
        }

        private static int GetShootingStat(string stat, char element, int substringNo)
        {
            if (!char.IsNumber(element))
                return 0;
            else
                return int.Parse(stat.Split('/')[substringNo]);
        }

        #region CreateGameData
        public static Game CreateGameDataFromFile(List<string> gameData, Team team, Team opponent, DateTime gamePlayedOn)
        {
            bool teamStatsImportedForThisGame = team.Games?.Any(x => x.PlayedOn == gamePlayedOn) ?? false;
            bool opponentStatsImportedForThisGame = opponent?.Games?.Any(x => x.PlayedOn == gamePlayedOn) ?? false;
            if (teamStatsImportedForThisGame)
                return null;
            //return $" { team.Name } statistics for game played on {gamePlayedOn.Value.ToLongDateString()} have already been imported!";
            var game = GetOrCreateGame(team, opponent, gamePlayedOn, opponentStatsImportedForThisGame);

            return game;
        }
        #endregion

        #region GetTeam
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
            List<string> gameData = ReplaceBadMinusCharacter(gameDataTemp);
            return gameData;
        }

        private static List<string> ReplaceBadMinusCharacter(string[] gameDataTemp)
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
    }
}