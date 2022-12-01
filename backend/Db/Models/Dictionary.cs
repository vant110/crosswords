using System;
using System.Collections.Generic;

namespace Crosswords.Db.Models;

public partial class Dictionary
{
    public short DictionaryId { get; set; }

    public string DictionaryName { get; set; } = null!;

    public virtual ICollection<Crossword> Crosswords { get; } = new List<Crossword>();

    public virtual ICollection<Word> Words { get; } = new List<Word>();
}
