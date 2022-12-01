using System;
using System.Collections.Generic;

namespace Crosswords.Db.Models;

public partial class Letter
{
    public int PlayerId { get; set; }

    public short CrosswordId { get; set; }

    public short X { get; set; }

    public short Y { get; set; }

    public char LetterName { get; set; }

    public virtual Save Save { get; set; } = null!;
}
