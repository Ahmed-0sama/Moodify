using System;
using System.Collections.Generic;

namespace Moodify.Models;

public  class Favorite
{
	public string? Userid { get; set; }
	public int? Musicid { get; set; }

	public virtual Music? Music { get; set; }
	public virtual User? User { get; set; }
}
