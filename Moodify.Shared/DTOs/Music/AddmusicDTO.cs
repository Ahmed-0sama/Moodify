using Microsoft.AspNetCore.Http;

namespace Moodify.DTO
{
	public class addmusicDTO
	{
		public string title { get; set; }
		public IFormFile file { get; set; }
		public string ContentType { get; set; }
		public List<int> ArtistIds { get; set; } = new List<int>();
	}
}
