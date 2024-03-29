﻿using System;
using System.Collections.Generic;

namespace Crosswords.Db.Models;

public partial class Player
{
    public int PlayerId { get; set; }

    public string Login { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public virtual ICollection<Save> Saves { get; } = new List<Save>();

    public virtual ICollection<SolvedCrossword> SolvedCrosswords { get; } = new List<SolvedCrossword>();
}
