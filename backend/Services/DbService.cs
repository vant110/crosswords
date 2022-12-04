using Crosswords.Db;
using Crosswords.Db.Models;
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

                    string wordName = line[..separatorIndex];
                    if (!_validationService.IsFileWordName(wordName, lineNumber, out string? message))
                        throw new ArgumentException(message);

                    string definition = line[(separatorIndex + 1)..];
                    if (!_validationService.IsFileDefinition(definition, lineNumber, out message))
                        throw new ArgumentException(message);

                    _db.Words.Add(new()
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

        #endregion

    }
}
