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
		public async Task<IActionResult> SearchforFriend( string query)
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
			var matchedUsers = await db.Users.Where(u => u.Id != user.Id &&
			  (u.FirstName.Contains(query) || u.LastName.Contains(query)))
			 .Select(u => new
			 {
			  u.Id,
			  u.FirstName,
			  u.LastName
			 })
				 .ToListAsync();
			return Ok(matchedUsers);
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
			var req = await db.FriendReqs.Where(s => s.sendid == user.Id).ToListAsync();
			var lt = req.Select(item => new ShowSentReqDTO
			{
				FirstName = item.senderfname,
				LastName = item.senderlname,
				status = item.Status,
				sendAt = item.sendAt
			}).ToList();
			return Ok(lt);

		}

	}
}
