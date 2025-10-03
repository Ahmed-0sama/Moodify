using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moodify.BAL.Interfaces
{
	public interface ISpotifyMusicService
	{
		Task<List<(string Name, string Link)>> SearchPlaylistsByMoodAsync(string mood);
		Task<List<object>> SearchTracksAsync(string query);
		Task<string> GetTopHitsAsync();
	}
}
