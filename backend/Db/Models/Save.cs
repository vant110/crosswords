using System;
using System.Collections.Generic;

namespace Crosswords.Db.Models;

public partial class Save
{
    public int PlayerId { get; set; }

    public short CrosswordId { get; set; }

    public short PromptCount { get; set; }

    public virtual Crossword Crossword { get; set; } = null!;

    public virtual ICollection<Letter> Letters { get; } = new List<Letter>();

    public virtual Player Player { get; set; } = null!;
}
