using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moodify.BAL.Interfaces;
using Moodify.DTO;
using Moodify.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Moodify.BAL.Services
{
	public class MusicService:IMusicService
	{
		private readonly MoodifyDbContext db;
		private readonly BlobServiceClient blobServiceClient;
		private readonly IConfiguration configuration;
		private readonly ISpotifyMusicService spotifyMusicService;
		private static readonly Dictionary<string, string> EmotionToMood = new()
		{
		{ "sad", "happy" },
		{ "angry", "calm" },
		{ "neutral", "lofi" },
		{ "happy", "party" },
		{ "tired", "energizing" },
		{ "surprised", "fun" },
		{ "confident", "power" }
		};

		public MusicService(MoodifyDbContext db, BlobServiceClient blobServiceClient, IConfiguration configuration, ISpotifyMusicService spotifyMusicService)
		{
			this.db = db;
			this.blobServiceClient = blobServiceClient;
			this.configuration = configuration;
			this.spotifyMusicService = spotifyMusicService;
		}

		public async Task<IEnumerable<object>> SearchLocalMusicAsync(string query)
		{
			return await db.Musics
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
		}

		public async Task<IEnumerable<SendMusicDto>> GetMusicByCategoryAsync(string category, int pageNumber, int pageSize)
		{
			return await db.Musics
				.Where(m => m.Category == category)
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.Select(m => new SendMusicDto
				{
					MusicId = m.MusicId,
					Title = m.Title,
					musicurl = m.musicurl,
					ContentType = m.Category,
					count = m.Count
				})
				.ToListAsync();
		}

		public async Task<IEnumerable<SendMusicDto>> GetAllMusicAsync(int pageNumber, int pageSize)
		{
			return await db.Musics
				.OrderBy(m => m.Category)
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.Select(m => new SendMusicDto
				{
					MusicId = m.MusicId,
					Title = m.Title,
					musicurl = m.musicurl,
					ContentType = m.Category,
					count = m.Count
				})
				.ToListAsync();
		}

		public async Task<SendMusicDto?> GetMusicByIdAsync(int id)
		{
			var music = await db.Musics.FindAsync(id);
			if (music == null) return null;

			return new SendMusicDto
			{
				MusicId = music.MusicId,
				Title = music.Title,
				musicurl = music.musicurl,
				ContentType = music.Category,
				count = music.Count
			};
		}

		public async Task AddMusicAsync(addmusicDTO dto)
		{
			var containerClient = blobServiceClient.GetBlobContainerClient("music");
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
		}
		public async Task<object> HandleFrameUploadAsync(IFormFile file)
		{
			if (file == null || file.Length == 0)
				throw new ArgumentException("Invalid file");

			using var client = new HttpClient();
			using var content = new MultipartFormDataContent();
			content.Add(new StreamContent(file.OpenReadStream()), "file", file.FileName);

			// Call FastAPI backend
			var response = await client.PostAsync("http://127.0.0.1:8000/predict", content);
			var result = await response.Content.ReadAsStringAsync();

			if (!response.IsSuccessStatusCode)
				throw new Exception($"FastAPI error: {response.StatusCode} - {result}");

			var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(result);
			var emotion = dict?["expression"]?.ToLower();

			if (string.IsNullOrEmpty(emotion) || !EmotionToMood.ContainsKey(emotion))
				throw new Exception($"No mood mapping found for emotion: {emotion ?? "unknown"}");

			var mood = EmotionToMood[emotion];

			// Call Spotify + Local DB
			var spotifyPlaylists = await spotifyMusicService.SearchPlaylistsByMoodAsync(mood);

			var localResults = await db.Musics
				.Where(m => m.Category.ToLower() == mood.ToLower())
				.Select(m => new
				{
					m.MusicId,
					m.Title,
					m.Category,
					m.musicurl,
					m.Count
				})
				.ToListAsync();

			return new
			{
				emotion,
				mood,
				spotify = spotifyPlaylists,
				local = localResults
			};
		}
	}
}
