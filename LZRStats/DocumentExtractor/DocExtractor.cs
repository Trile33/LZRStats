using LZRStats.DAL;
using LZRStats.Models;
using LZRStats.Models.Dtos;
using Novacode;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace LZRStats.DocumentExtractor
{
    public class DocExtractor
    {
        private static DatabaseContext db = new DatabaseContext();
        private static string teamName;

        public static List<string> ExtractFromFile(string filePath, string fileName)
        {


            List<string> errors = new List<string>();
            string[] lines = GetFileDataLines(filePath);
            List<string> gameData = GetFormattedGameData(lines[1]);
            var gamePlayedOn = GetGameDate(gameData);
            MatchDetails matchDetails = GetMatchDetails(fileName, gamePlayedOn);

            string gameTotals = lines.ToList().Find(x => x.Contains("TOTAL"));
            List<string> formattedStats = CreatEmptyLinesList(lines);
            var team = GetTeam(gameData);
            var opponent = GetOpponent(gameData);
            var game = CreateGameDataFromFile(gameData, gameTotals, team, opponent, gamePlayedOn.Value, matchDetails);
            if (game == null)
            {
                errors.Add("File already imported.");
                return errors;
            }

            int playersCount = (formattedStats.Count - 4) / 2;
            var finalData = formattedStats.ToArray();
            CreatePlayerStats(team, playersCount, finalData, game);

            db.SaveChanges();

            return errors.Count > 0 ? errors : null;
        }

        private static MatchDetails GetMatchDetails(string fileName, DateTime? gamePlayedOn)
        {
            var matchRoundAndNumberInfo = fileName.Split('-');
            int round = int.Parse(matchRoundAndNumberInfo[0]);
            int matchNumber = int.Parse(matchRoundAndNumberInfo[1]);
            var matchDetails = new MatchDetails
            {
                MatchNumber = matchNumber,
                Round = round,
                PlayedOn = gamePlayedOn
            };
            return matchDetails;
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
        public static Game CreateGameDataFromFile(List<string> gameData, string gameTotals, Team team, Team opponent, DateTime gamePlayedOn, MatchDetails matchDetails)
        {
            var totals = gameTotals.Split(new char[] { ' ' }, options: StringSplitOptions.RemoveEmptyEntries);
            int pointsScored = int.Parse(totals[totals.Length - 1]);
            bool teamStatsImportedForThisGame = team.TeamGames?
                .Any(x => x.Game.PlayedOn.Date == gamePlayedOn.Date || (x.Game.MatchNumber == matchDetails.MatchNumber && x.Game.Round == matchDetails.Round)) ?? false;
            if (teamStatsImportedForThisGame)
                return null;

            bool opponentStatsImportedForThisGame = opponent?.TeamGames?
                .Any(x => x.Game.PlayedOn.Date == gamePlayedOn.Date || (x.Game.MatchNumber == matchDetails.MatchNumber && x.Game.Round == matchDetails.Round)) ?? false;

           
            if (opponentStatsImportedForThisGame)
            {
                UpdateWinLossStats(team, opponent, gamePlayedOn, pointsScored, matchDetails);
            }
            var game = GetOrCreateGame(team, opponent, opponentStatsImportedForThisGame, pointsScored, matchDetails);

            return game;
        }

        private static void UpdateWinLossStats(Team team, Team opponent, DateTime gamePlayedOn, int pointsScored, MatchDetails matchDetails)
        {
            //TODO compare with match round and number
            var opponentPointsScored = opponent.TeamGames.SingleOrDefault(x => x.Game.PlayedOn.Date == gamePlayedOn.Date || (x.Game.MatchNumber == matchDetails.MatchNumber && x.Game.Round == matchDetails.Round))?.PointsScored;
            if (opponentPointsScored.HasValue)
            {
                bool hasWon = pointsScored > opponentPointsScored.Value;
                if (hasWon)
                {
                    team.NumberOfWins++;
                    opponent.NumberOfLoses++;
                }
                else
                {
                    team.NumberOfLoses++;
                    opponent.NumberOfWins++;
                }
            }
        }
        #endregion

        #region GetTeam
        private static Game GetOrCreateGame(Team team, Team opponent, bool opponentStatsImportedForThisGame, int pointsScored, MatchDetails matchDetails)
        {
            Game game;
            if (opponentStatsImportedForThisGame)
            {
                //TODO compare with match round and number
                game = opponent.TeamGames.SingleOrDefault(x => x.Game.PlayedOn.Date == matchDetails.PlayedOn.Value.Date || (x.Game.MatchNumber == matchDetails.MatchNumber && x.Game.Round == matchDetails.Round)).Game;
                game.TeamGames.Add(new TeamGame
                {
                    Team = team,
                    PointsScored = pointsScored
                });
            }
            else
            {
                game = new Game
                {
                    TeamGames = new List<TeamGame>
                    {
                        new TeamGame
                        {
                            Team = team,
                            PointsScored = pointsScored
                        }
                    },
                    PlayedOn = matchDetails.PlayedOn.Value,
                    Round = matchDetails.Round,
                    MatchNumber = matchDetails.MatchNumber
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
            int startIndex = gameData.IndexOf("[vs");
            int endIndex = gameData.IndexOf(gameData.LastOrDefault());
            gameData[endIndex] = gameData[endIndex].Split(']')[0];
            var teamNameLines = gameData.GetRange(startIndex + 1, endIndex - startIndex);
            string opponentName = "";
            foreach (var line in teamNameLines)
            {
                opponentName += teamNameLines.IndexOf(line) == teamNameLines.Count - 1 ? line : line + " ";
            }
            return opponentName;
        }

        private static string GetTeamName(List<string> gameData)
        {
            var teamNameStartIndex = gameData.IndexOf("-");
            var teamNameEndIndex = gameData.IndexOf("-", 2);
            var teamNameLines = gameData.GetRange(teamNameStartIndex + 1, teamNameEndIndex - 1);

            string teamName = "";
            if (teamNameLines[0].Length == 1)
                teamNameLines.ForEach(x => teamName += x);
            else
            {
                foreach (string line in teamNameLines)
                {
                    //TODO remove whitespaces from team name
                    string temp = line.Replace(" ", "");
                    teamName += teamNameLines.IndexOf(line) == teamNameLines.Count - 1 ? temp : temp + " ";
                }
            }

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