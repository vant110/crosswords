using System.Text;

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

        public async Task<Dictionary<string, string>> ReadWordsAsync(IFormFile? wordsFile, string encoding = "utf-8", string separator = " ", bool skipInvalid = false)
        {
            var words = new Dictionary<string, string>();

            if (wordsFile is not null
                && wordsFile.Length != 0)
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                using var reader = new StreamReader(wordsFile.OpenReadStream(), Encoding.GetEncoding(encoding));

                string? line;
                for (int lineNumber = 1; (line = await reader.ReadLineAsync()) is not null; lineNumber++)
                {
                    int separatorIndex = line.IndexOf(separator);

                    string wordName = line[..separatorIndex].ToUpperInvariant();
                    if (!_validationService.IsFileWordName(wordName, lineNumber, out string? message))
                    {
                        if (skipInvalid)
                            continue;
                        else
                            throw new ArgumentException(message);
                    }

                    string definition = line[(separatorIndex + separator.Length)..];
                    if (!_validationService.IsFileDefinition(definition, lineNumber, out message))
                    {
                        if (skipInvalid)
                            continue;
                        else
                            throw new ArgumentException(message);
                    }

                    if (!words.TryAdd(wordName, definition))
                    {
                        if (skipInvalid)
                            continue;
                        else
                            throw new ArgumentException($"Cлово '{wordName}' неуникально");
                    }
                }
            }

            return words;
        }

    }
}
