using System.ComponentModel.DataAnnotations.Schema;

namespace Moodify.Models
{
	public class FriendReq
	{
		public int Id { get; set; }
		public string sendid { get; set; }
		public string senderfname { get; set; }
		public string senderlname { get; set; }
		public string recieverfname { get; set; }
		public string recieverlname { get; set; }
		public string receiveid { get; set; }
		public string Status { get; set; }
		public DateTime sendAt { get; set; }

		public string userid { get; set; }

		[ForeignKey(nameof(userid))]
		public virtual User user { get; set; }
	}
}
