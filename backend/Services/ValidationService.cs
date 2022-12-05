using System.Text.RegularExpressions;

namespace Crosswords.Services
{
    public partial class ValidationService
    {
        #region Слово

        [GeneratedRegex("^[А-Я]+$")]
        private static partial Regex WordNameRegex();

        private const int MinWordNameLength = 3;
        private const int MaxWordNameLength = 15;
        private const int MinDefinitionLength = 10;
        private const int MaxDefinitionLength = 200;

        public bool IsFileWordName(string input, int lineNumber, out string? message)
        {
            message = null;

            if (input.Length < MinWordNameLength)
                message = $"В строке {lineNumber} слово слишком короткое. Минимальное количество символов: {MinWordNameLength}";
            else if (input.Length > MaxWordNameLength)
                message = $"В строке {lineNumber} слово слишком длинное. Максимальное количество символов: {MaxWordNameLength}";
            else if (!WordNameRegex().IsMatch(input))
                message = $"В строке {lineNumber} слово содержит недопустимые символы. Допустимые символы: русский алфавит";

            return message is null;
        }

        public bool IsFileDefinition(string input, int lineNumber, out string? message)
        {
            message = null;

            if (input.Length < MinDefinitionLength)
                message = $"В строке {lineNumber} определение слишком короткое. Минимальное количество символов: {MinDefinitionLength}";
            else if (input.Length > MaxDefinitionLength)
                message = $"В строке {lineNumber} определение слишком длинное. Максимальное количество символов: {MaxDefinitionLength}";

            return message is null;
        }

        public bool IsWordName(string input, out string? message)
        {
            message = null;

            if (input.Length < MinWordNameLength)
                message = $"Слово слишком короткое. Минимальное количество символов: {MinWordNameLength}";
            else if (input.Length > MaxWordNameLength)
                message = $"Слово слишком длинное. Максимальное количество символов: {MaxWordNameLength}";
            else if (!WordNameRegex().IsMatch(input))
                message = $"Слово содержит недопустимые символы. Допустимые символы: русский алфавит";

            return message is null;
        }

        public bool IsDefinition(string input, out string? message)
        {
            message = null;

            if (input.Length < MinDefinitionLength)
                message = $"Определение слишком короткое. Минимальное количество символов: {MinDefinitionLength}";
            else if (input.Length > MaxDefinitionLength)
                message = $"Определение слишком длинное. Максимальное количество символов: {MaxDefinitionLength}";

            return message is null;
        }

        #endregion

    }
}
