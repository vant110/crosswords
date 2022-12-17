export enum WordSort {
  ASC_APLHABET = 'ascAlphabet',
  DESC_APLHABET = 'descAlphabet',
  DESC_LENGTH = 'descLength',
  ASC_LENGTH = 'ascLength',
}

export const sortingOptions = [
  { id: WordSort.ASC_APLHABET, name: 'По алфавиту (А-Я)' },
  { id: WordSort.DESC_APLHABET, name: 'По алфавиту (Я-А)' },
  { id: WordSort.ASC_LENGTH, name: 'По длине (возр.)' },
  { id: WordSort.DESC_LENGTH, name: 'По длине (убыв.)' },
];
