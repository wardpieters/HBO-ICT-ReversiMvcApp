using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ReversiMvcApp.Data;
using ReversiMvcApp.Helpers;
using ReversiMvcApp.Models;
using ReversiMvcApp.Services;

namespace ReversiMvcApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ReversiDbContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly ApiService _apiService;

        public HomeController(ReversiDbContext context, ILogger<HomeController> logger, ApiService apiService)
        {
            _logger = logger;
            _context = context;
            _apiService = apiService;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            ClaimsPrincipal currentUser = User;
            string currentUserId = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Get player form database by current user id 
            var currentPlayer = _context.Players.FirstOrDefault(p => p.Guid == currentUserId);
            if (currentPlayer == null)
            {
                // Create player in database
                Player newPlayer = new Player();
                newPlayer.Guid = currentUserId;
                newPlayer.Name = currentUser.FindFirstValue(ClaimTypes.Name);;
                _context.Players.Add(newPlayer);
                await _context.SaveChangesAsync();
            }
            
            // Player is created
            var response = await _apiService.GetActiveGameByPlayer(currentUserId);
            Game currentGame = await response.GetData();

            if (currentGame.GameFinished)
            {
                return Redirect("/game");
            }

            return await this.ReturnViewOrError(response, Redirect("/game"), Redirect($"/game/details/{currentGame.Token}") );
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Stats()
        {
            var response = await _apiService.Stats();
            List<GameStatistic> stats = await response.GetData();
            
            var playerData = _context.Players.ToList();

            foreach (Player player in playerData)
            {
                player.GamesWon = stats.Count(x => x.GameWinnerToken == player.Guid);
                player.GamesLost = stats.Count(x => (x.Player1Token == player.Guid || x.Player2Token == player.Guid) && !String.IsNullOrEmpty(x.GameWinnerToken) && !x.GameWinnerToken.Equals(player.Guid));
                player.GamesDraw = stats.Count(x => (x.Player1Token == player.Guid || x.Player2Token == player.Guid) && x.GameWinnerToken == null);
            }
            
            ViewBag.StatsData = stats;
            ViewBag.PlayerData = playerData;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}
