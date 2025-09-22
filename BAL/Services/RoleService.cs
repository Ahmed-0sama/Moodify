//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using Moodify.BAL.Interfaces;
//using Moodify.DAL.Entities;
//using Moodify.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Moodify.BAL.Services
//{
//	public class RoleService:IRoleService
//	{
//		private readonly RoleManager<User> _roleManager;
//		private readonly MoodifyDbContext _context;

//		public RoleService(RoleManager<User> roleManager, MoodifyDbContext context)
//		{
//			_roleManager = roleManager;
//			_context = context;
//		}

//		public async Task<IEnumerable<Permission>> GetPermissionsAsync()
//		{
//			return await _context.Permissions.ToListAsync();
//		}

//		public async Task<ApplicationRole> AddRoleAsync(string roleName, IEnumerable<int> permissionIds)
//		{
//			var role = new ApplicationRole { Name = roleName };
//			var result = await _roleManager.CreateAsync(role);
//			if (!result.Succeeded)
//				throw new Exception(string.Join(",", result.Errors.Select(e => e.Description)));

//			var permissions = await _context.Permissions
//				.Where(p => permissionIds.Contains(p.Id))
//				.ToListAsync();

//			foreach (var perm in permissions)
//			{
//				_context.RoleClaims.Add(new IdentityRoleClaim<string>
//				{
//					RoleId = role.Id,
//					ClaimType = "Permission",
//					ClaimValue = $"{perm.Controller}.{perm.Name}"
//				});
//			}

//			await _context.SaveChangesAsync();
//			return role;
//		}

//		public async Task<ApplicationRole> EditRoleAsync(string roleId, string newName, IEnumerable<int> permissionIds)
//		{
//			var role = await _roleManager.FindByIdAsync(roleId);
//			if (role == null) throw new Exception("Role not found");

//			role.Name = newName;
//			await _roleManager.UpdateAsync(role);

//			var oldClaims = _context.RoleClaims.Where(rc => rc.RoleId == role.Id);
//			_context.RoleClaims.RemoveRange(oldClaims);

//			var permissions = await _context.Permissions
//				.Where(p => permissionIds.Contains(p.Id))
//				.ToListAsync();

//			foreach (var perm in permissions)
//			{
//				_context.RoleClaims.Add(new IdentityRoleClaim<string>
//				{
//					RoleId = role.Id,
//					ClaimType = "Permission",
//					ClaimValue = $"{perm.Controller}.{perm.Name}"
//				});
//			}

//			await _context.SaveChangesAsync();
//			return role;
//		}

//		public async Task<IEnumerable<ApplicationRole>> GetRolesAsync()
//		{
//			return await _roleManager.Roles.ToListAsync();
//		}
//		public async Task<IEnumerable<RoleWithPermissionsDto>> GetRolesWithPermissionsAsync()
//		{
//			var roles = await _roleManager.Roles.ToListAsync();
//			var rolesWithPermissions = new List<RoleWithPermissionsDto>();
//			foreach (var role in roles)
//			{
//				var roleClaims = await _roleManager.GetClaimsAsync(role);
//				rolesWithPermissions.Add(new RoleWithPermissionsDto
//				{
//					Name = role.Name,
//					Permissions = roleClaims
//						.Where(c => c.Type == "Permission")
//						.Select(c => c.Value)
//						.ToList()
//				});

//			}
//			return rolesWithPermissions;
//		}

//		public async Task<ApplicationRole?> GetByIdAsync(string roleId)
//		{
//			return await _roleManager.FindByIdAsync(roleId);
//		}

//		public async Task<IEnumerable<ApplicationRole>> SearchAsync(string keyword)
//		{
//			return await _roleManager.Roles
//				.Where(r => r.Name.Contains(keyword))
//				.ToListAsync();
//		}
//	}
//}
//}
