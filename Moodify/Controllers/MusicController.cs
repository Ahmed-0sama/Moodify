using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moodify.Models;
using Moodify.Services;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace Moodify.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class MusicController : ControllerBase
	{
		private readonly UserManager<User> userManager;
		private readonly IConfiguration configuration;
		private readonly SpotifyTokenManager spotifyTokenManager;
		public MusicController(UserManager<User> userManager, IConfiguration configuration,SpotifyTokenManager spotifyTokenManager)
		{
			this.userManager = userManager;
			this.configuration = configuration;
			this.spotifyTokenManager = spotifyTokenManager;
		}
		[Authorize]
		[HttpGet("searchformusic")]
		public async Task<IActionResult> SearchForMusic([FromQuery]string query)
		{
			var user = userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
			if (user == null)
			{
				return BadRequest("User Not Found");
			}
			var token = await spotifyTokenManager.GetAccessTokenAsync();
			using var client = new HttpClient();
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

			var url = $"https://api.spotify.com/v1/search?q={Uri.EscapeDataString(query)}&limit=10";

			var response = await client.GetAsync(url);
			var content = await response.Content.ReadAsStringAsync();

			if (!response.IsSuccessStatusCode)
				return StatusCode((int)response.StatusCode, content);

			return Content(content, "application/json");
		}
	}
}

