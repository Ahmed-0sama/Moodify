using Azure.Storage.Blobs;
using Google.Apis.Drive.v3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moodify.DTO;
using Moodify.Models;
using Moodify.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
		private readonly string sasUri;
		MoodifyDbContext db;
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
		public MusicController(UserManager<User> userManager, IConfiguration configuration,SpotifyTokenManager spotifyTokenManager, MoodifyDbContext db)
		{
			this.userManager = userManager;
			this.configuration = configuration;
			this.spotifyTokenManager = spotifyTokenManager;
			this.sasUri = configuration["AzureBlob:AzureBlobStorage"];
			this.db = db;
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
			var localResults = await db.Musics
			.Where(m => m.Title.Contains(query))
			.Select(m => new {
			m.MusicId,
			m.Title,
			m.ContentType,
			m.musicurl,
			m.Count,
			Source = "Local"
			})
			.ToListAsync();
			var token = await spotifyTokenManager.GetAccessTokenAsync();
			using var client = new HttpClient();
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

			var url = $"https://api.spotify.com/v1/search?q={Uri.EscapeDataString(query)}&limit=10&type=track";

			var response = await client.GetAsync(url);
			var content = await response.Content.ReadAsStringAsync();

			if (!response.IsSuccessStatusCode)
				return StatusCode((int)response.StatusCode, content);
			var spotifyResults = JsonConvert.DeserializeObject<JObject>(content)["tracks"]["items"]
			.Select(t => new {
				Id = (string)t["id"],
				Title = (string)t["name"],
				Artist = string.Join(", ", t["artists"].Select(a => (string)a["name"])),
				Url = (string)t["external_urls"]["spotify"],
				Source = "Spotify"
			})
			.ToList();
			var combinedResults = new
			{
				Local = localResults,
				Spotify = spotifyResults
			};

			return Ok(combinedResults);
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
		[Authorize(Roles = "Admin")]
		[HttpPost("AddSong")]
		public async Task<IActionResult> Addsong(addmusicDTO dto)
		{
			if (dto.file == null || dto.file.Length == 0)
				return BadRequest("No file provided.");

			try
			{
				var containerClient = new BlobContainerClient(
					configuration.GetConnectionString("AzureBlobStorage"),
					"music");

				await containerClient.CreateIfNotExistsAsync();


				var blobClient = containerClient.GetBlobClient(dto.file.FileName);

				using (var stream = dto.file.OpenReadStream())
				{
					await blobClient.UploadAsync(stream, overwrite: true);
				}
				var music = new Music
				{
					Title = dto.title,
					ContentType = dto.ContentType,
					musicurl = blobClient.Uri.ToString(),
					
				};
				await db.Musics.AddAsync(music);
				await db.SaveChangesAsync();
				foreach(var artist in dto.ArtistIds)
				{
					var artistmusic = new ArtistMusic
					{
						ArtistId = artist,
						MusicId = music.MusicId
					};
					db.ArtistMusics.Add(artistmusic);
				}
				await db.SaveChangesAsync();
				return Ok(new
				{
					Message = "Upload successful",
					Url = blobClient.Uri.ToString()
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Error: {ex.Message}");
			}
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
		[Authorize]
		[HttpGet("GetMusic/{type}")]
		public async Task<IActionResult> GetMusic(string type)
		{
			var user = await userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
			if (user == null)
			{
				return NotFound("User Not Found");
			}
			var list = await db.Musics.Where(s => s.ContentType == type).ToListAsync();
			var result = list.Select(item => new SendMusicDto
			{
				MusicId = item.MusicId,
				Title = item.Title,
				musicurl = item.musicurl,
				ContentType =
				item.ContentType,
				count = item.Count
			}).ToList();
			return Ok(result);
		}
		[Authorize]
		[HttpGet("GetAllMusic")]
		public async Task<IActionResult> GetAllMusic(int pageNumber = 1, int pageSize = 10)
		{
			var user = await userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
			if (user == null)
			{
				return Unauthorized();
			}
			var query = db.Musics.AsQueryable();

			// Total count for frontend
			var totalCount = await query.CountAsync();

			// Apply pagination
			var musicList = await query
				.OrderBy(m => m.ContentType) // Make sure ordering is applied
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			// Return results with metadata
			return Ok(new
			{
				TotalCount = totalCount,
				PageNumber = pageNumber,
				PageSize = pageSize,
				TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
				Data = musicList
			});
		}
		[Authorize]
		[HttpGet("GetMusicById{id}")]
		public async Task<IActionResult> GetMusicById(int id)
		{
			var user = await userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
			if (user == null)
			{
				return Unauthorized();
			}
			var music = await db.Musics.FirstOrDefaultAsync(s => s.MusicId == id);
			if (music != null)
			{
				var dto = new SendMusicDto()
				{
					ContentType = music.ContentType,
					Title = music.Title,
					count = music.Count,
					musicurl = music.musicurl
				};
				return Ok(dto);
			}
			return NotFound();
		}
	}
}