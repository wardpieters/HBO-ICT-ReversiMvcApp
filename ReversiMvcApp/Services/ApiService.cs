using System;
using System.Collections.Generic;
using ReversiMvcApp.Helpers;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ReversiMvcApp.Models;

namespace ReversiMvcApp.Services
{
    public class ApiService
    {
        private readonly HttpClient _client;
        public readonly string ApiUrl;
        public readonly IConfiguration config;

        public ApiService(string uri)
        {
            ApiUrl = uri;
            _client = new HttpClient();
            _client.BaseAddress = new Uri(uri);
            config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();
        }

        private async Task<ResponseWrapper<T>> GetAs<T>(string path)
        {
            var response = await _client.GetAsync(path);
            return new ResponseWrapper<T>(response);
        }

        private async Task<ResponseWrapper<T>> Post<T>(string path, object content)
        {
            StringContent postContent = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(path, postContent);

            return new ResponseWrapper<T>(response);
        }

        private async Task<ResponseWrapper<T>> Put<T>(string path, object content)
        {
            var response = await _client.PutAsJsonAsync(path, content);
            return new ResponseWrapper<T>(response);
        }

        public async Task<ResponseWrapper<List<Game>>> GetEmptyGames()
        {
            return await GetAs<List<Game>>("game");
        }

        public async Task<ResponseWrapper<GameResponse>> GetGame(string token)
        {
            return await GetAs<GameResponse>($"game/{token}");
        }
        
        public async Task<ResponseWrapper<GameResponse>> GetGameByPlayer(string token)
        {
            return await GetAs<GameResponse>($"game/player-token/{token}");
        }
        
        public async Task<ResponseWrapper<GameResponse>> GetActiveGameByPlayer(string token)
        {
            return await GetAs<GameResponse>($"game/player-token/{token}/active");
        }

        public async Task<ResponseWrapper<GameResponse>> CreateGame(Game game)
        {
            return await Post<GameResponse>("game", game);
        }

        public async Task<ResponseWrapper<GameResponse>> Join(string token, string playerToken)
        {
            return await Put<GameResponse>($"game/{token}/join", playerToken);
        }
        
        public async Task<ResponseWrapper<GameResponse>> Leave(string token, string playerToken)
        {
            return await Put<GameResponse>($"game/{token}/leave", playerToken);
        }
        
        public async Task<ResponseWrapper<List<GameStatistic>>> Stats()
        {
            string apiToken = config.GetValue<string>("ReversiApiToken");
            return await GetAs<List<GameStatistic>>($"stats?apiKey={apiToken}");
        }
        
        public async Task<ResponseWrapper<string>> Delete(string playerToken)
        {
            string apiToken = config.GetValue<string>("ReversiApiToken");
            return await GetAs<string>($"player/delete?apiKey={apiToken}&playerToken={playerToken}");
        }
    }
}
