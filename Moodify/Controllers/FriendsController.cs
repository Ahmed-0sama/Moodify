using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Moodify.DTO;
using Moodify.Models;
using System;
using System.Security.Claims;

namespace Moodify.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class FriendsController : ControllerBase
	{
		private readonly UserManager<User> userManager;
		private readonly IConfiguration configuration;
		private MoodifyDbContext db;
		public FriendsController(UserManager<User> userManager,IConfiguration configuration, MoodifyDbContext db)
		{
			this.userManager = userManager;
			this.configuration = configuration;
			this.db = db;
		}
		[Authorize]
		[HttpGet("SearchForFriend/{query}")]
		public async Task<IActionResult> SearchforFriend( string query ,int pageNumber = 1, int pageSize = 10)
		{
			var user = await userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
			if (user == null)
			{
				return NotFound("user not found");
			}
			if (string.IsNullOrWhiteSpace(query))
			{
				return BadRequest("Search query cannot be empty");
			}
			var totalCount = await db.Users
			.Where(u => u.Id != user.Id &&
				   (u.FirstName.Contains(query) || u.LastName.Contains(query)))
			.CountAsync();

			var matchedUsers = await db.Users
				.Where(u => u.Id != user.Id &&
							(u.FirstName.Contains(query) || u.LastName.Contains(query)))
				.OrderBy(u => u.FirstName) // always add an order when paginating
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.Select(u => new
				{
					u.Id,
					u.FirstName,
					u.LastName
				})
				.ToListAsync();
			return Ok(new
			{
				TotalCount = totalCount,
				PageNumber = pageNumber,
				PageSize = pageSize,
				TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
				Data = matchedUsers
			});
		}
		[Authorize]
		[HttpPost("SendFriendRequest")]
		public async Task<IActionResult> SendFriendRequest([FromBody]SendFriendRequestDTO dto)
		{
			var user = await userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
			if (user == null)
			{
				return NotFound("Not Found");
			}
			if (dto.userid == user.Id)
			{
				return BadRequest("you cant send friend request to yourself");
			}
			var receiver = await userManager.FindByIdAsync(dto.userid);
			if (receiver == null)
			{
				return NotFound("The user you are trying to add doesn't exist.");
			}
			var existingRequest = await db.FriendReqs.FirstOrDefaultAsync(fr =>
			fr.sendid == user.Id && fr.receiveid == dto.userid && fr.Status == "Pending");
			if (existingRequest != null)
			{
				return BadRequest("Friend request already sent.");
			}
			var friends = await db.Friends.FirstOrDefaultAsync(u => u.friendid == dto.userid);
			if (friends != null)
			{
				return BadRequest("Already Friends");
			}
			var FriendReq = new FriendReq
			{
				sendid = user.Id,
				senderfname=user.FirstName,
				senderlname=user.LastName,
				recieverfname=receiver.FirstName,
				recieverlname=receiver.LastName,
				receiveid = dto.userid,
				sendAt = DateTime.Now,
				Status="Pending"
			};
			await db.FriendReqs.AddAsync(FriendReq);
			await db.SaveChangesAsync();
			return Ok("Friend Request Sent Successfully ");
		}
		[Authorize]
		[HttpGet("ShowSentRequestForUser")]
		public async Task<IActionResult> ShowSentRequestForUser()
		{
			var user = await userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
			if (user == null)
			{
				return NotFound("User Not Found");
			}
			var req = await db.FriendReqs.Where(s => s.sendid == user.Id).ToListAsync();
			var lt = req.Select(item => new ShowSentReqDTO
			{
				//id = item.Id,
				FirstName = item.recieverfname,
				LastName = item.recieverlname,
				status = item.Status,
				sendAt = item.sendAt
			}).ToList();
			return Ok(lt);

		}
		[Authorize]
		[HttpGet("ShowRecieveRequestForUser")]
		public async Task<IActionResult> ShowRecieveRequestForUser()
		{
			var user = await userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
			if (user == null)
			{
				return NotFound("User Not Found");
			}
			var req = await db.FriendReqs.Where(s => s.receiveid == user.Id).ToListAsync();
			var lt = req.Select(item => new ShowSentReqDTO
			{
				id=item.Id,
				FirstName = item.senderfname,
				LastName = item.senderlname,
				status = item.Status,
				sendAt = item.sendAt
			}).ToList();
			return Ok(lt);
		}
		[Authorize]
		[HttpPost("Accept")]
		public async Task<IActionResult> Acceptreq([FromBody] int id)
		{
			if (id == null)
			{
				return BadRequest("must send the id");
			}
			var user = await userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
			if (user == null)
			{
				return NotFound("user Not found");
			}
			var item = await db.FriendReqs
			.Include(fr => fr.Sender)
			.Include(fr => fr.Receiver)
			.FirstOrDefaultAsync(fr => fr.Id == id);
			if (item == null)
			{
				return NotFound("Not Found item with this id");
			}
			var other = item.sendid == user.Id ? item.Receiver : item.Sender;
			if (item.Status== "Pending")
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

				await db.Friends.AddRangeAsync(friend1, friend2);
				db.FriendReqs.Remove(item);
				await db.SaveChangesAsync();
				return Ok("You Now are Friends");
			}
			return NotFound("this request are not found");
		}
		[Authorize]
		[HttpPost("reject")]
		public async Task<IActionResult> reject([FromBody] int id)
		{
			if (id == null)
			{
				return BadRequest("must send the id");
			}
			var user = userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
			if (user == null)
			{
				return NotFound("user Not found");
			}
			var item = await db.FriendReqs.FirstOrDefaultAsync(s => s.Id == id);
			if (item == null)
			{
				return NotFound("Not Found item with this id");
			}
			db.FriendReqs.Remove(item);
			await db.SaveChangesAsync();
			return Ok("the friend request has been declined");
		}
		[Authorize]
		[HttpGet("showthefriends")]
		public async Task<IActionResult> ShowFriendsUser()
		{
			var user = await userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
			if (user == null)
			{
				return NotFound("User Not Found");
			}
			var list = await db.Friends.Where(s => s.userid == user.Id).ToListAsync();

			var lt = list.Select(item => new ShowFrriendsDTO
			{
				name = item.friendName
			}).ToList();
			return Ok(lt);
		}

	}
}
