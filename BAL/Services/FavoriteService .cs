using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moodify.BAL.Interfaces;
using Moodify.Models;
using Moodify.Shared.DTOs.Favorites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moodify.BAL.Services
{
	public class FavoriteService : IFavoriteService
	{
		private readonly UserManager<User> _userManager;
		private readonly MoodifyDbContext _db;
		public FavoriteService(UserManager<User> userManager, MoodifyDbContext db)
		{
			_userManager = userManager;
			_db = db;
		}
		public async Task<string> AddFavoriteAsync(string userId, int songId)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				throw new Exception("User not found");
			}
			var song = await _db.Musics.FindAsync(songId);
			if(song == null)
			{
				throw new Exception("Song not found");
			}
			song.Count += 1;
			var favorite = new Favorite
			{
				Userid = user.Id,
				Musicid = song.MusicId
			};
			await _db.Favorites.AddAsync(favorite);
			await _db.SaveChangesAsync();
			return "Song Added to Favourites";
		}
		public async Task<IEnumerable<FavoriteDto>> GetFavoritesAsync(string userId)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				throw new Exception("User not found");
			}

			var favs = await _db.Favorites
				.Where(s => s.Userid == user.Id)
				.Include(s => s.Music)
				.Select(s => new FavoriteDto
				{
					MusicId = s.Music.MusicId,
					Title = s.Music.Title,
					Url = s.Music.musicurl
				})
				.ToListAsync();

			return favs;
		}
	}
}
