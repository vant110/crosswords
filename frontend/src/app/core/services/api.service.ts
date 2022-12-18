import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AuthResponse } from '../models/auth';
import {
  ChangeLetterResponse,
  Crossword,
  CrosswordList,
  CrosswordPrompt,
  CrosswordWord,
  UserCrossword,
} from '../models/crossword';
import { Dictionary } from '../models/dictionary';
import { CrosswordTheme } from '../models/theme';
import { AvailableWord, DictionaryWord } from '../models/word';

@Injectable({ providedIn: 'root' })
export class ApiService {
  constructor(private http: HttpClient) {}

  signin(login: string, password: string) {
    return this.http.post<AuthResponse>('/api/auth/signin', {
      login,
      password,
    });
  }

  singup(login: string, password: string) {
    return this.http.post<AuthResponse>('/api/auth/signup', {
      login,
      password,
    });
  }

  getDictionaries() {
    return this.http.get<Dictionary[]>('/api/dictionaries');
  }

  createDictionary(formData: FormData) {
    return this.http.post('/api/dictionaries', formData);
  }

  editDictionary(id: number, name: string) {
    return this.http.patch(`/api/dictionaries/${id}`, { name });
  }

  deleteDictionary(id: number) {
    return this.http.delete(`/api/dictionaries/${id}`);
  }

  getWords(
    dictionaryId: number,
    sort: string = '',
    search: string = '',
    lastName: string | null = null,
    limit: number | null = 25,
  ) {
    const params = {} as any;
    if (sort) params.sort = sort;
    if (search) params.search = search;
    if (lastName) params.lastName = lastName;
    if (limit) params.limit = limit;

    return this.http.get<DictionaryWord[]>(
      `/api/dictionaries/${dictionaryId}/words`,
      { params },
    );
  }

  createWord(dictionaryId: number, name: string, definition: string) {
    return this.http.post(`/api/dictionaries/${dictionaryId}/words`, {
      name,
      definition,
    });
  }

  editWord(id: number, definition: string) {
    return this.http.patch(`/api/words/${id}`, { definition });
  }

  deleteWord(id: number) {
    return this.http.delete(`/api/words/${id}`);
  }

  getThemes() {
    return this.http.get<CrosswordTheme[]>('/api/themes');
  }

  createTheme(name: string) {
    return this.http.post('/api/themes', { name });
  }

  putTheme(id: number, name: string) {
    return this.http.put(`/api/themes/${id}`, { name });
  }

  deleteTheme(id: number) {
    return this.http.delete(`/api/themes/${id}`);
  }

  getCrosswordsList(themeId: number) {
    return this.http.get<CrosswordList[]>(`/api/themes/${themeId}/crosswords`);
  }

  getCrossword(id: number) {
    return this.http.get<Crossword>(`/api/crosswords/${id}`);
  }

  createCrossword(crossword: Crossword) {
    return this.http.post<{ id: number }>('/api/crosswords', crossword);
  }

  putCrossword(crossword: Crossword, id: number) {
    return this.http.put(`/api/crosswords/${id}`, crossword);
  }

  deleteCrossword(id: number) {
    return this.http.delete(`/api/crosswords/${id}`);
  }

  generateCrossword(width: number, height: number, dictionaryId: number) {
    return this.http.get<CrosswordWord[]>(
      `/api/dictionaries/${dictionaryId}/generate_crossword`,
      { params: { width, height } },
    );
  }

  getAvailableWords(
    dictionaryId: number,
    sort: string = '',
    search: string = '',
    mask: string | null = null,
    lastName: string | null = null,
    limit: number | null = 25,
  ) {
    const params = {} as any;
    if (sort) params.sort = sort;
    if (search) params.search = search;
    if (lastName) params.lastName = lastName;
    if (limit) params.limit = limit;
    if (mask) params.mask = mask;

    return this.http.get<AvailableWord[]>(
      `/api/dictionaries/${dictionaryId}/words`,
      { params },
    );
  }

  getUnstartedCrosswordList(themeId: number) {
    return this.http.get<CrosswordList[]>(
      `/api/themes/${themeId}/crosswords/unstarted`,
    );
  }

  getStartedCrosswordList(themeId: number) {
    return this.http.get<CrosswordList[]>(
      `/api/themes/${themeId}/crosswords/started`,
    );
  }

  getUnstartedCrossword(id: number) {
    return this.http.get<UserCrossword>(`/api/crosswords/${id}/unstarted`);
  }

  getStartedCrossword(id: number) {
    return this.http.get<UserCrossword>(`/api/crosswords/${id}/started`);
  }

  changeLetter(crosswordId: number, x: number, y: number, letter: string) {
    return this.http.get<ChangeLetterResponse>(
      `/api/crosswords/${crosswordId}/change_letter`,
      { params: { x, y, letter } },
    );
  }

  takePrompt(crosswordId: number, x: number, y: number) {
    return this.http.get<CrosswordPrompt>(
      `/api/crosswords/${crosswordId}/take_prompt`,
      { params: { x, y } },
    );
  }
}
