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
