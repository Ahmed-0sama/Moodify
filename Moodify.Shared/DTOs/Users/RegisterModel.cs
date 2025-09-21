using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moodify.Shared.DTOs.Users
{
	public class RegisterModel
	{
		public string Username { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
		public string Fname { get; set; }
		public string Lname { get; set; }
	}
}
