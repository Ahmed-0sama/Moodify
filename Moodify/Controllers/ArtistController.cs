using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moodify.BAL.Interfaces;
using Moodify.BAL.Services;
using Moodify.DTO;
using Moodify.Models;
using System.Security.Claims;

namespace Moodify.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ArtistController : ControllerBase
	{
		private readonly IArtistService _artistService;
		public ArtistController(IArtistService artistService)
		{
			_artistService = artistService;
		}
		[Authorize(Roles = "Admin")]
		[HttpPost("AddArtist")]
		public async Task<IActionResult> AddArtist(AddArtistInfoDTO dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null) return Unauthorized();

			var result = await _artistService.AddArtistAsync(userId, dto);

			return result switch
			{
				"UserNotFound" => NotFound(result),
				_ => Ok(result)
			};
		}
		[Authorize]
		[HttpGet("SearchArtistInfo/{query}")]
		public async Task<IActionResult> SearchArtistInfo(string query)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null) return Unauthorized();

			try
			{
				var artists = await _artistService.SearchArtistInfoAsync(userId, query);
				return Ok(artists);
			}
			catch (Exception ex)
			{
				return NotFound(ex.Message);
			}
		}
		[Authorize]
		[HttpGet("GetArtistInfo/{id}")]
		public async Task<IActionResult> GetArtistInfo(int id)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null) return Unauthorized();

			try
			{
				var artist = await _artistService.GetArtistInfoAsync(userId, id);
				return Ok(artist);
			}
			catch (Exception ex)
			{
				return NotFound(ex.Message);
			}
		}

	}
}
