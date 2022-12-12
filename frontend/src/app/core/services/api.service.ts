import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AuthResponse } from '../models/auth';

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
}
