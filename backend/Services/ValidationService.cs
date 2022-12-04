using System.Text.RegularExpressions;

namespace Crosswords.Services
{
    public partial class ValidationService
    {
        [GeneratedRegex("^[а-яА-Я]+$")]
        private static partial Regex WordNameRegex();


        public bool IsFileWordName(string input, int lineNumber, out string? message)
        {
            message = null;

            int minLength = 3;
            int maxLength = 15;
            if (input.Length < minLength)
                message = $"В строке {lineNumber} слово слишком короткое. Минимальное количество символов: {minLength}";
            else if (input.Length > maxLength)
                message = $"В строке {lineNumber} слово слишком длинное. Максимальное количество символов: {maxLength}";
            else if (!WordNameRegex().IsMatch(input))
                message = $"В строке {lineNumber} слово содержит недопустимые символы. Допустимые символы: русский алфавит";

            return message is null;
        }

        public bool IsFileDefinition(string input, int lineNumber, out string? message)
        {
            message = null;

            int minLength = 10;
            int maxLength = 200;
            if (input.Length < minLength)
                message = $"В строке {lineNumber} определение слишком короткое. Минимальное количество символов: {minLength}";
            else if (input.Length > maxLength)
                message = $"В строке {lineNumber} определение слишком длинное. Максимальное количество символов: {maxLength}";

            return message is null;
        }


    }
}
