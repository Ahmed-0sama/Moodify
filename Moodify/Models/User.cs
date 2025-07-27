using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Moodify.Models;

public class User:IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
	public string? RefreshToken { get; set; }
	public DateTime RefreshTokenExpirytime { get; set; }

	public byte[]? Photo { get; set; }
    public virtual ICollection<Favorite> Favorite { get; set; } = new List<Favorite>();
    public virtual ICollection<History> Histories { get; set; } = new List<History>();
    public virtual ICollection<Friends> Friends { get; set; } = new List<Friends>();
    public virtual ICollection<FriendReq> SentFriendRequests { get; set; } = new List<FriendReq>();
	public virtual ICollection<FriendReq> ReceivedFriendRequests { get; set; }

}
