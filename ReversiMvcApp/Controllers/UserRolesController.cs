using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ReversiMvcApp.Data;
using ReversiMvcApp.Models;
using ReversiMvcApp.Services;

namespace ReversiMvcApp.Controllers
{
    [Authorize(Roles = "Admin,Moderator")]
    public class UserRolesController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly ApiService _apiService;
        private readonly ReversiDbContext _reversiContext;

        public UserRolesController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
            IEmailSender emailSender, ApiService apiService, ReversiDbContext reversiContext)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _emailSender = emailSender;
            _apiService = apiService;
            _reversiContext = reversiContext;
        }

        public async Task<IActionResult> Index()
        {
            ClaimsPrincipal currentUser = User;
            string currentUserId = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);

            var users = await _userManager.Users.ToListAsync();
            var userRolesViewModel = new List<UserRolesViewModel>();
            foreach (ApplicationUser user in users)
            {
                // User can't edit their own account
                if (user.Id.Equals(currentUserId)) continue;

                var thisViewModel = new UserRolesViewModel();
                thisViewModel.UserId = user.Id;
                thisViewModel.Email = user.Email;
                thisViewModel.FirstName = user.FirstName;
                thisViewModel.LastName = user.LastName;
                thisViewModel.Roles = await GetUserRoles(user);
                userRolesViewModel.Add(thisViewModel);
            }

            var tempData = TempData["AlertMessage"];
            if (tempData != null)
            {
                ViewBag.AlertMessage = JsonConvert.DeserializeObject<dynamic>(tempData.ToString());    
            }

            return View(userRolesViewModel);
        }

        private async Task<List<string>> GetUserRoles(ApplicationUser user)
        {
            var a = await _userManager.GetRolesAsync(user);
            return new List<string>(a);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Manage(string userId)
        {
            ViewBag.userId = userId;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with ID {userId} not found";
                return NotFound();
            }

            ViewBag.UserName = user.UserName;
            var model = new List<ManageUserRolesViewModel>();
            foreach (var role in _roleManager.Roles)
            {
                var userRolesViewModel = new ManageUserRolesViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name
                };

                userRolesViewModel.Selected = await _userManager.IsInRoleAsync(user, role.Name);
                model.Add(userRolesViewModel);
            }
            
            var tempData = TempData["AlertMessage"];
            if (tempData != null)
            {
                ViewBag.AlertMessage = JsonConvert.DeserializeObject<dynamic>(tempData.ToString());    
            }

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Manage(List<ManageUserRolesViewModel> model, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, roles);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing roles");
                return View(model);
            }

            result = await _userManager.AddToRolesAsync(user, model.Where(x => x.Selected).Select(y => y.RoleName));
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add selected roles to user");
                return View(model);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ViewUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }
            
            var tempData = TempData["AlertMessage"];
            if (tempData != null)
            {
                ViewBag.AlertMessage = JsonConvert.DeserializeObject<dynamic>(tempData.ToString());    
            }

            ModelState.AddModelError("", "Cannot remove user existing roles");
            return View(user);
        }

        public async Task<IActionResult> SendPasswordRestEmail(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new {area = "Identity", code},
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(
                user.Email,
                "Reset Password",
                $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>."
            );

            TempData["AlertMessage"] = JsonConvert.SerializeObject(new {message = "De e-mail is verstuurd.", type = "success"});
            return RedirectToAction("ViewUser", new { userId = user.Id});
        }

        public async Task<IActionResult> DisableTwoFactorForUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) {
                return NotFound();
            }

            user.TwoFactorEnabled = false;
            await _userManager.UpdateAsync(user);
            
            TempData["AlertMessage"] = JsonConvert.SerializeObject(new {message = "Tweefactorauthenticatie is uitgeschakeld.", type = "success"});

            return RedirectToAction("ViewUser", "UserRoles", new {userId});
        }

        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            ClaimsPrincipal currentUser = User;
            string currentUserId = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);
            var roles = await _userManager.GetRolesAsync(user);

            // Prevent user from deleting their own account
            if (!user.Id.Equals(currentUserId) && !roles.Contains(Roles.Admin.ToString()))
            {
                var apiResult = await _apiService.Delete(user.Id);

                if (!apiResult.HasError()) {
                    var result = await _userManager.DeleteAsync(user);
                    if (result.Succeeded) {
                        var player = _reversiContext.Players.FirstOrDefault(x => x.Guid.Equals(user.Id));
                        if (player != null)
                        {
                            _reversiContext.Players.Remove(player);
                            await _reversiContext.SaveChangesAsync();
                        }
                        
                        TempData["AlertMessage"] = JsonConvert.SerializeObject(new {message = "De gebruiker is succesvol verwijderd.", type = "success"});
                        return RedirectToAction("Index");
                    }
                }
            }

            TempData["AlertMessage"] = JsonConvert.SerializeObject(new {message = "Er is een fout opgetreden.", type = "danger"});
            return RedirectToAction("Index");
        }
    }
}