using System;
using System.Collections.Generic;

namespace Crosswords.Db.Models;

public partial class Player
{
    public int PlayerId { get; set; }

    public string Login { get; set; } = null!;

    public byte[] PasswordHash { get; set; } = null!;

    public virtual ICollection<Save> Saves { get; } = new List<Save>();

    public virtual ICollection<Crossword> Crosswords { get; } = new List<Crossword>();
}
