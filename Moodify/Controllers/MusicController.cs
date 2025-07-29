using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moodify.Models;
using Moodify.Services;
using System.Net.Http.Headers;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Moodify.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class MusicController : ControllerBase
	{
		private readonly UserManager<User> userManager;
		private readonly IConfiguration configuration;
		private readonly SpotifyTokenManager spotifyTokenManager;
		public static Dictionary<string, string> EmotionToMood = new Dictionary<string, string>
{
		{ "sad", "happy" },
		{ "angry", "calm" },
		{ "neutral", "lofi" },
		{ "happy", "party" },
		{ "tired", "energizing" },
		{ "surprised", "fun" },
		{ "confident", "power" }
};
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

			var url = $"https://api.spotify.com/v1/search?q={Uri.EscapeDataString(query)}&limit=10&type=track";

			var response = await client.GetAsync(url);
			var content = await response.Content.ReadAsStringAsync();

			if (!response.IsSuccessStatusCode)
				return StatusCode((int)response.StatusCode, content);

			return Content(content, "application/json");
		}
		[Authorize]
		[HttpGet("searchbymood")]
		public async Task<IActionResult> searchbymood([FromQuery]string query)
		{
			var user = userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
			if (user == null)
			{
				return BadRequest("User Not Found");
			}
			if (!EmotionToMood.ContainsKey(query.ToLower()))
			{
				return NotFound("Not valid mood");
			}
			string mood = EmotionToMood[query.ToLower()];//get the mood to seach for
			var token = await spotifyTokenManager.GetAccessTokenAsync();
			using var client = new HttpClient();
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

			var url = $"https://api.spotify.com/v1/search?q={Uri.EscapeDataString(mood)}&limit=10&type=playlist";

			var response = await client.GetAsync(url);
			var content = await response.Content.ReadAsStringAsync();

			if (!response.IsSuccessStatusCode)
				return StatusCode((int)response.StatusCode, content);

			return Content(content, "application/json");
		}
		[Authorize]
		[HttpGet("getTopHits")]
		public async Task<IActionResult> GetTopHits()
		{
			var user = userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
			if (user == null)
			{
				return BadRequest("User Not Found");
			}
			var token = await spotifyTokenManager.GetAccessTokenAsync();
			using var client = new HttpClient();
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

			var url = $"https://api.spotify.com/v1/browse/new-releases?limit=5";

			var response = await client.GetAsync(url);
			var content = await response.Content.ReadAsStringAsync();

			if (!response.IsSuccessStatusCode)
				return StatusCode((int)response.StatusCode, content);

			return Content(content, "application/json");
		}
	}
	
}

