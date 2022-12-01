using System;
using System.Collections.Generic;

namespace Crosswords.Db.Models;

public partial class Theme
{
    public short ThemeId { get; set; }

    public string ThemeName { get; set; } = null!;

    public virtual ICollection<Crossword> Crosswords { get; } = new List<Crossword>();
}
