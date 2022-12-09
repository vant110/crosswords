﻿using Crosswords.Db;
using Crosswords.Db.Models;
using Crosswords.Models;
using Microsoft.EntityFrameworkCore;

namespace Crosswords.Services
{
    public class DbService
    {
        private readonly ILogger<DbService> _logger;
        private readonly CrosswordsContext _db;
        private readonly ValidationService _validationService;

        public DbService(
            ILogger<DbService> logger,
            CrosswordsContext db,
            ValidationService validationService)
        {
            _logger = logger;
            _db = db;
            _validationService = validationService;
        }

        #region Пользователь

        public async Task<int> InsertPlayerAsync(string login, string passwordHash)
        {
            var player = new Player
            {
                Login = login,
                PasswordHash = passwordHash
            };

            _db.Players.Add(player);
            await _db.SaveChangesAsync();

            return player.PlayerId;
        }

        public async Task<Player?> SelectPlayerAsync(string login)
        {
            return await _db.Players
                .Where(p => p.Login == login)
                .SingleOrDefaultAsync();
        }

        #endregion

        #region Администратор - Словари

        public async Task<short> InsertDictionaryAsync(string name, IFormFile? dictionaryFile)
        {
            var dictionary = new Dictionary
            {
                DictionaryName = name
            };
            _db.Dictionaries.Add(dictionary);
            // Слова
            if (dictionaryFile is not null
                && dictionaryFile.Length != 0)
            {
                using var reader = new StreamReader(dictionaryFile.OpenReadStream());
                string? line;
                for (int lineNumber = 1; (line = await reader.ReadLineAsync()) is not null; lineNumber++)
                {
                    int separatorIndex = line.IndexOf(' ');

                    string wordName = line[..separatorIndex].ToUpperInvariant();
                    if (!_validationService.IsFileWordName(wordName, lineNumber, out string? message))
                        throw new ArgumentException(message);

                    string definition = line[(separatorIndex + 1)..];
                    if (!_validationService.IsFileDefinition(definition, lineNumber, out message))
                        throw new ArgumentException(message);

                    _db.Words.Add(new Word
                    {
                        Dictionary = dictionary,
                        WordName = wordName,
                        Definition = definition
                    });
                }
            }

            await _db.SaveChangesAsync();
            return dictionary.DictionaryId;
        }

        public async Task UpdateDictionaryAsync(short id, string name)
        {
            var dictionary = new Dictionary
            {
                DictionaryId = id
            };
            _db.Dictionaries.Attach(dictionary);
            dictionary.DictionaryName = name;
            await _db.SaveChangesAsync();
        }

        #endregion

        #region Администратор - Слова словаря

        public async Task<int> InsertWordAsync(short dictionaryId, string name, string definition)
        {
            name = name.ToUpperInvariant();

            if (!_validationService.IsWordName(name, out string? message))
                throw new ArgumentException(message);

            if (!_validationService.IsDefinition(definition, out message))
                throw new ArgumentException(message);

            var word = new Word
            {
                DictionaryId = dictionaryId,
                WordName = name,
                Definition = definition
            };

            _db.Words.Add(word);
            await _db.SaveChangesAsync();

            return word.WordId;
        }

        public async Task UpdateWordAsync(int id, string definition)
        {
            if (!_validationService.IsDefinition(definition, out string? message))
                throw new ArgumentException(message);

            var word = new Word
            {
                WordId = id
            };
            _db.Words.Attach(word);
            word.Definition = definition;
            await _db.SaveChangesAsync();
        }

        #endregion

        #region Администратор - Темы

        public async Task<short> InsertThemeAsync(string name)
        {
            var theme = new Theme
            {
                ThemeName = name
            };

            _db.Themes.Add(theme);
            await _db.SaveChangesAsync();

            return theme.ThemeId;
        }

        public async Task UpdateThemeAsync(short id, string name)
        {
            var theme = new Theme
            {
                ThemeId = id
            };
            _db.Themes.Attach(theme);
            theme.ThemeName = name;
            await _db.SaveChangesAsync();
        }


        #endregion

        #region Администратор - Кроссворды

        public async Task<short> InsertCrosswordAsync(CrosswordModel crosswordModel)
        {
            var crossword = crosswordModel.ToCrossword();
            _db.Crosswords.Add(crossword);

            _db.CrosswordWords.AddRange(crosswordModel.Words
                .Select(w => w.ToCrosswordWord(crossword)));

            await _db.SaveChangesAsync();
            return crossword.CrosswordId;
        }

        #endregion

    }
}
