using System;
using System.Collections.Generic;

namespace Crosswords.Db.Models;

public partial class Crossword
{
    public short CrosswordId { get; set; }

    public string CrosswordName { get; set; } = null!;

    public short ThemeId { get; set; }

    public short DictionaryId { get; set; }

    public short Width { get; set; }

    public short Height { get; set; }

    public short PromptCount { get; set; }

    public virtual ICollection<CrosswordWord> CrosswordWords { get; } = new List<CrosswordWord>();

    public virtual Dictionary Dictionary { get; set; } = null!;

    public virtual ICollection<Save> Saves { get; } = new List<Save>();

    public virtual ICollection<SolvedCrossword> SolvedCrosswords { get; } = new List<SolvedCrossword>();

    public virtual Theme Theme { get; set; } = null!;
}
