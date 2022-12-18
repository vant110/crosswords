import { CrosswordWord } from '../models/crossword';

export function getWordsForPoint(
  words: CrosswordWord[],
  x: number,
  y: number,
): CrosswordWord[] {
  return words.filter((word) => {
    if (word.p1.x === word.p2.x && word.p1.x === x) {
      return y >= word.p1.y && y <= word.p2.y;
    } else if (word.p1.y === word.p2.y && word.p1.y === y) {
      return x >= word.p1.x && x <= word.p2.x;
    } else return false;
  });
}
