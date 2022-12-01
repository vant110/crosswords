using System;
using System.Collections.Generic;

namespace Crosswords.Db.Models;

public partial class CrosswordWord
{
    public short CrosswordId { get; set; }

    public int WordId { get; set; }

    public short X1 { get; set; }

    public short Y1 { get; set; }

    public short X2 { get; set; }

    public short Y2 { get; set; }

    public virtual Crossword Crossword { get; set; } = null!;

    public virtual Word Word { get; set; } = null!;
}
