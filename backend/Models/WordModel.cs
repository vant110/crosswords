namespace Crosswords.Models
{
    public class WordModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Definition { get; set; }

        public bool IsSolved { get; set; }

    }
}
