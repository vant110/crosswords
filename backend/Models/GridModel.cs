using Crosswords.Db.Models;
using Crosswords.Models.DTOs;
using Crosswords.Models.Enums;
using System.Text;
using System.Text.RegularExpressions;

namespace Crosswords.Models
{
    public class GridModel
    {
        public int Width { get; }
        public int Height { get; }
        public CellModel[,] Cells { get; }


        public GridModel(int width, int height)
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


        private static string CreatePattern(string mask)
        {
            var leftMatch = Regex.Match(mask, @"^(\.+)");

            var patternBuilder = new StringBuilder("^");
            if (leftMatch.Success)
            {
                patternBuilder.Append($".{{0,{leftMatch.Length}}}");
            }
            if (leftMatch.Length != mask.Length)
            {
                var rightMatch = Regex.Match(mask, @"(\.+)$");

                patternBuilder.Append($"({mask[leftMatch.Length..(mask.Length - rightMatch.Length)]})");
                if (rightMatch.Success)
                {
                    patternBuilder.Append($".{{0,{rightMatch.Length}}}");
                }
            }
            patternBuilder.Append('$');

            return patternBuilder.ToString();
        }


        private string CreateMask(SideEnum orientation, int constIdx)
        {
            var maskBuilder = new StringBuilder();

            switch (orientation)
            {
                case SideEnum.Up:
                case SideEnum.Down:
                    for (int x = 0; x < Width; x++)
                    {
                        var cell = Cells[x, constIdx];

                        if (cell.VWord is not null)
                        {
                            maskBuilder.Append(cell.VWord.WordName[cell.VLetterIndex]);
                        }
                        else if ((cell.Lock & CellLockEnum.Horizontally) != CellLockEnum.None)
                        {
                            if (Regex.IsMatch(maskBuilder.ToString(), @"[А-ЯЁ]"))
                                break;

                            maskBuilder.Clear();
                        }
                        else
                        {
                            maskBuilder.Append('.');
                        }
                    }
                    break;
                case SideEnum.Left:
                case SideEnum.Right:
                    for (int y = 0; y < Height; y++)
                    {
                        var cell = Cells[constIdx, y];

                        if (cell.HWord is not null)
                        {
                            maskBuilder.Append(cell.HWord.WordName[cell.HLetterIndex]);
                        }
                        else if ((cell.Lock & CellLockEnum.Vertically) != CellLockEnum.None)
                        {
                            if (Regex.IsMatch(maskBuilder.ToString(), @"[А-ЯЁ]"))
                                break;

                            maskBuilder.Clear();
                        }
                        else
                        {
                            maskBuilder.Append('.');
                        }
                    }
                    break;
            }

            return maskBuilder.ToString();
        }

        private bool TryAddWord(List<Word> words, SideEnum orientation, int constIdx, bool isFirstWord = false)
        {
            void AddWord(Word word, SideEnum orientation, int constIdx, int firstIdx, int lastIdx)
            {
                void AddWord(Word word, PointDTO<int> p1, PointDTO<int> p2)
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
                            cell.Lock |= CellLockEnum.Horizontally;
                            cell.HWord = word;
                            cell.HLetterIndex = i++;
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
                            for (x = p1.X; x <= p2.X; x++)
                            {
                                cell = Cells[x, y - 1];
                                cell.Lock |= CellLockEnum.Horizontally;
                            }
                        }
                        // ----Cнизу
                        if (y != Height - 1)
                        {
                            for (x = p1.X; x <= p2.X; x++)
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
                            cell.Lock |= CellLockEnum.Vertically;
                            cell.VWord = word;
                            cell.VLetterIndex = i++;
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
                            for (y = p1.Y; y <= p2.Y; y++)
                            {
                                cell = Cells[x - 1, y];
                                cell.Lock |= CellLockEnum.Vertically;
                            }
                        }
                        // ----Справа
                        if (x != Width - 1)
                        {
                            for (y = p1.Y; y <= p2.Y; y++)
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


            // Формируем маску
            string mask = CreateMask(orientation, constIdx);
            // Формируем шаблон
            string pattern = CreatePattern(mask);
            // Находим слово
            Word? word = null;
            if (Regex.IsMatch(mask, @"[А-ЯЁ]"))
            {
                word = words
                    .Where(w => Regex.IsMatch(w.WordName, pattern))
                    .FirstOrDefault();
            }
            else if (isFirstWord)
            {
                int maxValue = words.Count > 100
                    ? words.Count / 100 + 10
                    : 3;

                Random random = new();
                int randomCount = random.Next(0, maxValue);

                word = words
                    .Where(w => w.WordName.Length <= mask.Length)
                    .Skip(randomCount)
                    .FirstOrDefault();
            }
            // Добавляем слово
            if (word is null)
                return false;

            int maskBodyIdx = Regex.Match(mask, pattern).Groups[1].Index;
            int wordBodyIdx = Regex.Match(word.WordName, pattern).Groups[1].Index;

            int firstIdx = maskBodyIdx - wordBodyIdx;
            int lastIdx = firstIdx + word.WordName.Length - 1;

            AddWord(word, orientation, constIdx, firstIdx, lastIdx);
            words.Remove(word);

            return true;
        }


        public void GenerateCrossword(List<Word> words)
        {
            int minWordCount = 10;
            if (words.Count < minWordCount)
                throw new ArgumentException($"Не найдено минимальное количество слов. Минимальное количество слов: {minWordCount}");

            int upIdx = Height / 2 - 1;
            int downIdx = upIdx + 2;

            int leftIdx = Width / 2 - 1;
            int rightIdx = leftIdx + 2;

            int n = Width > Height
                ? Width
                : Height;
            bool isFirstWord = true;
            for (int i = 0; i < n - 2; i++)
            {
                if (upIdx >= 0)
                {
                    upIdx -= TryAddWord(words, SideEnum.Up, upIdx, isFirstWord)
                        ? 2
                        : 1;
                    isFirstWord = false;
                }
                if (leftIdx >= 0)
                {
                    leftIdx -= TryAddWord(words, SideEnum.Left, leftIdx)
                        ? 2
                        : 1;
                }
                if (downIdx < Height)
                {
                    downIdx += TryAddWord(words, SideEnum.Down, downIdx)
                        ? 2
                        : 1;
                }
                if (rightIdx < Width)
                {
                    rightIdx += TryAddWord(words, SideEnum.Right, rightIdx)
                        ? 2
                        : 1;
                }
            }
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
                        && !crosswordWordDTOs.ContainsKey(word.WordId))
                    {
                        crosswordWordDTOs.Add(
                            word.WordId,
                            new CrosswordWordDTO
                            {
                                Id = word.WordId,
                                Name = word.WordName,
                                Definition = word.Definition,
                                P1 = new PointDTO<short>
                                {
                                    X = (short)(x - cell.HLetterIndex),
                                    Y = (short)y
                                },
                                P2 = new PointDTO<short>
                                {
                                    X = (short)(x + (word.WordName.Length - cell.HLetterIndex - 1)),
                                    Y = (short)y
                                }
                            });
                    }

                    word = cell.VWord;
                    if (word is not null
                        && !crosswordWordDTOs.ContainsKey(word.WordId))
                    {
                        crosswordWordDTOs.Add(
                            word.WordId,
                            new CrosswordWordDTO
                            {
                                Id = word.WordId,
                                Name = word.WordName,
                                Definition = word.Definition,
                                P1 = new PointDTO<short>
                                {
                                    X = (short)x,
                                    Y = (short)(y - cell.VLetterIndex)
                                },
                                P2 = new PointDTO<short>
                                {
                                    X = (short)x,
                                    Y = (short)(y + (word.WordName.Length - cell.VLetterIndex - 1))
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
