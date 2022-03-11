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
using ReversiMvcApp.Models;

namespace ReversiMvcApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ReversiDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ReversiDbContext context, ILogger<HomeController> logger)
        {
            _logger = logger;
            _context = context;
        }

        [Authorize]
        public async Task<string> Index()
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
                return "player added";
            }

            return "aaaa";
            //return View();
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