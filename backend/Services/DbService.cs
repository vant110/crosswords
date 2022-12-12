using Crosswords.Db;
using Crosswords.Db.Models;
using Crosswords.Models.Client;
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

        public async Task<short> InsertDictionaryAsync(string name, Dictionary<string, string> words)
        {
            var dictionary = new Dictionary
            {
                DictionaryName = name
            };
            _db.Dictionaries.Add(dictionary);
            
            _db.Words.AddRange(words.Select(w => new Word
            {
                Dictionary = dictionary,
                WordName = w.Key,
                Definition = w.Value
            }));

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
            name = name.ToUpper();
            if (!_validationService.IsWordName(name, out string? message))
                throw new ArgumentException(message);

            definition = char.ToUpper(definition[0]) + definition[1..];
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
            definition = char.ToUpper(definition[0]) + definition[1..];
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

        public async Task UpdateCrosswordAsync(short id, CrosswordModel crosswordModel)
        {
            var crossword = await _db.Crosswords
                .AsTracking()
                .Where(c => c.CrosswordId == id)
                .Include(c => c.CrosswordWords)
                .Include(c => c.Saves) // !!!
                .Include(c => c.Players) // !!!
                .SingleAsync();

            crossword.CrosswordName = crosswordModel.Name;
            crossword.ThemeId = crosswordModel.ThemeId;
            crossword.DictionaryId = crosswordModel.DictionaryId;
            crossword.Width = crosswordModel.Size.Width;
            crossword.Height = crosswordModel.Size.Height;
            crossword.PromptCount = crosswordModel.PromptCount;

            var newCrosswordWords = crosswordModel.Words
                .Select(w => w.ToCrosswordWord(crossword))
                .ToList();
            foreach (var cw in crossword.CrosswordWords)
            {
                var newCW = newCrosswordWords
                    .Where(w => w.WordId == cw.WordId)
                    .SingleOrDefault();

                if (newCW is null)
                {
                    // Удаление
                    _db.CrosswordWords.Remove(cw);
                }
                else
                {
                    // Изменение
                    cw.X1 = newCW.X1;
                    cw.Y1 = newCW.Y1;
                    cw.X2 = newCW.X2;
                    cw.Y2 = newCW.Y2;

                    newCrosswordWords.Remove(newCW);
                }
            }
            // Добавление
            _db.CrosswordWords.AddRange(newCrosswordWords);

            crossword.Saves.Clear();
            crossword.Players.Clear();

            await _db.SaveChangesAsync();
        }

        #endregion

    }
}
