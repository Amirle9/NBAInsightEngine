using Microsoft.AspNetCore.Mvc;
using NBAInsightEngine.Services;

namespace NBAInsightEngine.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly INbaApiService _nbaApiService;
        private const string GameId = "0022000180";

        public GamesController(INbaApiService nbaApiService)
        {
            _nbaApiService = nbaApiService;
        }

        [HttpGet("players")]
        public async Task<IActionResult> GetAllPlayersNames()
        {
            // get game data using the game ID.
            var gameData = await _nbaApiService.GetGameDataAsync(GameId);
            if (gameData == null)
            {
                return NotFound("Game data not found.");
            }

            var playerNamesGroupedByTeam = gameData.Game.Actions
                .Where(a => !string.IsNullOrEmpty(a.PlayerNameI) && a.TeamId.HasValue) // Ensure TeamId has value
                .GroupBy(a => a.TeamId.Value) // Use the value of TeamId for grouping
                .ToDictionary(
                    g => g.Key.ToString(), // Convert TeamId to string now that we know it's not null
                    g => g.Select(a => a.PlayerNameI).Distinct()
                );

            return Ok(playerNamesGroupedByTeam);
        }

        [HttpGet("players/{playerName}/actions")]
        public async Task<IActionResult> GetAllActionByPlayerName(string playerName)
        {
            var gameData = await _nbaApiService.GetGameDataAsync(GameId);
            if (gameData == null)
            {
                return NotFound("Game data not found.");
            }

            var lastName = playerName.Split(' ').Last().ToLower();
            // Filter the actions where the player is involved either by direct action or by assist, or is involved in a jump ball.
            var actionsByPlayer = gameData.Game.Actions
                .Where(a => string.Equals(a.PlayerNameI, playerName, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(a.AssistPlayerNameInitial, playerName, StringComparison.OrdinalIgnoreCase) ||
                            (a.FoulDrawnPlayerName != null && a.FoulDrawnPlayerName.ToLower().Contains(lastName)) ||
                            (a.JumpBallWonPlayerName != null && a.JumpBallWonPlayerName.ToLower().Contains(lastName)) ||
                            (a.JumpBallLostPlayerName != null && a.JumpBallLostPlayerName.ToLower().Contains(lastName)))
                // If the action is an assist by the player, label it as such.
                .Select(a => new
                {
                    ActionType = !string.IsNullOrEmpty(a.AssistPlayerNameInitial) && a.AssistPlayerNameInitial.ToLower() == playerName.ToLower() ? "assist" : a.ActionType,
                })
                .ToList();

            // If no actions are associated with the player, return a NotFound result.
            if (!actionsByPlayer.Any())
            {
                return NotFound($"Actions for {playerName} not found.");
            }

            return Ok(actionsByPlayer);
        }


        // Retrieves the total points, rebounds, and assists for each player grouped by their team.
        [HttpGet("totals")]
        public async Task<IActionResult> GetAllPlayersTotalsGroupedByTeam()
        {
            var gameData = await _nbaApiService.GetGameDataAsync(GameId);
            if (gameData == null)
            {
                return NotFound("Game data not found.");
            }

            // Group player names by their team ID.
            var playerNamesGroupedByTeam = gameData.Game.Actions
                .Where(a => !string.IsNullOrEmpty(a.PlayerNameI) && a.TeamId.HasValue)
                .GroupBy(a => a.TeamId.Value)
                .ToDictionary(g => g.Key, g => g.Select(a => a.PlayerNameI).Distinct());

            // Calculate the total stats for each player and group them by team.
            var allPlayersStats = playerNamesGroupedByTeam.ToDictionary(
                teamGroup => teamGroup.Key.ToString(),
                teamGroup => teamGroup.Value.Select(playerName =>
                {
                    var playerActions = gameData.Game.Actions
                        .Where(a => string.Equals(a.PlayerNameI, playerName, StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(a.AssistPlayerNameInitial, playerName, StringComparison.OrdinalIgnoreCase))
                        .ToList();


                    // last action with pointsTotal is the latest total points for the player.
                    var lastPointsAction = gameData.Game.Actions
                        .Where(a => a.PlayerNameI == playerName && a.PointsTotal.HasValue)
                        .OrderByDescending(a => a.ActionNumber)
                        .FirstOrDefault();

                    var totalPoints = lastPointsAction?.PointsTotal ?? 0;

                    // last action with reboundTotal is the latest total rebounds for the player.
                    var lastReboundAction = gameData.Game.Actions
                        .Where(a => a.PlayerNameI == playerName && a.ReboundTotal.HasValue)
                        .OrderByDescending(a => a.ActionNumber)
                        .FirstOrDefault();

                    var totalRebounds = lastReboundAction?.ReboundTotal ?? 0;

                    int totalAssists = playerActions
                        .Where(a => a.AssistPlayerNameInitial?.Equals(playerName, StringComparison.OrdinalIgnoreCase) == true)
                        .Count();

                    // Create an object containing the player's name and their total stats.
                    return new
                    {
                        PlayerName = playerName,
                        TotalPoints = totalPoints,
                        TotalRebounds = totalRebounds,
                        TotalAssists = totalAssists
                    };

                })
            // Sort by total points, then by total rebounds, and then by total assists
            .OrderByDescending(p => p.TotalPoints)
            .ThenByDescending(p => p.TotalRebounds)
            .ThenByDescending(p => p.TotalAssists)
            .ToList()
            .Select(p => $"{p.PlayerName}: PTS: {p.TotalPoints} | REB: {p.TotalRebounds} | AST: {p.TotalAssists}") // Project into the string format
            .ToList());

            return Ok(allPlayersStats);
        }
    }
}