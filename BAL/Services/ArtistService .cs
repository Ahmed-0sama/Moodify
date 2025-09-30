using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moodify.BAL.Interfaces;
using Moodify.DTO;
using Moodify.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moodify.BAL.Services
{
	public class ArtistService:IArtistService
	{
		private readonly UserManager<User> _userManager;
		private readonly MoodifyDbContext _db;

		public ArtistService(UserManager<User> userManager, MoodifyDbContext db)
		{
			_userManager = userManager;
			_db = db;
		}
		public async Task<string> AddArtistAsync(string userId, AddArtistInfoDTO dto)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return "UserNotFound";
			}

			var artist = new Artist
			{
				ArtistName = dto.FName + " " + dto.LName,
				Description = dto.Info,
				Photo = dto.Image
			};

			await _db.Artists.AddAsync(artist);
			await _db.SaveChangesAsync();

			return "Artist added successfully";
		}
		public async Task<IEnumerable<artistInfoDTO>> SearchArtistInfoAsync(string userId, string query)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				throw new Exception("User not found");
			}

			return await _db.Artists
				.Where(s => s.ArtistName.Contains(query))
				.Select(s => new artistInfoDTO
				{
					artistid = s.ArtistId,
					ArtistName = s.ArtistName,
					Photo = s.Photo,
					Description = s.Description
				})
				.ToListAsync();
		}
		public async Task<Artist> GetArtistInfoAsync(string userId, int id)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				throw new Exception("User not found");
			}

			var artist = await _db.Artists.FirstOrDefaultAsync(s => s.ArtistId == id);
			if (artist == null)
			{
				throw new Exception("Artist not found");
			}

			return artist;
		}
	}
}
