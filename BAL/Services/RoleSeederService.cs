using Microsoft.AspNetCore.Identity;
using Moodify.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moodify.BAL.Services
{
	public class RoleSeederService
	{
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly UserManager<User> _userManager;
		public RoleSeederService(RoleManager<IdentityRole> roleManager, UserManager<User> userManager)
		{
			_roleManager = roleManager;
			_userManager = userManager;
		}

		public async Task SeedRolesAndAdminAsync()
		{
			string[] roles = { "Admin", "User", "Artist" };

			foreach (var role in roles)
			{
				if (!await _roleManager.RoleExistsAsync(role))
				{
					await _roleManager.CreateAsync(new IdentityRole(role));
				}
			}
			var adminEmail = "admin@moodify.com";
			var adminUser = await _userManager.FindByEmailAsync(adminEmail);

			if (adminUser == null)
			{
				var user = new User
				{
					UserName = "admin",
					Email = adminEmail,
					FirstName = "System",
					LastName = "Admin",
					EmailConfirmed = true
				};

				var result = await _userManager.CreateAsync(user, "Admin@123");
				if (result.Succeeded)
				{
					await _userManager.AddToRoleAsync(user, "Admin");
				}
			}
		}
	}
}
