using System;
using System.Collections.Generic;

namespace Crosswords.Db.Models;

public partial class SolvedCrossword
{
    public short CrosswordId { get; set; }

    public int PlayerId { get; set; }

    public virtual Crossword Crossword { get; set; } = null!;

    public virtual Player Player { get; set; } = null!;
}
