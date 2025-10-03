using Azure.Storage.Blobs;
using Google.Apis.Drive.v3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moodify.BAL.Interfaces;
using Moodify.BAL.Services;
using Moodify.DTO;
using Moodify.Models;
using Moodify.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Drawing.Printing;
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
		private readonly IMusicService _musicService;
		private readonly ISpotifyMusicService _spotifyMusicService;
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
		public MusicController(IMusicService musicService, ISpotifyMusicService spotifyMusicService)
		{
			_musicService = musicService;
			_spotifyMusicService = spotifyMusicService;
		}
		[Authorize]
		[HttpGet("searchformusic")]
		public async Task<IActionResult> SearchForMusic([FromQuery] string query)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId))
				return Unauthorized("User not found.");

			var localResults = await _musicService.SearchLocalMusicAsync(query);
			var spotifyResults = await _spotifyMusicService.SearchTracksAsync(query);

			return Ok(new
			{
				Local = localResults,
				Spotify = spotifyResults
			});
		}
		[Authorize]
		[HttpGet("searchbymoodSpotify")]
		public async Task<IActionResult> searchbymood([FromQuery] string query)
		{
			if (string.IsNullOrWhiteSpace(query))
				return BadRequest("Invalid emotion");

			var playlists = await _spotifyMusicService.SearchPlaylistsByMoodAsync(query);
			if (playlists == null || !playlists.Any())
				return NotFound("No playlists found for mood.");

			return Ok(playlists);
		}
		[Authorize(Roles = "Admin")]
		[HttpPost("AddSong")]
		public async Task<IActionResult> Addsong(addmusicDTO dto)
		{
			if (dto.file == null || dto.file.Length == 0)
				return BadRequest("No file provided.");

			await _musicService.AddMusicAsync(dto);

			return Ok(new { Message = "Upload successful" });
		}
		[Authorize]
		[HttpGet("getTopHits")]
		public async Task<IActionResult> GetTopHits()
		{
			var hits = await _spotifyMusicService.GetTopHitsAsync();
			return Content(hits, "application/json");
		}
		[Authorize]
		[HttpGet("GetMusic/{type}")]
		public async Task<IActionResult> GetMusicByCategory(string category, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
		{
			var paged = await _musicService.GetMusicByCategoryAsync(category, pageNumber, pageSize);
			return Ok(paged);
		}
		[Authorize]
		[HttpGet("GetAllMusic")]
		public async Task<IActionResult> GetAllMusic(int pageNumber = 1, int pageSize = 10)
		{
			var paged = await _musicService.GetAllMusicAsync(pageNumber, pageSize);
			return Ok(paged);
		}
		[Authorize]
		[HttpGet("GetMusicById{id}")]
		public async Task<IActionResult> GetMusicById(int id)
		{
			var music = await _musicService.GetMusicByIdAsync(id);
			if (music == null) return NotFound("Music not found.");

			return Ok(music);
		}
		[HttpGet("GetMuiscByCategory{category}")]
		public async Task<IActionResult> GetMuiscByCategory(
		string category,
		int pageNumber = 1,
		int pageSize = 10)
		{
			var paged = await _musicService.GetMusicByCategoryAsync(category, pageNumber, pageSize);
			return Ok(paged);
		}
		[Authorize]
		[HttpPost("upload-frame")]
		public async Task<IActionResult> UploadFrame(IFormFile file)
		{
			if (file == null || file.Length == 0)
				return BadRequest("No file provided.");

			var result = await _musicService.HandleFrameUploadAsync(file);
			return Ok(result);
		}
	}
}