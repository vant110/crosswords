using System.Text;
using System.Text.RegularExpressions;

namespace Crosswords.Models
{
    public partial class MaskModel
    {
        private string mask;


        public string Full
        {
            get => mask;
            set
            {
                mask = value;

                // Формируем шаблон
                Left = null;
                Body = null;
                Right = null;

                var patternBuilder = new StringBuilder("^");

                var leftMatch = LeftRegex().Match(mask);

                if (leftMatch.Success)
                {
                    Left = mask[..leftMatch.Length];
                    patternBuilder.Append($".{{0,{leftMatch.Length}}}");
                }
                if (leftMatch.Length != mask.Length)
                {
                    var rightMatch = RightRegex().Match(mask);

                    Body = mask[leftMatch.Length..(mask.Length - rightMatch.Length)];
                    patternBuilder.Append($"({Body})");

                    if (rightMatch.Success)
                    {
                        Right = mask[rightMatch.Index..];
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

        public int Index { get; set; }


        [GeneratedRegex("^(\\.+)")]
        private static partial Regex LeftRegex();

        [GeneratedRegex("(\\.+)$")]
        private static partial Regex RightRegex();

    }
}
