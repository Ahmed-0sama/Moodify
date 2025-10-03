using Moodify.BAL.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Moodify.BAL.Services
{
	public class SpotifyMusicService:ISpotifyMusicService
	{
		private readonly ISpotifyTokenManager _tokenManager;
		public SpotifyMusicService(ISpotifyTokenManager tokenManager)
		{
			_tokenManager = tokenManager;
		}
		private async Task<HttpClient> GetClientAsync()
		{
			var token = await _tokenManager.GetAccessTokenAsync();
			var client = new HttpClient();
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
			return client;
		}
		public async Task<List<(string Name, string Link)>> SearchPlaylistsByMoodAsync(string mood)
		{
			var client = await GetClientAsync();
			var url = $"https://api.spotify.com/v1/search?q={Uri.EscapeDataString(mood)}&limit=10&type=playlist";

			var response = await client.GetStringAsync(url);
			var jObj = JObject.Parse(response);

			return jObj["playlists"]["items"]
				.Select(i => ((string)i["name"], (string)i["external_urls"]["spotify"]))
				.ToList();
		}


		public async Task<List<object>> SearchTracksAsync(string query)
		{
			var client = await GetClientAsync();
			var url = $"https://api.spotify.com/v1/search?q={Uri.EscapeDataString(query)}&limit=10&type=track";

			var response = await client.GetStringAsync(url);
			var jObj = JObject.Parse(response);

			return jObj["tracks"]["items"].Select(t => new
			{
				Id = (string)t["id"],
				Title = (string)t["name"],
				Artist = string.Join(", ", t["artists"].Select(a => (string)a["name"])),
				Url = (string)t["external_urls"]["spotify"]
			}).Cast<object>().ToList();
		}

		public async Task<string> GetTopHitsAsync()
		{
			var client = await GetClientAsync();
			return await client.GetStringAsync("https://api.spotify.com/v1/browse/new-releases?limit=5");
		}
	}
}

