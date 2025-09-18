using Moodify.Models;

namespace Moodify.DTO
{
	public class GetMusicDto
	{
		public int MusicId { get; set; }
		public string Title { get; set; } = string.Empty;
		public string MusicUrl { get; set; } = string.Empty;
		public string Category { get; set; } = string.Empty;
		public int? Count { get; set; }
		public List<artistInfoDTO> Artists { get; set; } = new();
	}
}
