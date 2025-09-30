using Moodify.Shared.DTOs.Favorites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moodify.BAL.Interfaces
{
	public interface IFavoriteService
	{
		Task<string> AddFavoriteAsync(string userId, int songId);
		Task<IEnumerable<FavoriteDto>> GetFavoritesAsync(string userId);
	}
}
