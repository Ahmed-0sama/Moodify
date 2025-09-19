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
using System.Text.Json;
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
		public MusicController(UserManager<User> userManager, IConfiguration configuration, SpotifyTokenManager spotifyTokenManager, MoodifyDbContext db)
		{
			this.userManager = userManager;
			this.configuration = configuration;
			this.spotifyTokenManager = spotifyTokenManager;
			this.sasUri = configuration["AzureBlob:AzureBlobStorage"];
			this.db = db;
		}
		[Authorize]
		[HttpGet("searchformusic")]
		public async Task<IActionResult> SearchForMusic([FromQuery] string query)
		{
			var user = userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
			if (user == null)
			{
				return BadRequest("User Not Found");
			}
			var localResults = await db.Musics
			.Where(m => m.Title.Contains(query))
			.Select(m => new
			{
				m.MusicId,
				m.Title,
				m.Category,
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
			.Select(t => new
			{
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
		[HttpGet("searchbymoodSpotify")]
		public async Task<IActionResult> searchbymood([FromQuery] string query)
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
			var playlists = await SearchSpotifyByMoodAsync(query);

			var response = playlists.Select(p => new { name = p.Name, link = p.Link });
			return Ok(response);
		}
		private async Task<List<(string Name, string Link)>> SearchSpotifyByMoodAsync(string query)
		{
			string mood = EmotionToMood[query.ToLower()]; // map emotion → mood

			var token = await spotifyTokenManager.GetAccessTokenAsync();

			using var client = new HttpClient();
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

			var url = $"https://api.spotify.com/v1/search?q={Uri.EscapeDataString(mood)}&limit=10&type=playlist";

			var response = await client.GetAsync(url);
			var content = await response.Content.ReadAsStringAsync();

			if (!response.IsSuccessStatusCode)
			{
				// Throw exception or just return content with error info
				throw new Exception($"Spotify API error: {response.StatusCode}, {content}");
			}
			var results = new List<(string Name, string Link)>();

			using (JsonDocument doc = JsonDocument.Parse(content))
			{
				var items = doc.RootElement
							   .GetProperty("playlists")
							   .GetProperty("items");

				foreach (var item in items.EnumerateArray())
				{
					if (item.ValueKind == JsonValueKind.Null) continue;

					string name = item.GetProperty("name").GetString();
					string link = item.GetProperty("external_urls")
									  .GetProperty("spotify").GetString();

					results.Add((name, link));
				}
			}
			return results; // JSON string from after edit it Spotify
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
					Category = dto.ContentType,
					musicurl = blobClient.Uri.ToString(),

				};
				await db.Musics.AddAsync(music);
				await db.SaveChangesAsync();
				foreach (var artist in dto.ArtistIds)
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
			var list = await db.Musics.Where(s => s.Category == type).ToListAsync();
			var result = list.Select(item => new SendMusicDto
			{
				MusicId = item.MusicId,
				Title = item.Title,
				musicurl = item.musicurl,
				ContentType =
				item.Category,
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
				.OrderBy(m => m.Category) // Make sure ordering is applied
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
					ContentType = music.Category,
					Title = music.Title,
					count = music.Count,
					musicurl = music.musicurl
				};
				return Ok(dto);
			}
			return NotFound();
		}
		[HttpGet("GetMuiscByCategory{category}")]
		public async Task<IActionResult> GetMuiscByCategory(
		string category,
		int pageNumber = 1,
		int pageSize = 10)
		{
			// Get current user
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var user = await userManager.FindByIdAsync(userId);

			if (user == null)
			{
				return Unauthorized("User not found or not logged in.");
			}

			// Query musics by category
			var query = db.Musics.Where(s => s.Category == category);

			var totalCount = await query.CountAsync();

			var musics = await db.Musics
				.Where(m => m.Category == category)
				.Include(m => m.ArtistMusics)
					.ThenInclude(am => am.Artist) // load related artist
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.Select(m => new GetMusicDto
				{
					MusicId = m.MusicId,
					Title = m.Title,
					MusicUrl = m.musicurl,
					Category = m.Category,
					Count = m.Count,
					Artists = m.ArtistMusics.Select(am => new artistInfoDTO
					{
						artistid = am.Artist.ArtistId,
						ArtistName = am.Artist.ArtistName
					}).ToList()
				})
				.ToListAsync();

			// Build response
			var response = new PagedResponseDto<GetMusicDto>
			{
				TotalCount = totalCount,
				PageNumber = pageNumber,
				PageSize = pageSize,
				TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
				Data = musics
			};

			return Ok(response);
		}
		[Authorize]
		[HttpPost("upload-frame")]
		public async Task<IActionResult> UploadFrame(IFormFile file)
		{
			var user = await userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
			if (user == null)
				return Unauthorized();

			if (file == null || file.Length == 0)
				return BadRequest("No file provided.");

			try
			{
				// Send frame to Python FastAPI
				using var client = new HttpClient();
				using var content = new MultipartFormDataContent();
				content.Add(new StreamContent(file.OpenReadStream()), "file", file.FileName);

				var response = await client.PostAsync("http://127.0.0.1:8000/predict", content);
				var result = await response.Content.ReadAsStringAsync();

				if (!response.IsSuccessStatusCode)
					return StatusCode((int)response.StatusCode, result);

				//Parse emotion JSON from Python
				var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(result);
				var emotion = dict?["expression"]?.ToLower();

				if (string.IsNullOrEmpty(emotion) || !EmotionToMood.ContainsKey(emotion))
					return NotFound($"No mood mapping found for emotion: {emotion ?? "unknown"}");

				var mood = EmotionToMood[emotion];

				//Call both Spotify + Local DB in parallel
				var spotifyTask = SearchSpotifyByMoodAsync(mood);
				var localTask = SearchSpotifyBySite(mood);

				await Task.WhenAll(spotifyTask, localTask);

				var spotifyData = spotifyTask.Result
				.Select(p => new { name = p.Name, link = p.Link })
				.ToList();
				var siteJson = localTask.Result;
				var siteData = System.Text.Json.JsonSerializer.Deserialize<object>(siteJson);

				// Return combined musics
				return Ok(new
				{
					emotion,
					mood,
					spotify = spotifyData,
					local = siteData
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Error: {ex.Message}");
			}
		}

		private async Task<string> SearchSpotifyBySite(string query)
		{

			var musics = await db.Musics
						 .Where(q => q.Category.ToLower() == query.ToLower())
						 .Select(m => new
						 {
							 m.musicurl,
							 m.Title,
							 m.ArtistMusics,
							 m.Category
						 })
						 .ToListAsync();

			// Convert result to JSON string
			return System.Text.Json.JsonSerializer.Serialize(musics);
		}
	}

}