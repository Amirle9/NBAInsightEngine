using NBAInsightEngine.Models;

namespace NBAInsightEngine.Services
{
    public interface INbaApiService
    {
        Task<GameData> GetGameDataAsync(string gameId);
    }

}
