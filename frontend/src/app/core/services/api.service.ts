import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AuthResponse } from '../models/auth';
import { Dictionary } from '../models/dictionary';
import { DictionaryWord } from '../models/word';

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
}
