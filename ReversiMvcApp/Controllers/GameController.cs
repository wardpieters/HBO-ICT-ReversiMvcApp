using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ReversiMvcApp.Data;
using ReversiMvcApp.Helpers;
using ReversiMvcApp.Models;
using ReversiMvcApp.Services;

namespace ReversiMvcApp.Controllers
{
    public class GameController : Controller
    {
        private readonly ApiService _apiService;

        public GameController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: Game
        public async Task<IActionResult> Index()
        {
            var response = await _apiService.GetEmptyGames();

            foreach (var game in await response.GetData())
            {
                Console.WriteLine(game.Player1Token);
                Console.WriteLine(game.Player2Token);
                Console.WriteLine(game.Description);
            }

            return await this.ReturnViewOrError(response);
        }

        // GET: Game/Details/5
        [Authorize]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();
            
            var response = await _apiService.Join(id, User.GetId());

            ViewData["ApiUrl"] = _apiService.ApiUrl;

            return await this.ReturnViewOrError(response);
        }

        // GET: Game/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Game/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Description")] Game game)
        {
            if (ModelState.IsValid)
            {
                game.Player1Token = User.GetId();
                var response = await _apiService.CreateGame(game);
                var token = (await response.GetData()).Token;

                return this.ReturnViewOrError(response, Redirect("/game"), Redirect($"details/{token}"));
            }
            
            return View(game);
        }

        // GET: Game/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var response = await _apiService.GetGame(id);
            
            return await this.ReturnViewOrError(response);
        }

        // POST: Game/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Player1Token,Description")] Game game)
        {
            if (id != game.Token)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // _apiService.Update(game);
                    // await _apiService.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GameExists(game.Token))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(game);
        }

        // GET: Game/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // var game = await _apiService.Game
            //     .FirstOrDefaultAsync(m => m.Token == id);
            // if (game == null)
            // {
                // return NotFound();
            // }

            return View(new Game());
        }

        // POST: Game/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            // var game = await _apiService.Game.FindAsync(id);
            // _apiService.Game.Remove(game);
            // await _apiService.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("/game/leave")]
        public async Task<IActionResult> LeaveGame([FromForm] string token)
        {
            if (string.IsNullOrEmpty(token)) return BadRequest("Missing fields");

            var response = await _apiService.Leave(token, User.GetId());

            return this.ReturnViewOrError(response, Redirect($"/game/details/{token}"), Redirect("/game"));
        }

        private bool GameExists(string id)
        {
            return false;
        }
    }
}
