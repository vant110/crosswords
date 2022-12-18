export interface CrosswordList {
  id: number;
  name: string;
}

export interface CrosswordWord {
  id: number;
  name: string;
  definition: string;
  p1: {
    x: number;
    y: number;
  };
  p2: {
    x: number;
    y: number;
  };
  isSolved?: boolean;
}

export interface CrosswordGrid {
  x: number;
  y: number;
  l: string;
}

export interface Crossword {
  name: string;
  themeId: number;
  dictionaryId: number;
  size: {
    width: number;
    height: number;
  };
  promptCount: number;
  words: Array<CrosswordWord>;
}

export interface UserCrossword {
  size: {
    width: number;
    height: number;
  };
  promptCount: number;
  words: Array<CrosswordWord>;
  grid: Array<CrosswordGrid>;
}

export interface ChangeLetterResponse {
  solvedWords: Array<{ id: number }>;
}

export interface CrosswordPrompt extends Partial<ChangeLetterResponse> {
  letter: string;
}
