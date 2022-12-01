using System;
using System.Collections.Generic;
using Crosswords.Db.Models;
using Microsoft.EntityFrameworkCore;

namespace Crosswords.Db;

public partial class CrosswordsContext : DbContext
{
    public CrosswordsContext(DbContextOptions<CrosswordsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Crossword> Crosswords { get; set; }

    public virtual DbSet<CrosswordWord> CrosswordWords { get; set; }

    public virtual DbSet<Dictionary> Dictionaries { get; set; }

    public virtual DbSet<Letter> Letters { get; set; }

    public virtual DbSet<Player> Players { get; set; }

    public virtual DbSet<Save> Saves { get; set; }

    public virtual DbSet<Theme> Themes { get; set; }

    public virtual DbSet<Word> Words { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Crossword>(entity =>
        {
            entity.HasKey(e => e.CrosswordId).HasName("crosswords_pkey");

            entity.ToTable("crosswords");

            entity.HasIndex(e => e.CrosswordName, "crosswords_crossword_name_key").IsUnique();

            entity.Property(e => e.CrosswordId).HasColumnName("crossword_id");
            entity.Property(e => e.CrosswordName)
                .HasMaxLength(30)
                .HasColumnName("crossword_name");
            entity.Property(e => e.DictionaryId).HasColumnName("dictionary_id");
            entity.Property(e => e.HorizontalSize).HasColumnName("horizontal_size");
            entity.Property(e => e.PromptCount).HasColumnName("prompt_count");
            entity.Property(e => e.ThemeId).HasColumnName("theme_id");
            entity.Property(e => e.VerticalSize).HasColumnName("vertical_size");

            entity.HasOne(d => d.Dictionary).WithMany(p => p.Crosswords)
                .HasForeignKey(d => d.DictionaryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("crosswords_dictionary_id_fkey");

            entity.HasOne(d => d.Theme).WithMany(p => p.Crosswords)
                .HasForeignKey(d => d.ThemeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("crosswords_theme_id_fkey");
        });

        modelBuilder.Entity<CrosswordWord>(entity =>
        {
            entity.HasKey(e => new { e.CrosswordId, e.WordId }).HasName("crossword_words_pkey");

            entity.ToTable("crossword_words");

            entity.Property(e => e.CrosswordId).HasColumnName("crossword_id");
            entity.Property(e => e.WordId).HasColumnName("word_id");
            entity.Property(e => e.X1).HasColumnName("x1");
            entity.Property(e => e.X2).HasColumnName("x2");
            entity.Property(e => e.Y1).HasColumnName("y1");
            entity.Property(e => e.Y2).HasColumnName("y2");

            entity.HasOne(d => d.Crossword).WithMany(p => p.CrosswordWords)
                .HasForeignKey(d => d.CrosswordId)
                .HasConstraintName("crossword_words_crossword_id_fkey");

            entity.HasOne(d => d.Word).WithMany(p => p.CrosswordWords)
                .HasForeignKey(d => d.WordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("crossword_words_word_id_fkey");
        });

        modelBuilder.Entity<Dictionary>(entity =>
        {
            entity.HasKey(e => e.DictionaryId).HasName("dictionaries_pkey");

            entity.ToTable("dictionaries");

            entity.HasIndex(e => e.DictionaryName, "dictionaries_dictionary_name_key").IsUnique();

            entity.Property(e => e.DictionaryId).HasColumnName("dictionary_id");
            entity.Property(e => e.DictionaryName)
                .HasMaxLength(30)
                .HasColumnName("dictionary_name");
        });

        modelBuilder.Entity<Letter>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.CrosswordId, e.X, e.Y }).HasName("letters_pkey");

            entity.ToTable("letters");

            entity.Property(e => e.PlayerId).HasColumnName("player_id");
            entity.Property(e => e.CrosswordId).HasColumnName("crossword_id");
            entity.Property(e => e.X).HasColumnName("x");
            entity.Property(e => e.Y).HasColumnName("y");
            entity.Property(e => e.LetterName)
                .HasMaxLength(1)
                .HasColumnName("letter_name");

            entity.HasOne(d => d.Save).WithMany(p => p.Letters)
                .HasForeignKey(d => new { d.PlayerId, d.CrosswordId })
                .HasConstraintName("letters_player_id_crossword_id_fkey");
        });

        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.PlayerId).HasName("players_pkey");

            entity.ToTable("players");

            entity.HasIndex(e => e.Login, "players_login_key").IsUnique();

            entity.Property(e => e.PlayerId).HasColumnName("player_id");
            entity.Property(e => e.Login)
                .HasMaxLength(10)
                .HasColumnName("login");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");

            entity.HasMany(d => d.Crosswords).WithMany(p => p.Players)
                .UsingEntity<Dictionary<string, object>>(
                    "SolvedCrossword",
                    r => r.HasOne<Crossword>().WithMany()
                        .HasForeignKey("CrosswordId")
                        .HasConstraintName("solved_crosswords_crossword_id_fkey"),
                    l => l.HasOne<Player>().WithMany()
                        .HasForeignKey("PlayerId")
                        .HasConstraintName("solved_crosswords_player_id_fkey"),
                    j =>
                    {
                        j.HasKey("PlayerId", "CrosswordId").HasName("solved_crosswords_pkey");
                        j.ToTable("solved_crosswords");
                    });
        });

        modelBuilder.Entity<Save>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.CrosswordId }).HasName("saves_pkey");

            entity.ToTable("saves");

            entity.Property(e => e.PlayerId).HasColumnName("player_id");
            entity.Property(e => e.CrosswordId).HasColumnName("crossword_id");
            entity.Property(e => e.PromptCount).HasColumnName("prompt_count");

            entity.HasOne(d => d.Crossword).WithMany(p => p.Saves)
                .HasForeignKey(d => d.CrosswordId)
                .HasConstraintName("saves_crossword_id_fkey");

            entity.HasOne(d => d.Player).WithMany(p => p.Saves)
                .HasForeignKey(d => d.PlayerId)
                .HasConstraintName("saves_player_id_fkey");
        });

        modelBuilder.Entity<Theme>(entity =>
        {
            entity.HasKey(e => e.ThemeId).HasName("themes_pkey");

            entity.ToTable("themes");

            entity.HasIndex(e => e.ThemeName, "themes_theme_name_key").IsUnique();

            entity.Property(e => e.ThemeId).HasColumnName("theme_id");
            entity.Property(e => e.ThemeName)
                .HasMaxLength(30)
                .HasColumnName("theme_name");
        });

        modelBuilder.Entity<Word>(entity =>
        {
            entity.HasKey(e => e.WordId).HasName("words_pkey");

            entity.ToTable("words");

            entity.HasIndex(e => new { e.DictionaryId, e.WordName }, "words_dictionary_id_word_name_key").IsUnique();

            entity.Property(e => e.WordId).HasColumnName("word_id");
            entity.Property(e => e.Definition)
                .HasMaxLength(200)
                .HasColumnName("definition");
            entity.Property(e => e.DictionaryId).HasColumnName("dictionary_id");
            entity.Property(e => e.WordName)
                .HasMaxLength(15)
                .HasColumnName("word_name");

            entity.HasOne(d => d.Dictionary).WithMany(p => p.Words)
                .HasForeignKey(d => d.DictionaryId)
                .HasConstraintName("words_dictionary_id_fkey");
        });
        modelBuilder.HasSequence<short>("themes_theme_id_seq");

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
