using Crosswords.Db.Models;
using System.Text.Json.Serialization;

namespace Crosswords.Models.DTOs
{
    public class CrosswordWordDTO
    {
        public int Id { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public string Name { get; set; }

        public string Definition { get; set; }

        public PointDTO<short> P1 { get; set; }

        public PointDTO<short> P2 { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool IsSolved { get; set; }


        public CrosswordWord ToCrosswordWord(Crossword crossword)
        {
            return new CrosswordWord
            {
                Crossword = crossword,
                WordId = Id,
                X1 = P1.X,
                Y1 = P1.Y,
                X2 = P2.X,
                Y2 = P2.Y
            };
        }

    }
}
