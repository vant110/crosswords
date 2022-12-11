using System.Text.RegularExpressions;

namespace Crosswords.Services
{
    public partial class ValidationService
    {
        #region Слово

        [GeneratedRegex("^[А-ЯЁ]+$")]
        private static partial Regex WordNameRegex();

        [GeneratedRegex("^[а-яА-ЯЁё(\"]")]
        private static partial Regex DefinitionRegex();

        private const int MinWordNameLength = 3;
        private const int MaxWordNameLength = 15;
        private const int MinDefinitionLength = 10;
        private const int MaxDefinitionLength = 200;

        public bool IsFileWordName(string input, int lineNumber, out string? message)
        {
            message = null;

            if (input.Length < MinWordNameLength)
                message = $"В строке {lineNumber} слово '{input}' слишком короткое. Минимальное количество символов: {MinWordNameLength}";
            else if (input.Length > MaxWordNameLength)
                message = $"В строке {lineNumber} слово '{input[..MaxWordNameLength]}...' слишком длинное. Максимальное количество символов: {MaxWordNameLength}";
            else if (!WordNameRegex().IsMatch(input))
                message = $"В строке {lineNumber} слово '{input}' содержит недопустимые символы. Допустимые символы: русский алфавит";

            return message is null;
        }

        public bool IsFileDefinition(string input, int lineNumber, out string? message)
        {
            message = null;

            if (input.Length < MinDefinitionLength)
                message = $"В строке {lineNumber} определение '{input}' слишком короткое. Минимальное количество символов: {MinDefinitionLength}";
            else if (input.Length > MaxDefinitionLength)
                message = $"В строке {lineNumber} определение '{input[..MaxDefinitionLength]}...' слишком длинное. Максимальное количество символов: {MaxDefinitionLength}";
            else if (!DefinitionRegex().IsMatch(input))
                message = $"В строке {lineNumber} определение '{input}' начинается с недопустимого символа. Допустимые символы: русский алфавит, (, \"";

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
            else if (!DefinitionRegex().IsMatch(input))
                message = $"Определение начинается с недопустимого символа. Допустимые символы: русский алфавит, (, \"";

            return message is null;
        }

        #endregion

    }
}
