export interface DictionaryWord {
  id: number;
  name: string;
  definition: string;
}

export interface AvailableWord {
  id: number;
  name: string;
  definition: string;
  offset?: number;
}
