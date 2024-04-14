using NBAInsightEngine.Models;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NBAInsightEngine.Services
{
    public class NbaApiService : INbaApiService
    {
        private readonly HttpClient _httpClient;

        // Constructor injects an HttpClient instance.
        public NbaApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // GetGameDataAsync makes a GET request to the NBA API to retrieve game data by the game ID.
        public async Task<GameData> GetGameDataAsync(string gameId)
        {
            // Assemble the request URL using the game ID.
            var response = await _httpClient.GetAsync($"https://cdn.nba.com/static/json/liveData/playbyplay/playbyplay_{gameId}.json");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            
            // Deserialize the JSON string into a GameData object and return it.
            return JsonConvert.DeserializeObject<GameData>(content);
        }
    }
}