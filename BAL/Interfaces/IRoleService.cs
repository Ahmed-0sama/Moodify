using Moodify.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Moodify.BAL.Interfaces
{
	public interface IRoleService
	{
		Task<IEnumerable<Permission>> GetPermissionsAsync();
		Task<ApplicationRole> AddRoleAsync(string roleName, IEnumerable<int> permissionIds);
		Task<ApplicationRole> EditRoleAsync(string roleId, string newName, IEnumerable<int> permissionIds);
		Task<IEnumerable<ApplicationRole>> GetRolesAsync();
		Task<ApplicationRole?> GetByIdAsync(string roleId);
		Task<IEnumerable<ApplicationRole>> SearchAsync(string keyword);
		Task<IEnumerable<RoleWithPermissionsDto>> GetRolesWithPermissionsAsync();
	}
}
