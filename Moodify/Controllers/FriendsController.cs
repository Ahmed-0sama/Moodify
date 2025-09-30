using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Moodify.BAL.Interfaces;
using Moodify.BAL.Services;
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
		private IFriendService _friendService;
		public FriendsController(UserManager<User> userManager,IConfiguration configuration, MoodifyDbContext db,IFriendService friendService)
		{
			_friendService = friendService;
		}
		[Authorize]
		[HttpGet("SearchForFriend/{query}")]
		public async Task<IActionResult> SearchforFriend( string query ,int pageNumber = 1, int pageSize = 10)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId)) return Unauthorized();

			if (string.IsNullOrWhiteSpace(query))
			{
				return BadRequest("Search query cannot be empty");
			}

			var result = await _friendService.SearchForFriendAsync(userId, query, pageNumber, pageSize);
			return Ok(result);
		}
		[Authorize]
		[HttpPost("SendFriendRequest")]
		public async Task<IActionResult> SendFriendRequest([FromBody]SendFriendRequestDTO dto)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null) return Unauthorized();
			var result = await _friendService.SendFriendRequestAsync(userId, dto.userid);
			if (result == "Friend request sent successfully")
			{
				return Ok(result);
			}
			return BadRequest(result);
		}
		[Authorize]
		[HttpGet("ShowSentRequestForUser")]
		public async Task<IActionResult> ShowSentRequestForUser()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null) return Unauthorized();
			var result=await _friendService.GetSentRequestsAsync(userId);
			return Ok(result);

		}
		[Authorize]
		[HttpGet("ShowRecieveRequestForUser")]
		public async Task<IActionResult> ShowRecieveRequestForUser()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null) return Unauthorized();
			var result = await _friendService.GetReceivedRequestsAsync(userId);
			return Ok(result);
		}
		[Authorize]
		[HttpPost("Accept")]
		public async Task<IActionResult> Acceptreq([FromBody] int id)
		{
			if (id <= 0)
			{
				return BadRequest("Must send a valid request id");
			}

			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized();
			}

			var result = await _friendService.AcceptRequestAsync(userId, id);
			return Ok(result); ;
		}
		[Authorize]
		[HttpPost("reject")]
		public async Task<IActionResult> reject([FromBody] int id)
		{
			if (id == null)
			{
				return BadRequest("must send the id");
			}
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var reult= await _friendService.RejectRequestAsync(userId, id);
			return Ok(reult);
		}
		[Authorize]
		[HttpGet("showthefriends")]
		public async Task<IActionResult> ShowFriendsUser()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null) return Unauthorized();
			var result = await _friendService.GetFriendsAsync(userId);
			return Ok(result);
		}

	}
}
