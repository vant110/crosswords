namespace Crosswords.Services
{
    public class FileService
    {
        private readonly ValidationService _validationService;

        public FileService(
            ValidationService validationService)
        {
            _validationService = validationService;
        }

        public async Task<Dictionary<string, string>> ReadWordsAsync(IFormFile? wordsFile)
        {
            var words = new Dictionary<string, string>();

            if (wordsFile is not null
                && wordsFile.Length != 0)
            {
                using var reader = new StreamReader(wordsFile.OpenReadStream());
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

                    if (!words.TryAdd(wordName, definition))
                        throw new ArgumentException($"Cлово '{wordName}' неуникально");
                }
            }

            return words;
        }

    }
}
