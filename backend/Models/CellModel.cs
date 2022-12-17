using Crosswords.Models.Enums;

namespace Crosswords.Models
{
    public class CellModel
    {
        public CellLockEnum Lock { get; set; }

        public WordModel? HWord { get; set; }
        public WordModel? VWord { get; set; }

        public int HIndex { get; set; }
        public int VIndex { get; set; }

        public char Input { get; set; }

        public bool IsSolved { get; set; }


        public void SetInput(char input)
        {
            Input = input;

            IsSolved = HWord is not null
                ? HWord.Name[HIndex] == Input
                : VWord is not null && VWord.Name[VIndex] == Input;
        }

        public override string? ToString()
        {
            if (HWord is not null)
            {
                return HWord.Name[HIndex].ToString();
            }
            else if (VWord is not null)
            {
                return VWord.Name[VIndex].ToString();
            }
            /*
            else if ((Lock & CellLockEnum.Horizontally) != CellLockEnum.None)
            {
                if ((Lock & CellLockEnum.Vertically) != CellLockEnum.None)
                {
                    return "+";
                }
                else
                {
                    return "-";
                }
            }
            else if ((Lock & CellLockEnum.Vertically) != CellLockEnum.None)
            {
                return "|";
            }
            else
            {
                return "*";
            }
            */
            else
            {
                return " ";
            }
        }

    }
}
