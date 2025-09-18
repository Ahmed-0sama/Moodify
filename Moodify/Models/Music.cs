using System;
using System.Collections.Generic;

namespace Moodify.Models;

public  class Music
{
    public int MusicId { get; set; }
	public string Title { get; set; }
	public string musicurl { get; set; }
	public string Category { get; set; }
	public int? Count { get; set; }

	public virtual ICollection<History> Histories { get; set; } = new List<History>();

	public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

	public ICollection<ArtistMusic> ArtistMusics { get; set; } = new List<ArtistMusic>();
}
