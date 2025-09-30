using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moodify.BAL.Interfaces
{
	public interface ISpotifyTokenManager
	{
		Task<string> GetAccessTokenAsync();
	}
}
