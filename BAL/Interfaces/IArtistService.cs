using Moodify.DTO;
using Moodify.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moodify.BAL.Interfaces
{
	public interface IArtistService
	{
		Task<string> AddArtistAsync(string userId, AddArtistInfoDTO dto);
		Task<IEnumerable<artistInfoDTO>> SearchArtistInfoAsync(string userId, string query);
		Task<Artist> GetArtistInfoAsync(string userId, int id);
	}
}
