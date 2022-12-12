import { Component } from '@angular/core';
import { NzModalService } from 'ng-zorro-antd/modal';
import { AuthService } from './core/services/auth.service';
import { ReferenceComponent } from './shared/reference/reference.component';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent {
  authenticated$ = this.authService.authenticated$;

  get isAdmin() {
    return this.authService.currentUser.isAdmin;
  }

  get userName() {
    return this.isAdmin ? 'Admin' : this.authService.currentUser.login;
  }

  constructor(
    private authService: AuthService,
    private modal: NzModalService,
  ) {}

  onLogout() {
    this.authService.logout();
  }

  onOpenHelp() {
    this.modal.create({
      nzTitle: 'Справка',
      nzContent: ReferenceComponent,
      nzFooter: null,
    });
  }
}
