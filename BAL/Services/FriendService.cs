using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moodify.BAL.Interfaces;
using Moodify.DTO;
using Moodify.Models;
using Moodify.Shared.DTOs.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Moodify.BAL.Services
{
	public class FriendService: IFriendService
	{
		private readonly UserManager<User> _userManager;
		private readonly MoodifyDbContext _db;

		public FriendService(UserManager<User> userManager, MoodifyDbContext db)
		{
			_userManager = userManager;
			_db = db;
		}
		public async Task<string> SendFriendRequestAsync(string userId, string targetUserId)
		{
			var user = await _userManager.FindByIdAsync(userId);
			var targetUser = await _userManager.FindByIdAsync(targetUserId);
			if (user == null || targetUser == null)
			{
				return "User not found";
			}
			if (userId == targetUserId)
			{
				return "You cannot send a friend request to yourself";
			}
			var existingRequest = await _db.FriendReqs
				.FirstOrDefaultAsync(fr => fr.sendid == userId && fr.receiveid == targetUserId);
			if (existingRequest != null)
			{
				return "Friend request already sent";
			}
			var friends=await _db.Friends.FirstOrDefaultAsync(u=>u.userid==userId && u.friendid==targetUserId);
			if (friends != null)
			{
				return "You are already friends";
			}
			var friendRequest = new FriendReq
			{
				sendid = userId,
				senderfname= user.FirstName,
				senderlname= user.LastName,
				recieverfname= targetUser.FirstName,
				recieverlname= targetUser.LastName,
				receiveid = targetUserId,
				sendAt = DateTime.Now,
				Status = "Pending"
			};
			await _db.FriendReqs.AddAsync(friendRequest);
			await _db.SaveChangesAsync();
			return "Friend request sent successfully";
		}
		public async Task<IEnumerable<ShowFrriendsDTO>> GetFriendsAsync(string userId)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return Enumerable.Empty<ShowFrriendsDTO>();
			}
			var list= await _db.Friends.Where(s => s.userid == user.Id).ToListAsync();
			var lt = list.Select(item => new ShowFrriendsDTO
			{
				name= item.friendName
			});
			return lt;
		}
		public async Task<string> RejectRequestAsync(string userId, int requestId)
		{
			var user =await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return "User not found";
			}
			var friendRequest = await _db.FriendReqs.FirstOrDefaultAsync(fr => fr.Id == requestId && fr.receiveid == userId);
			if (friendRequest == null)
			{
				return "Friend request not found";
			}
			if (friendRequest.Status != "Pending")
			{
				return "Friend request already processed";
			}
			_db.FriendReqs.Remove(friendRequest);
			await _db.SaveChangesAsync();
			return "Friend request rejected successfully";
		}
		public async Task<string> AcceptRequestAsync(string userId, int requestId)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return "User not found";
			}
			var friendRequest = await _db.FriendReqs.FirstOrDefaultAsync(fr => fr.Id == requestId && fr.receiveid == userId);
			if (friendRequest == null)
			{
				return "Friend request not found";
			}
			if (friendRequest.Status != "Pending")
			{
				return "Friend request already processed";
			}
			var item=await _db.FriendReqs.Include(f=>f.Sender)
				.Include(f=>f.Receiver)
				.FirstOrDefaultAsync(f=>f.Id==requestId);
			if(item == null)
			{
				return "Not Found item with this id";
			}
			var other = item.sendid == user.Id ? item.Receiver : item.Sender;
			if (item.Status == "Pending")
			{
				var sender = item.Sender;
				var receiver = item.Receiver;

				// Friend from sender to receiver
				var friend1 = new Friends
				{
					userid = sender.Id,
					friendid = receiver.Id,
					friendName = receiver.FirstName + " " + receiver.LastName
				};

				// Friend from receiver to sender
				var friend2 = new Friends
				{
					userid = receiver.Id,
					friendid = sender.Id,
					friendName = sender.FirstName + " " + sender.LastName
				};

				await _db.Friends.AddRangeAsync(friend1, friend2);
				_db.FriendReqs.Remove(item);
				await _db.SaveChangesAsync();
				return "You Now are Friends";
			}
			return "this request are not found";
		}

		public async Task<PagedResponseDto<UserDataDtoShared>> SearchForFriendAsync(string userId, string query, int pageNumber, int pageSize)
		{
			if (string.IsNullOrWhiteSpace(query))
			{
				return new PagedResponseDto<UserDataDtoShared>
				{
					TotalCount = 0,
					PageNumber = pageNumber,
					PageSize = pageSize,
					TotalPages = 0,
					Data = new List<UserDataDtoShared>()
				};
			}

			var totalCount = await _db.Users
				.Where(u => u.Id != userId &&
							(u.FirstName.Contains(query) || u.LastName.Contains(query)))
				.CountAsync();

			var users = await _db.Users
				.Where(u => u.Id != userId &&
							(u.FirstName.Contains(query) || u.LastName.Contains(query)))
				.OrderBy(u => u.FirstName) // add ordering when paging
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.Select(u => new UserDataDtoShared
				{
					Id=u.Id,
					Username= u.UserName,
					Fname = u.FirstName,
					Lname = u.LastName
				})
				.ToListAsync();

			return new PagedResponseDto<UserDataDtoShared>
			{
				TotalCount = totalCount,
				PageNumber = pageNumber,
				PageSize = pageSize,
				TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
				Data = users
			};
		}

		public async Task<IEnumerable<ShowSentReqDTO>> GetSentRequestsAsync(string userId)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				throw new Exception("User Not Found");
			}
			return await _db.FriendReqs
				.Where(fr => fr.sendid == userId && fr.Status == "Pending")
				.Select(fr => new ShowSentReqDTO
				{
					 FirstName= fr.recieverfname,
					LastName = fr.recieverlname,
					status = fr.Status,
					sendAt= fr.sendAt
				})
				.ToListAsync();
		}

		public async Task<IEnumerable<ShowSentReqDTO>> GetReceivedRequestsAsync(string userId)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				throw new Exception("User Not Found");
			}
			var req = await _db.FriendReqs.Where(s => s.receiveid == user.Id).ToListAsync();
			var lt = req.Select(item => new ShowSentReqDTO
			{
				id = item.Id,
				FirstName = item.senderfname,
				LastName = item.senderlname,
				status = item.Status,
				sendAt = item.sendAt
			}).ToList();
			return lt;
		}
	}
}
