using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Http;
using Moodify.BAL.Interfaces;

namespace Moodify.BAL.Services
{
	public class EmotionService:IEmotionService
	{
		private readonly HttpClient client;

		public EmotionService(IHttpClientFactory httpClientFactory)
		{
			client = httpClientFactory.CreateClient();
		}

		public async Task<string?> DetectEmotionAsync(IFormFile file)
		{
			using var content = new MultipartFormDataContent();
			content.Add(new StreamContent(file.OpenReadStream()), "file", file.FileName);

			var response = await client.PostAsync("http://127.0.0.1:8000/predict", content);
			if (!response.IsSuccessStatusCode) return null;

			var result = await response.Content.ReadAsStringAsync();
			var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(result);

			return dict?["expression"]?.ToLower();
		}
	}
}
