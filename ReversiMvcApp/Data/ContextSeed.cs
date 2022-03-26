using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using ReversiMvcApp.Models;

namespace ReversiMvcApp.Data
{
    public static class ContextSeed
    {
        public static void SeedData(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) {
            SeedRolesAsync(userManager, roleManager);
            SeedDefaultAdmin(userManager, roleManager);
        }

        private static void SeedRolesAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            //Seed Roles
            foreach (Roles role in Enum.GetValues(typeof(Roles)))
            {
                roleManager.CreateAsync(new IdentityRole(role.ToString())).Wait();
            }
        }

        private static void SeedDefaultAdmin(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            //Seed Default User
            var defaultUser = new ApplicationUser 
            {
                Email = "reversi-admin@ward.nl",
                UserName = "reversi-admin@ward.nl",
                FirstName = "Ward",
                LastName = "Pieters",
                EmailConfirmed = true, 
                PhoneNumberConfirmed = true 
            };

            var user = userManager.FindByEmailAsync(defaultUser.Email).Result;
            if (user == null)
            {
                userManager.CreateAsync(defaultUser, "123Pa$$word.").Wait();
                userManager.AddToRoleAsync(defaultUser, Roles.Admin.ToString()).Wait();
            }
            else
            {
                userManager.AddToRoleAsync(user, Roles.Admin.ToString()).Wait();
            }
        }
    }
}