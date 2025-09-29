using Moodify.DTO;
using Moodify.Shared.DTOs.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moodify.BAL.Interfaces
{
	public interface IFriendService
	{
		Task<PagedResponseDto<UserDataDtoShared>> SearchForFriendAsync(string userId, string query, int pageNumber, int pageSize);
		Task<string> SendFriendRequestAsync(string userId, string targetUserId);
		Task<IEnumerable<ShowSentReqDTO>> GetSentRequestsAsync(string userId);
		Task<IEnumerable<ShowSentReqDTO>> GetReceivedRequestsAsync(string userId);
		Task<string> AcceptRequestAsync(string userId, int requestId);
		Task<string> RejectRequestAsync(string userId, int requestId);
		Task<IEnumerable<ShowFrriendsDTO>> GetFriendsAsync(string userId);
	}
}
