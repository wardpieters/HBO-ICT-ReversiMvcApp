﻿using System.Diagnostics;
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
            var response = await _apiService.GetGameByPlayer(currentUserId);
            var token = (await response.GetData()).Token;

            return this.ReturnViewOrError(response, Redirect("/game"), Redirect($"/game/details/{token}") );
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}
