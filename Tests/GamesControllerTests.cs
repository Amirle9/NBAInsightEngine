using Microsoft.AspNetCore.Mvc;
using Moq;
using NBAInsightEngine.Controllers;
using NBAInsightEngine.Models;
using NBAInsightEngine.Services;
using Action = NBAInsightEngine.Models.Action;

namespace Tests
{
    public class GamesControllerTests
    {
        [Fact]
        public async Task GetAllPlayersNames_ReturnsOkObjectResultWithNames()
        {
            // Arrange
            var mockService = new Mock<INbaApiService>();
            var gameId = "0022000180";
            var gameData = new GameData
            {
                Game = new Game
                {
                    Actions = new List<Action>
                    {
                        new Action { PlayerNameI = "Player1", TeamId = 123 },
                        new Action { PlayerNameI = "Player2", TeamId = 123 },
                        new Action { PlayerNameI = "Player3", TeamId = 456 },
                    }
                }
            };
            // Mocking the GetGameDataAsync method to return our game data.
            mockService.Setup(s => s.GetGameDataAsync(gameId)).ReturnsAsync(gameData);
            var controller = new GamesController(mockService.Object);

            // Act
            var result = await controller.GetAllPlayersNames();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<Dictionary<string, IEnumerable<string>>>(okResult.Value);
            Assert.True(returnValue.ContainsKey("123"));
            Assert.True(returnValue.ContainsKey("456"));
            Assert.Contains("Player1", returnValue["123"]);
            Assert.Contains("Player2", returnValue["123"]);
            Assert.Contains("Player3", returnValue["456"]);
        }

        [Fact]
        public async Task GetAllActionByPlayerName_ReturnsOkObjectResultWithActions()
        {
            // Arrange
            var mockService = new Mock<INbaApiService>();
            var gameId = "0022000180";
            var playerName = "Player1";
            var gameData = new GameData
            {
                Game = new Game
                {
                    Actions = new List<Action>
                    {
                        new Action { PlayerNameI = playerName, ActionType = "foul" },
                        new Action { PlayerNameI = "Player2", ActionType = "substitution" },
                        new Action { PlayerNameI = playerName, ActionType = "3pt" },
                    }
                }
            };

            mockService.Setup(s => s.GetGameDataAsync(gameId)).ReturnsAsync(gameData);
            var controller = new GamesController(mockService.Object);

            // Act
            var result = await controller.GetAllActionByPlayerName(playerName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actions = Assert.IsAssignableFrom<IEnumerable<dynamic>>(okResult.Value);
            var projectedActions = actions.Select(a => new { ActionType = a.GetType().GetProperty("ActionType").GetValue(a) }).ToList();

            Assert.Equal(2, projectedActions.Count); // Check if there are exactly 2 actions for Player1

            // Check for action types using the projected type
            Assert.Contains(projectedActions, a => a.ActionType.Equals("foul"));
            Assert.Contains(projectedActions, a => a.ActionType.Equals("3pt"));
        }


        [Fact]
        public async Task GetAllPlayersTotalsGroupedByTeam_ReturnsOkObjectResultWithTotals()
        {
            // Arrange
            var mockService = new Mock<INbaApiService>();
            var gameId = "0022000180";
            var gameData = new GameData
            {
                Game = new Game
                {
                    Actions = new List<Action>
                    {
                        new Action { ActionNumber = 2, PlayerNameI = "Player1", TeamId = 123, PointsTotal = 2, AssistPlayerNameInitial = "Player2" },
                        new Action { ActionNumber = 3, PlayerNameI = "Player1", TeamId = 123, ReboundTotal = 1 },
                        new Action { ActionNumber = 4, PlayerNameI = "Player1", TeamId = 123, PointsTotal = 5 },
                        new Action { ActionNumber = 5, PlayerNameI = "Player3", TeamId = 456, PointsTotal = 3, AssistPlayerNameInitial = "Player2" },
                        new Action { ActionNumber = 6, PlayerNameI = "Player2", TeamId = 123, PointsTotal = 2 },
                        new Action { ActionNumber = 7, PlayerNameI = "Player2", TeamId = 123, ReboundTotal = 2 },
                        new Action { ActionNumber = 8, PlayerNameI = "Player2", TeamId = 123, PointsTotal = 3, AssistPlayerNameInitial = "Player1" },
                        new Action { ActionNumber = 9, PlayerNameI = "Player3", TeamId = 456, PointsTotal = 5 },
                        new Action { ActionNumber = 10, PlayerNameI = "Player3", TeamId = 456, ReboundTotal = 1, AssistPlayerNameInitial = "Player2" },
                    }
                }
            };

            mockService.Setup(s => s.GetGameDataAsync(gameId)).ReturnsAsync(gameData);
            var controller = new GamesController(mockService.Object);

            // Act
            var result = await controller.GetAllPlayersTotalsGroupedByTeam();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<Dictionary<string, List<string>>>(okResult.Value);

            // Test for Team 123
            Assert.Contains("123", returnValue.Keys);
            var team123Stats = returnValue["123"];
            Assert.Contains(team123Stats, s => s.Contains("Player1: PTS: 5 | REB: 1 | AST: 1"));
            Assert.Contains(team123Stats, s => s.Contains("Player2: PTS: 3 | REB: 2 | AST: 3"));

            // Test for Team 456
            Assert.Contains("456", returnValue.Keys);
            var team456Stats = returnValue["456"];
            Assert.Contains(team456Stats, s => s.Contains("Player3: PTS: 5 | REB: 1 | AST: 0"));
        }


    }
}
