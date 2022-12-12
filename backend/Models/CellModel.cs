using Crosswords.Db.Models;
using Crosswords.Models.Enums;

namespace Crosswords.Models
{
    public class CellModel
    {
        public CellLockEnum Lock { get; set; }

        public Word? HWord { get; set; }
        public Word? VWord { get; set; }

        public int HLetterIndex { get; set; }
        public int VLetterIndex { get; set; }


        public override string? ToString()
        {
            if (HWord is not null)
            {
                return HWord.WordName[HLetterIndex].ToString();
            }
            else if (VWord is not null)
            {
                return VWord.WordName[VLetterIndex].ToString();
            }
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

        }

    }
}
