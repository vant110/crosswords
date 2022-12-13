using Crosswords.Db.Models;

namespace Crosswords.Models.DTOs
{
    public class CrosswordDTO
    {
        public string Name { get; set; }

        public short ThemeId { get; set; }

        public short DictionaryId { get; set; }

        public SizeDTO<short> Size { get; set; }

        public short PromptCount { get; set; }

        public List<CrosswordWordDTO> Words { get; set; }

        public Crossword ToCrossword()
        {
            return new Crossword
            {
                CrosswordName = Name,
                ThemeId = ThemeId,
                DictionaryId = DictionaryId,
                Width = Size.Width,
                Height = Size.Height,
                PromptCount = PromptCount
            };
        }

    }
}
