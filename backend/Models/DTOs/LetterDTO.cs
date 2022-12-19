using System.Text.Json.Serialization;

namespace Crosswords.Models.DTOs
{
    public class LetterDTO
    {
        public short X { get; set; }

        public short Y { get; set; }

        public char L { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool IsPrompted { get; set; }

    }
}
