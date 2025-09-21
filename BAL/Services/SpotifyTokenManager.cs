using Microsoft.Extensions.Configuration;
using Moodify.BAL.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Moodify.Services
{
	public class SpotifyTokenManager:ISpotifyTokenManager
	{
		private readonly string clientId;
		private readonly string clientSecret;
		private string accessToken;
		private DateTime expiryTime;

		public SpotifyTokenManager(IConfiguration configuration)
		{
			clientId = configuration["Spotify:ClientId"];
			clientSecret = configuration["Spotify:ClientSecret"];
		}
		public async Task<string> GetAccessTokenAsync()
		{
			if (string.IsNullOrEmpty(accessToken) || DateTime.UtcNow >= expiryTime)
			{
				await RefreshAccessTokenAsync();
			}

			return accessToken;
		}

		private async Task RefreshAccessTokenAsync()
		{
			var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
			using var client = new HttpClient();
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);

			var body = new FormUrlEncodedContent(new[]
			{
			new KeyValuePair<string, string>("grant_type", "client_credentials")
			});

			var response = await client.PostAsync("https://accounts.spotify.com/api/token", body);
			var json = await response.Content.ReadAsStringAsync();

			using var doc = JsonDocument.Parse(json);
			var root = doc.RootElement;
			accessToken = root.GetProperty("access_token").GetString();
			int expiresIn = root.GetProperty("expires_in").GetInt32();
			expiryTime = DateTime.UtcNow.AddSeconds(expiresIn - 60); // Refresh 1 min early
		}
	}
}
