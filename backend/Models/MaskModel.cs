using System.Text;
using System.Text.RegularExpressions;

namespace Crosswords.Models
{
    public partial class MaskModel
    {
        private string _mask;


        public string Full
        {
            get => _mask;
            set
            {
                _mask = value;

                // Формируем шаблон
                Left = null;
                Body = null;
                Right = null;

                var patternBuilder = new StringBuilder("^");

                var leftMatch = LeftRegex().Match(_mask);

                if (leftMatch.Success)
                {
                    Left = _mask[..leftMatch.Length];
                    patternBuilder.Append($".{{0,{leftMatch.Length}}}");
                }
                if (leftMatch.Length != _mask.Length)
                {
                    var rightMatch = RightRegex().Match(_mask);

                    Body = _mask[leftMatch.Length..(_mask.Length - rightMatch.Length)];
                    patternBuilder.Append($"({Body})");

                    if (rightMatch.Success)
                    {
                        Right = _mask[rightMatch.Index..];
                        patternBuilder.Append($".{{0,{rightMatch.Length}}}");
                    }
                }
                patternBuilder.Append('$');

                Pattern = patternBuilder.ToString();
            }
        }
        public string? Left { get; private set; }
        public string? Body { get; private set; }
        public string? Right { get; private set; }
        public string Pattern { get; private set; }

        public int Offset { get; set; }


        [GeneratedRegex("^(\\.+)")]
        private static partial Regex LeftRegex();

        [GeneratedRegex("(\\.+)$")]
        private static partial Regex RightRegex();

    }
}
