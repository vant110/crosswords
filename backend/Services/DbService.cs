using Crosswords.Db;
using Crosswords.Db.Models;
using Crosswords.Models;
using Crosswords.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Crosswords.Services
{
    public class DbService
    {
        private readonly CrosswordsContext _db;
        private readonly ValidationService _validationService;

        public DbService(
            CrosswordsContext db,
            ValidationService validationService)
        {
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

        public async Task<short> InsertCrosswordAsync(CrosswordDTO crosswordDTO)
        {
            var crossword = crosswordDTO.ToCrossword();
            _db.Crosswords.Add(crossword);

            _db.CrosswordWords.AddRange(crosswordDTO.Words
                .Select(w => w.ToCrosswordWord(crossword)));

            await _db.SaveChangesAsync();
            return crossword.CrosswordId;
        }

        public async Task UpdateCrosswordAsync(short id, CrosswordDTO crosswordDTO)
        {
            var crossword = await _db.Crosswords
                .AsTracking()
                .Where(c => c.CrosswordId == id)
                .Include(c => c.CrosswordWords)
                .Include(c => c.Saves)
                .Include(c => c.SolvedCrosswords)
                .SingleAsync();

            crossword.CrosswordName = crosswordDTO.Name;
            crossword.ThemeId = crosswordDTO.ThemeId;
            crossword.DictionaryId = crosswordDTO.DictionaryId;
            crossword.Width = crosswordDTO.Size.Width;
            crossword.Height = crosswordDTO.Size.Height;
            crossword.PromptCount = crosswordDTO.PromptCount;

            var newCrosswordWords = crosswordDTO.Words
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
            crossword.SolvedCrosswords.Clear();

            await _db.SaveChangesAsync();
        }

        #endregion

        #region Игрок - Кроссворды

        public async Task<CrosswordModel> SelectUnstartedCrosswordAsync(short id)
        {
            return await _db.Crosswords
                .Where(c => c.CrosswordId == id)
                .Select(c => new CrosswordModel(
                    c.Width,
                    c.Height,
                    c.PromptCount,
                    c.CrosswordWords
                        .Select(cw => new CrosswordWordDTO
                        {
                            Id = cw.WordId,
                            Name = cw.Word.WordName,
                            Definition = cw.Word.Definition,
                            P1 = new PointDTO<short>
                            {
                                X = cw.X1,
                                Y = cw.Y1,
                            },
                            P2 = new PointDTO<short>
                            {
                                X = cw.X2,
                                Y = cw.Y2,
                            },
                        })))
                .SingleAsync();
        }

        public async Task<CrosswordModel?> SelectStartedCrosswordAsync(short crosswordId, int playerId)
        {
            return await _db.Saves
                .Where(s => s.CrosswordId == crosswordId
                    && s.PlayerId == playerId)
                .Select(s => new CrosswordModel(
                    s.Crossword.Width,
                    s.Crossword.Height,
                    s.PromptCount,
                    s.Crossword.CrosswordWords
                        .Select(cw => new CrosswordWordDTO
                        {
                            Id = cw.WordId,
                            Name = cw.Word.WordName,
                            Definition = cw.Word.Definition,
                            P1 = new PointDTO<short>
                            {
                                X = cw.X1,
                                Y = cw.Y1,
                            },
                            P2 = new PointDTO<short>
                            {
                                X = cw.X2,
                                Y = cw.Y2,
                            },
                        }),
                    s.Letters
                        .Select(l => new Letter
                        {
                            X = l.X,
                            Y = l.Y,
                            LetterName = l.LetterName
                        })))
                .SingleOrDefaultAsync();
        }

        public async Task InsertLetterAsync(short crosswordId, int playerId, short x, short y, char letterName)
        {
            var letter = new Letter
            {
                CrosswordId = crosswordId,
                PlayerId = playerId,
                X = x,
                Y = y,
                LetterName = letterName
            };

            _db.Letters.Add(letter);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateLetterAsync(short crosswordId, int playerId, short x, short y, char letterName)
        {
            var letter = new Letter
            {
                CrosswordId = crosswordId,
                PlayerId = playerId,
                X = x,
                Y = y
            };
            _db.Letters.Attach(letter);
            letter.LetterName = letterName;

            await _db.SaveChangesAsync();
        }

        public async Task DeleteLetterAsync(short crosswordId, int playerId, short x, short y)
        {
            var letter = new Letter
            {
                CrosswordId = crosswordId,
                PlayerId = playerId,
                X = x,
                Y = y
            };
            _db.Letters.Remove(letter);
            await _db.SaveChangesAsync();
        }

        private void ReducePromptCount(short crosswordId, int playerId, short promptCount)
        {
            var save = new Save
            {
                CrosswordId = crosswordId,
                PlayerId = playerId,
                PromptCount = -1

            };
            _db.Saves.Attach(save);
            save.PromptCount = promptCount;
        }

        public async Task InsertPromptedLetterAsync(short crosswordId, int playerId, short promptCount, short x, short y, char letterName)
        {
            ReducePromptCount(crosswordId, playerId, promptCount);

            await InsertLetterAsync(crosswordId, playerId, x, y, letterName);
        }

        public async Task UpdatePromptedLetterAsync(short crosswordId, int playerId, short promptCount, short x, short y, char letterName)
        {
            ReducePromptCount(crosswordId, playerId, promptCount);

            await UpdateLetterAsync(crosswordId, playerId, x, y, letterName);
        }

        public async Task InsertSaveAsync(short crosswordId, int playerId, short promptCount, short x, short y, char letterName)
        {
            var save = new Save
            {
                CrosswordId = crosswordId,
                PlayerId = playerId,
                PromptCount = promptCount
            };
            _db.Saves.Add(save);

            await InsertLetterAsync(crosswordId, playerId, x, y, letterName);
        }

        public async Task InsertSolvedCrosswordAsync(short crosswordId, int playerId)
        {
            _db.SolvedCrosswords.Add(new SolvedCrossword
            {
                CrosswordId = crosswordId,
                PlayerId = playerId
            });

            _db.Saves.Remove(new Save
            {
                CrosswordId = crosswordId,
                PlayerId = playerId
            });

            await _db.SaveChangesAsync();
        }

        #endregion

    }
}
