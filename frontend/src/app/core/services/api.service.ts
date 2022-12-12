import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AuthResponse } from '../models/auth';
import { Dictionary } from '../models/dictionary';

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
}
