using System;
using System.Collections.Generic;

namespace Crosswords.Db.Models;

public partial class Word
{
    public int WordId { get; set; }

    public short DictionaryId { get; set; }

    public string WordName { get; set; } = null!;

    public string Definition { get; set; } = null!;

    public virtual ICollection<CrosswordWord> CrosswordWords { get; } = new List<CrosswordWord>();

    public virtual Dictionary Dictionary { get; set; } = null!;
}
