using Crosswords.Db.Models;

namespace Crosswords.Models.Client
{
    public class CrosswordWordModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Definition { get; set; }

        public PointModel<short> P1 { get; set; }

        public PointModel<short> P2 { get; set; }


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
