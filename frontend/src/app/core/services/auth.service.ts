import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, tap } from 'rxjs';
import { ApiService } from './api.service';

export interface UserInfo {
  token: string;
  login: string;
  isAdmin: boolean;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  authenticated$ = new BehaviorSubject(false);
  currentUser!: UserInfo;

  private _credentialsKey = 'crosswords-user';

  get isAuthenticated() {
    return this.authenticated$.value;
  }

  constructor(private api: ApiService, private router: Router) {
    this.restoreCredentials();
  }

  signin(login: string, password: string) {
    return this.api.signin(login, password).pipe(
      tap((result) => {
        this.currentUser = {
          isAdmin: result.isAdmin || false,
          login: login,
          token: result.bearerToken,
        };

        if (result.bearerToken) {
          this.storeCredentials(this.currentUser);
          this.authenticated$.next(true);
        }
      }),
    );
  }

  register(login: string, password: string) {
    return this.api.singup(login, password).pipe(
      tap((result) => {
        this.currentUser = {
          isAdmin: false,
          login: login,
          token: result.bearerToken,
        };

        if (result.bearerToken) {
          this.storeCredentials(this.currentUser);
          this.authenticated$.next(true);
        }
      }),
    );
  }

  logout() {
    this.currentUser = {} as UserInfo;
    localStorage.setItem(this._credentialsKey, '');
    this.authenticated$.next(false);
    this.router.navigate(['/login']);
  }

  storeCredentials(userInfo: UserInfo) {
    localStorage.setItem(this._credentialsKey, JSON.stringify(userInfo));
  }

  restoreCredentials() {
    this.currentUser = JSON.parse(
      localStorage.getItem(this._credentialsKey) || '{}',
    );
    if (this.currentUser?.token) this.authenticated$.next(true);
  }
}
