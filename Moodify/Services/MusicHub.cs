using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moodify.Models;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Moodify.Services
{
	public class MusicHub:Hub
	{
		private readonly MoodifyDbContext _db;
		private readonly UserManager<User> _userManager;
		private static ConcurrentDictionary<string, List<string>> _connections = new();
		public MusicHub(MoodifyDbContext db, UserManager<User> userManager)
		{
			_db = db;
			_userManager = userManager;
		}
		public override async Task OnConnectedAsync()
		{
			var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId))
			{
				await base.OnConnectedAsync();
				return;
			}

			_connections.AddOrUpdate(
				userId,
				new List<string> { Context.ConnectionId },
				(key, existingList) =>
				{
					existingList.Add(Context.ConnectionId);
					return existingList;
				});

			await base.OnConnectedAsync();
		}

		public override async Task OnDisconnectedAsync(Exception exception)
		{
			var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (!string.IsNullOrEmpty(userId))
			{
				if (_connections.TryGetValue(userId, out var connections))
				{
					connections.Remove(Context.ConnectionId);
					if (connections.Count == 0)
						_connections.TryRemove(userId, out _);
				}
			}

			await base.OnDisconnectedAsync(exception);
		}

		// Helper: get list of friend userIds from DB
		private async Task<List<string>> GetFriendsForUser(string userId)
		{
			return await _db.Friends
				.Where(f => f.userid == userId)
				.Select(f => f.friendid)
				.ToListAsync();
		}

		// Broadcast position
		public async Task UpdatePosition(string musicId, double currentTime)
		{
			var userId = Context.UserIdentifier;
			if (string.IsNullOrEmpty(userId)) return;

			var friends = await GetFriendsForUser(userId);

			await Clients.Users(friends)
				.SendAsync("ReceivePosition", musicId, currentTime, userId);
		}

		public async Task SendPlay(string musicId)
		{
			var userId = Context.UserIdentifier;
			if (string.IsNullOrEmpty(userId)) return;

			var friends = await GetFriendsForUser(userId);

			await Clients.Users(friends)
				.SendAsync("ReceivePlay", musicId, userId);
		}

		public async Task SendPause(string musicId)
		{
			var userId = Context.UserIdentifier;
			if (string.IsNullOrEmpty(userId)) return;

			var friends = await GetFriendsForUser(userId);

			await Clients.Users(friends)
				.SendAsync("ReceivePause", musicId, userId);
		}
		public async Task RespondWithPosition(string musicId, double currentTime, string requesterId)
		{
			var userId = Context.UserIdentifier;
			if (string.IsNullOrEmpty(userId)) return;

			// Send A’s position back to the requester (friend B)
			await Clients.User(requesterId)
				.SendAsync("ReceivePosition", musicId, currentTime, userId);
		}
		public async Task RequestSync(string friendId, string musicId)
		{
			var userId = Context.UserIdentifier;
			if (string.IsNullOrEmpty(userId)) return;

			var friends = await GetFriendsForUser(userId);
			if (!friends.Contains(friendId)) return;

			await Clients.User(friendId)
				.SendAsync("RequestCurrentPosition", musicId, userId);
		}
	}
}
