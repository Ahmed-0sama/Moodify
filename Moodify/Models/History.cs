using System;
using System.Collections.Generic;

namespace Moodify.Models;

public  class History
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? MusicId { get; set; }

    public DateTime? ListenedAt { get; set; }

    public virtual Music Music { get; set; }

    public virtual User User { get; set; }
}
