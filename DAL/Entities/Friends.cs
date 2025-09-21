using System.ComponentModel.DataAnnotations.Schema;

namespace Moodify.Models
{
	public class Friends
	{
		public int id { get; set; }
		public string userid { get; set; }
		public string friendid { get; set; }
		public string friendName { get; set; }

		[ForeignKey(nameof(userid))]
		public virtual User user { get; set; }
	}
}
