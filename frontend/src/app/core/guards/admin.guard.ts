import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root',
})
export class AdminGuard implements CanActivate {
  constructor(private router: Router, private authService: AuthService) {}

  canActivate(): boolean {
    if (!this.authService.currentUser.isAdmin) {
      this.router.navigate(['/user-crosswords'], { replaceUrl: true });
      return false;
    }

    return true;
  }
}
