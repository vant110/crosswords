using Crosswords.Models.DTOs;
using Crosswords.Models.Enums;
using Crosswords.Services;
using System.Text;
using System.Text.RegularExpressions;

namespace Crosswords.Models
{
    public partial class CrosswordModel
    {
        public int Width { get; }
        public int Height { get; }
        public CellModel[,] Cells { get; }

        public int PromptCount { get; set; }


        public CrosswordModel(int width, int height)
        {
            Width = width;
            Height = height;

            Cells = new CellModel[Width, Height];
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Cells[x, y] = new CellModel();
                }
            }
        }


        [GeneratedRegex("[А-ЯЁ]")]
        private static partial Regex ValidMaskRegex();

        [GeneratedRegex("(?n:[А-ЯЁ]\\.*){3}")]
        private static partial Regex ValidMaskBodyRegex();


        private List<MaskModel> CreateMasks(SideEnum orientation, int constIdx)
        {
            var masks = new List<MaskModel>();

            var maskBuilder = new StringBuilder();
            switch (orientation)
            {
                case SideEnum.Up:
                case SideEnum.Down:
                    for (int x = 0; x < Width; x++)
                    {
                        var cell = Cells[x, constIdx];

                        if ((cell.Lock & CellLockEnum.Horizontally) != CellLockEnum.None)
                        {
                            string mask = maskBuilder.ToString();

                            if (mask.Length >= ValidationService.MinWordNameLength
                                && ValidMaskRegex().IsMatch(mask))
                            {
                                masks.Add(new MaskModel
                                {
                                    Full = mask,
                                    Index = x - mask.Length
                                });
                            }

                            maskBuilder.Clear();
                        }
                        else
                        {
                            if (cell.VWord is not null)
                            {
                                maskBuilder.Append(cell.VWord.Name[cell.VIndex]);
                            }
                            else
                            {
                                maskBuilder.Append('.');
                            }

                            if (x == Width - 1)
                            {
                                string mask = maskBuilder.ToString();

                                if (mask.Length >= ValidationService.MinWordNameLength
                                    && ValidMaskRegex().IsMatch(mask))
                                {
                                    masks.Add(new MaskModel
                                    {
                                        Full = mask,
                                        Index = Width - mask.Length
                                    });
                                }
                            }
                        }
                    }
                    break;
                case SideEnum.Left:
                case SideEnum.Right:
                    for (int y = 0; y < Height; y++)
                    {
                        var cell = Cells[constIdx, y];

                        if ((cell.Lock & CellLockEnum.Vertically) != CellLockEnum.None)
                        {
                            string mask = maskBuilder.ToString();

                            if (mask.Length >= ValidationService.MinWordNameLength
                                && ValidMaskRegex().IsMatch(mask))
                            {
                                masks.Add(new MaskModel
                                {
                                    Full = mask,
                                    Index = y - mask.Length
                                });
                            }

                            maskBuilder.Clear();
                        }
                        else
                        {
                            if (cell.HWord is not null)
                            {
                                maskBuilder.Append(cell.HWord.Name[cell.HIndex]);
                            }
                            else
                            {
                                maskBuilder.Append('.');
                            }

                            if (y == Height - 1)
                            {
                                string mask = maskBuilder.ToString();

                                if (mask.Length >= ValidationService.MinWordNameLength
                                    && ValidMaskRegex().IsMatch(mask))
                                {
                                    masks.Add(new MaskModel
                                    {
                                        Full = mask,
                                        Index = Height - mask.Length
                                    });
                                }
                            }
                        }
                    }
                    break;
            }

            return masks;
        }

        private int AddWords(List<WordModel> words, SideEnum orientation, int constIdx, ref bool isFirstWord)
        {
            void AddWord(WordModel word, SideEnum orientation, int constIdx, int firstIdx, int lastIdx)
            {
                void AddWord(WordModel word, PointDTO<int> p1, PointDTO<int> p2)
                {
                    CellModel cell;

                    if (p1.X < p2.X
                        && p1.Y == p2.Y)
                    {
                        // Горизонтально
                        // --Слово
                        int x;
                        int y = p1.Y;
                        int i = 0;
                        for (x = p1.X; x <= p2.X; x++)
                        {
                            cell = Cells[x, y];

                            if (x != 0
                                && Cells[x - 1, y].VWord is not null
                                || x != Width - 1
                                && Cells[x + 1, y].VWord is not null)
                            {
                                cell.Lock = CellLockEnum.Horizontally;
                            }
                            else
                            {
                                cell.Lock |= CellLockEnum.Horizontally;
                            }

                            cell.HWord = word;
                            cell.HIndex = i++;
                        }
                        // --Блокировки
                        // ----Слева
                        x = p1.X;
                        if (x != 0)
                        {
                            cell = Cells[x - 1, y];
                            cell.Lock |= CellLockEnum.Horizontally | CellLockEnum.Vertically;
                        }
                        // ----Справа
                        x = p2.X;
                        if (x != Width - 1)
                        {
                            cell = Cells[x + 1, y];
                            cell.Lock |= CellLockEnum.Horizontally | CellLockEnum.Vertically;
                        }
                        // ----Cверху
                        if (y != 0)
                        {
                            // Слева
                            x = p1.X;
                            if (Cells[x, y - 1].VWord is null)
                            {
                                cell = Cells[x, y - 1];
                                cell.Lock |= CellLockEnum.Horizontally;
                            }
                            // В центре
                            for (x = p1.X + 1; x < p2.X; x++)
                            {
                                cell = Cells[x, y - 1];
                                cell.Lock |= CellLockEnum.Horizontally;
                            }
                            // Справа
                            x = p2.X;
                            if (Cells[x, y - 1].VWord is null)
                            {
                                cell = Cells[x, y - 1];
                                cell.Lock |= CellLockEnum.Horizontally;
                            }
                        }
                        // ----Cнизу
                        if (y != Height - 1)
                        {
                            // Слева
                            x = p1.X;
                            if (Cells[x, y + 1].VWord is null)
                            {
                                cell = Cells[x, y + 1];
                                cell.Lock |= CellLockEnum.Horizontally;
                            }
                            // В центре
                            for (x = p1.X + 1; x < p2.X; x++)
                            {
                                cell = Cells[x, y + 1];
                                cell.Lock |= CellLockEnum.Horizontally;
                            }
                            // Справа
                            x = p2.X;
                            if (Cells[x, y + 1].VWord is null)
                            {
                                cell = Cells[x, y + 1];
                                cell.Lock |= CellLockEnum.Horizontally;
                            }
                        }
                    }
                    else if (p1.Y < p2.Y
                        && p1.X == p2.X)
                    {
                        // Вертикально
                        // --Слово
                        int x = p1.X;
                        int y;
                        int i = 0;
                        for (y = p1.Y; y <= p2.Y; y++)
                        {
                            cell = Cells[x, y];

                            if (y != 0
                                && Cells[x, y - 1].HWord is not null
                                || y != Height - 1
                                && Cells[x, y + 1].HWord is not null)
                            {
                                cell.Lock = CellLockEnum.Vertically;
                            }
                            else
                            {
                                cell.Lock |= CellLockEnum.Vertically;
                            }

                            cell.VWord = word;
                            cell.VIndex = i++;
                        }
                        // --Блокировки
                        // ----Cверху
                        y = p1.Y;
                        if (y != 0)
                        {
                            cell = Cells[x, y - 1];
                            cell.Lock |= CellLockEnum.Horizontally | CellLockEnum.Vertically;
                        }
                        // ----Cнизу
                        y = p2.Y;
                        if (y != Height - 1)
                        {
                            cell = Cells[x, y + 1];
                            cell.Lock |= CellLockEnum.Horizontally | CellLockEnum.Vertically;
                        }
                        // ----Слева
                        if (x != 0)
                        {
                            // Cверху
                            y = p1.Y;
                            if (Cells[x - 1, y].HWord is null)
                            {
                                cell = Cells[x - 1, y];
                                cell.Lock |= CellLockEnum.Vertically;
                            }
                            // В центре
                            for (y = p1.Y + 1; y < p2.Y; y++)
                            {
                                cell = Cells[x - 1, y];
                                cell.Lock |= CellLockEnum.Vertically;
                            }
                            // Cнизу
                            y = p2.Y;
                            if (Cells[x - 1, y].HWord is null)
                            {
                                cell = Cells[x - 1, y];
                                cell.Lock |= CellLockEnum.Vertically;
                            }
                        }
                        // ----Справа
                        if (x != Width - 1)
                        {
                            // Cверху
                            y = p1.Y;
                            if (Cells[x + 1, y].HWord is null)
                            {
                                cell = Cells[x + 1, y];
                                cell.Lock |= CellLockEnum.Vertically;
                            }
                            // В центре
                            for (y = p1.Y + 1; y < p2.Y; y++)
                            {
                                cell = Cells[x + 1, y];
                                cell.Lock |= CellLockEnum.Vertically;
                            }
                            // Cнизу
                            y = p2.Y;
                            if (Cells[x + 1, y].HWord is null)
                            {
                                cell = Cells[x + 1, y];
                                cell.Lock |= CellLockEnum.Vertically;
                            }
                        }
                    }
                    else
                    {
                        throw new ArgumentException();
                    }
                }


                var p1 = new PointDTO<int>();
                var p2 = new PointDTO<int>();
                switch (orientation)
                {
                    case SideEnum.Up:
                    case SideEnum.Down:
                        p1.X = firstIdx;
                        p2.X = lastIdx;
                        p1.Y = constIdx;
                        p2.Y = constIdx;
                        break;
                    case SideEnum.Left:
                    case SideEnum.Right:
                        p1.X = constIdx;
                        p2.X = constIdx;
                        p1.Y = firstIdx;
                        p2.Y = lastIdx;
                        break;
                }

                AddWord(word, p1, p2);
            }

            WordModel? FindWord(MaskModel mask)
            {
                WordModel? word;

                int type = 0;
                string fullMask = mask.Full;

                while (true)
                {
                    word = words
                        .Where(w => Regex.IsMatch(w.Name, mask.Pattern))
                        .FirstOrDefault();

                    if (word is not null)
                        return word;

                    switch (type)
                    {
                        case 0:
                            // Обрезаем справа
                            if (mask.Left is not null
                                && (mask.Left.Length + mask.Body.Length) >= 5)
                            {
                                mask.Full = mask.Full[..(mask.Left.Length + mask.Body.Length - 2)];
                            }
                            else if (mask.Body.Length >= 5)
                            {
                                mask.Full = mask.Full[..(mask.Body.Length - 2)];
                            }
                            else
                            {
                                type++;
                                mask.Full = fullMask;
                                goto case 1;
                            }
                            break;
                        case 1:
                            // Обрезаем слева
                            if (mask.Right is not null
                                && (mask.Right.Length + mask.Body.Length) >= 5)
                            {
                                int startIndex = mask.Full.Length - mask.Right.Length - mask.Body.Length + 2;

                                mask.Full = mask.Full[startIndex..];
                                mask.Index += startIndex;
                            }
                            else if (mask.Body.Length >= 5)
                            {
                                int startIndex = mask.Full.Length - mask.Body.Length + 2;

                                mask.Full = mask.Full[startIndex..];
                                mask.Index += startIndex;
                            }
                            else
                            {
                                type++;
                                mask.Full = fullMask;
                                goto case 2;
                            }
                            break;
                        case 2:
                            // Обрезаем слева и справа
                            if (mask.Body.Length >= 7
                                && ValidMaskBodyRegex().IsMatch(mask.Body))
                            {
                                if (mask.Left is not null)
                                {
                                    int startIndex = mask.Left.Length + 2;

                                    mask.Full = mask.Full[startIndex..(mask.Left.Length + mask.Body.Length - 2)];
                                    mask.Index += startIndex;
                                }
                                else
                                {
                                    int startIndex = 2;

                                    mask.Full = mask.Full[startIndex..(mask.Body.Length - 2)];
                                    mask.Index += startIndex;
                                }
                            }
                            else
                            {
                                return word;
                            }
                            break;
                    }
                }
            }


            int nAddedWords = 0;
            WordModel? word = null;

            if (isFirstWord)
            {
                int maxWordLength = orientation == SideEnum.Up | orientation == SideEnum.Down
                        ? Width
                        : Height;

                int randomCount;
                {
                    Random random = new();
                    int maxValue = words.Count > 100
                        ? words.Count / 100 + 10
                        : 3;
                    randomCount = random.Next(0, maxValue);
                }

                word = words
                    .Where(w => w.Name.Length <= maxWordLength)
                    .Skip(randomCount)
                    .First();

                int firstIdx = (maxWordLength - word.Name.Length) / 2;
                int lastIdx = firstIdx + word.Name.Length - 1;

                AddWord(word, orientation, constIdx, firstIdx, lastIdx);
                words.Remove(word);
                nAddedWords++;
                isFirstWord = false;
            }
            else
            {
                var masks = CreateMasks(orientation, constIdx);

                foreach (var mask in masks)
                {
                    word = FindWord(mask);

                    if (word is null)
                        continue;

                    int maskBodyIdx = Regex.Match(mask.Full, mask.Pattern).Groups[1].Index;
                    int wordBodyIdx = Regex.Match(word.Name, mask.Pattern).Groups[1].Index;

                    int firstIdx = mask.Index + maskBodyIdx - wordBodyIdx;
                    int lastIdx = firstIdx + word.Name.Length - 1;

                    AddWord(word, orientation, constIdx, firstIdx, lastIdx);
                    words.Remove(word);
                    nAddedWords++;
                }
            }
            return nAddedWords;
        }


        public void Generate(List<WordModel> words)
        {
            int minWordCount = 10;
            if (words.Count < minWordCount)
                throw new ArgumentException($"Не найдено минимальное количество слов. Минимальное количество слов: {minWordCount}");

            int minWordLength = words.Last().Name.Length;
            if (Width < Height
                ? Width < minWordLength
                : Height < minWordLength)
            {
                string orientation = Width < Height
                    ? "горизонтали"
                    : "вертикали";
                throw new ArgumentException($"Не найдено ни одного достаточно короткого слова для размещения по {orientation}");
            }


            int n = Width > Height
                ? Width / 2 + Width % 2
                : Height / 2 + Height % 2;

            bool isFirstWord = true;

            int nAddedWords;
            do
            {
                nAddedWords = 0;

                int upIdx = Height / 2 - 1;
                int downIdx = upIdx + 1;

                int leftIdx = Width / 2 - 1;
                int rightIdx = leftIdx + 1;

                for (int i = 0; i < n; i++)
                {
                    if (upIdx >= 0)
                    {
                        nAddedWords += AddWords(words, SideEnum.Up, upIdx, ref isFirstWord);
                        upIdx--;
                    }
                    if (leftIdx >= 0)
                    {
                        nAddedWords += AddWords(words, SideEnum.Left, leftIdx, ref isFirstWord);
                        leftIdx--;
                    }
                    if (downIdx < Height)
                    {
                        nAddedWords += AddWords(words, SideEnum.Down, downIdx, ref isFirstWord);
                        downIdx++;
                    }
                    if (rightIdx < Width)
                    {
                        nAddedWords += AddWords(words, SideEnum.Right, rightIdx, ref isFirstWord);
                        rightIdx++;
                    }
                }
            } while (nAddedWords != 0);
        }

        public List<CrosswordWordDTO> ToCrosswordWordDTOs()
        {
            var crosswordWordDTOs = new Dictionary<int, CrosswordWordDTO>();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var cell = Cells[x, y];

                    var word = cell.HWord;
                    if (word is not null
                        && !crosswordWordDTOs.ContainsKey(word.Id))
                    {
                        crosswordWordDTOs.Add(
                            word.Id,
                            new CrosswordWordDTO
                            {
                                Id = word.Id,
                                Name = word.Name,
                                Definition = word.Definition,
                                P1 = new PointDTO<short>
                                {
                                    X = (short)(x - cell.HIndex),
                                    Y = (short)y
                                },
                                P2 = new PointDTO<short>
                                {
                                    X = (short)(x + (word.Name.Length - cell.HIndex - 1)),
                                    Y = (short)y
                                }
                            });
                    }

                    word = cell.VWord;
                    if (word is not null
                        && !crosswordWordDTOs.ContainsKey(word.Id))
                    {
                        crosswordWordDTOs.Add(
                            word.Id,
                            new CrosswordWordDTO
                            {
                                Id = word.Id,
                                Name = word.Name,
                                Definition = word.Definition,
                                P1 = new PointDTO<short>
                                {
                                    X = (short)x,
                                    Y = (short)(y - cell.VIndex)
                                },
                                P2 = new PointDTO<short>
                                {
                                    X = (short)x,
                                    Y = (short)(y + (word.Name.Length - cell.VIndex - 1))
                                }
                            });
                    }
                }
            }

            return crosswordWordDTOs
                .OrderBy(cwm => cwm.Value.Name)
                .Select(cwm => cwm.Value)
                .ToList();
        }

    }
}
