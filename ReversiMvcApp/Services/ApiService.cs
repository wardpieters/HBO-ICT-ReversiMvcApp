using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ReversiMvcApp.Helpers;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ReversiMvcApp.Data;
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

        public async Task<ResponseWrapper<Game>> GetGame(string token)
        {
            return await GetAs<Game>($"game/{token}");
        }
        
        public async Task<ResponseWrapper<Game>> GetGameByPlayer(string token)
        {
            return await GetAs<Game>($"game/player-token/{token}");
        }

        public async Task<ResponseWrapper<Game>> CreateGame(Game game)
        {
            return await Post<Game>("game", game);
        }

        public async Task<ResponseWrapper<Game>> Join(string token, string playerToken)
        {
            return await Put<Game>($"game/{token}/join", playerToken);
        }
        
        public async Task<ResponseWrapper<Game>> Leave(string token, string playerToken)
        {
            return await Put<Game>($"game/{token}/leave", playerToken);
        }
        
        public async Task<ResponseWrapper<List<GameStatistic>>> Stats()
        {
            string apiToken = config.GetValue<string>("ReversiApiToken");
            return await GetAs<List<GameStatistic>>($"stats?apiKey={apiToken}");
        }
    }
}
