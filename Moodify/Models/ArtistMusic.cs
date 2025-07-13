using System;
using System.Collections.Generic;

namespace Moodify.Models;

public  class ArtistMusic
{
    public int ArtistId { get; set; }

    public int MusicId { get; set; }

    public virtual Artist Artist { get; set; }

    public virtual Music Music { get; set; }
}
