using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moodify.BAL.Interfaces;
using Moodify.Models;
using System.Security.Claims;

namespace Moodify.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class FavoriteController : ControllerBase
	{
		private readonly IFavoriteService _favoriteService;
		public FavoriteController(IFavoriteService favoriteService)
		{
			_favoriteService = favoriteService;
		}
		[HttpPost("AddFavourites")]
		public async Task<IActionResult> AddFavourites([FromBody] int id)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null) return Unauthorized();

			var result = await _favoriteService.AddFavoriteAsync(userId, id);

			return result switch
			{
				"UserNotFound" => BadRequest(result),
				"MusicNotFound" => NotFound(result),
				_ => Ok(result)
			};
		}

		[HttpGet("GetFavorite")]
		public async Task<IActionResult> GetFavorite()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null) return Unauthorized();

			var favs = await _favoriteService.GetFavoritesAsync(userId);
			return Ok(favs);
		}
	}
}
