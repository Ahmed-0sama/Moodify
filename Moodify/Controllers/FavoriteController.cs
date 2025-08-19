using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moodify.Models;
using System.Security.Claims;

namespace Moodify.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class FavoriteController : ControllerBase
	{
		private UserManager<User> userManager;
		private IConfiguration configuration;
		private MoodifyDbContext db;
		public FavoriteController(UserManager<User> userManager, IConfiguration configuration, MoodifyDbContext db)
		{
			this.userManager = userManager;
			this.configuration = configuration;
			this.db = db;

		}
		[Authorize]
		[HttpPost("AddFavourites")]
		public async Task<IActionResult> AddFavourites([FromBody] int id)
		{
			var user = await userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
			if (user == null)
			{
				return BadRequest("UserNotFound");
			}
			var music = await db.Musics.FirstOrDefaultAsync(s => s.MusicId == id);
			if (music == null)
			{
				return NotFound("MusicNotFound");
			}
			music.Count += 1;
			var obj = new Favorite()
			{
				Musicid = music.MusicId,
				Userid = user.Id
			};
			await db.Favorites.AddAsync(obj);
			await db.SaveChangesAsync();
			return Ok("Song Added to Favourites");
		}
		[HttpGet("GetFavorite")]
		public async Task<IActionResult> GetFavorite()
		{
			var user = await userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
			if (user == null)
			{
				return NotFound("User Not Found");
			}
			var favs = await db.Favorites
				.Where(s => s.Userid == user.Id)
				.Include(s => s.Music)
				.Select(s => new
				{
					MusicId = s.Music.MusicId,
					Title = s.Music.Title,
					Url = s.Music.musicurl
				})
				.ToListAsync();

			return Ok(favs);
		}
	}
}
