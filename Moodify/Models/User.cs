using System;
using System.Collections.Generic;

namespace Moodify.Models;

public partial class User
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public byte[]? Photo { get; set; }
    public virtual ICollection<Favorite> Favorite { get; set; } = new List<Favorite>();
    public virtual ICollection<History> Histories { get; set; } = new List<History>();
}
