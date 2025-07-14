using System;
using System.Collections.Generic;

namespace Moodify.Models;

public  class Artist
{
    public int ArtistId { get; set; }

    public string? ArtistName { get; set; }
	public ICollection<ArtistMusic> ArtistMusics { get; set; } = new List<ArtistMusic>();
}
