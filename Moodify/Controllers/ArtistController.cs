using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moodify.DTO;
using Moodify.Models;
using System.Security.Claims;

namespace Moodify.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ArtistController : ControllerBase
	{
		private UserManager<User> userManager;
		private IConfiguration configuration;
		private MoodifyDbContext db;
		public ArtistController(UserManager<User> userManager, IConfiguration configuration, MoodifyDbContext db)
		{
			this.userManager = userManager;
			this.configuration = configuration;
			this.db = db;

		}
		[Authorize]
		[HttpPost("AddArtist")]
		public async Task<IActionResult> AddArtist(AddArtistInfoDTO dto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			var user = await userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
			if (user == null)
			{
				return NotFound("User Not Found");
			}
			var arrtist = new Artist
			{
				ArtistName = dto.FName + " " + dto.LName,
				Description = dto.Info,
				Photo = dto.Image
			};
			await db.Artists.AddAsync(arrtist);
			await db.SaveChangesAsync();
			return Ok("Artist added successfully");
		}
		[HttpGet("SearchArtistInfo/{query}")]
		[Authorize]
		public async Task<IActionResult> SearchArtistInfo(string query)
		{
			var user = await userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
			if (user == null)
			{
				return NotFound("User Not Found");
			}
			var matchingArtists = await db.Artists
				.Where(s => s.ArtistName.Contains(query))
				.Select(s => new artistInfoDTO
				{
					artistid=s.ArtistId,
					ArtistName = s.ArtistName,
					Photo=s.Photo,
					Description=s.Description
				})
				.ToListAsync();
			return Ok(matchingArtists);
		}
		[HttpGet("GetArtistInfo")]
		[Authorize]
		public async Task<IActionResult> GetArtistInfo([FromBody] int id)
		{
			var user = await userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
			if (user == null)
			{
				return NotFound("User Not Found");
			}
			var artist = await db.Artists.FirstOrDefaultAsync(s => s.ArtistId == id);
			if (artist == null)
			{
				return NotFound("Artist Not Found");
			}
			return Ok(artist);
		}

	}
}
