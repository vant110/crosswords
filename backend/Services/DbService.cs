using Crosswords.Db;
using Crosswords.Db.Models;
using Microsoft.EntityFrameworkCore;

namespace Crosswords.Services
{
    public class DbService
    {
        private readonly ILogger<DbService> _logger;
        private readonly CrosswordsContext _db;

        public DbService(
            ILogger<DbService> logger,
            CrosswordsContext db)
        {
            _logger = logger;
            _db = db;
        }

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

    }
}
