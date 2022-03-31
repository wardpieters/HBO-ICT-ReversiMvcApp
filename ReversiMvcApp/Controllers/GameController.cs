using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using ReversiMvcApp.Helpers;
using ReversiMvcApp.Models;
using ReversiMvcApp.Services;

namespace ReversiMvcApp.Controllers
{
    [Authorize]
    public class GameController : Controller
    {
        private readonly ApiService _apiService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public GameController(ApiService apiService, IWebHostEnvironment webHostEnvironment)
        {
            _apiService = apiService;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Game
        public async Task<IActionResult> Index()
        {
            var response = await _apiService.GetEmptyGames();

            try
            {
                var tempData = TempData["AlertMessage"];
                if (tempData != null)
                {
                    Console.WriteLine(tempData.ToString());
                    ViewBag.AlertMessage = JsonConvert.DeserializeObject<dynamic>(tempData.ToString());    
                }
            }
            catch (Exception) {}

            return await this.ReturnViewOrError(response);
        }

        // GET: Game/Details/5
        
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();
            
            var response = await _apiService.Join(id, User.GetId());

            ViewData["ApiUrl"] = _webHostEnvironment.IsProduction() ? "/api" : _apiService.ApiUrl;
            
            ClaimsPrincipal currentUser = User;
            ViewData["CurrentUserId"] = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);

            return await this.ReturnViewOrError(response);
        }

        // GET: Game/Create
        
        public IActionResult Create()
        {
            return View();
        }

        // POST: Game/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Description")] Game game)
        {
            if (ModelState.IsValid)
            {
                game.Player1Token = User.GetId();
                var response = await _apiService.CreateGame(game);
                var responseData = await response.GetData();
                var gameResponse = responseData.Game;
                
                string gameToken = String.Empty;

                if (gameResponse != null)
                {
                    gameToken = gameResponse.Token;
                }

                return await this.ReturnViewOrError(response, Redirect("/game"), Redirect($"details/{gameToken}"));
            }
            
            return View(game);
        }

        [HttpPost("/game/leave")]
        
        public async Task<IActionResult> LeaveGame([FromForm] string token)
        {
            if (string.IsNullOrEmpty(token)) return BadRequest("Missing fields");

            var response = await _apiService.Leave(token, User.GetId());

            return await this.ReturnViewOrError(response, Redirect($"/game/details/{token}"), Redirect("/game"));
        }
    }
}
