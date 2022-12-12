import { Component } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { NzNotificationService } from 'ng-zorro-antd/notification';
import { AuthService } from 'src/app/core/services/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent {
  loginForm = this.formBuilder.group({
    login: ['', [Validators.required]],
    password: ['', [Validators.required]],
  });

  get isInvalid() {
    return this.loginForm.invalid;
  }

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private notify: NzNotificationService,
    private router: Router,
  ) {}

  onSignin() {
    const login = this.loginForm.value.login as string;
    const password = this.loginForm.value.password as string;

    this.authService.signin(login, password).subscribe(
      (result) => {
        this.notify.success(
          'Успех',
          `${
            this.authService.currentUser.isAdmin
              ? 'Администратор'
              : 'Пользователь'
          } ${login} авторизован`,
        );

        const route = result.isAdmin ? '/admin-crosswords' : '/user-crosswords';
        this.router.navigate([route]);
      },
      (error) => this.notify.error('Ошибка', error.error.message),
    );
  }

  onRegister() {
    const login = this.loginForm.value.login as string;
    const password = this.loginForm.value.password as string;

    this.authService.register(login, password).subscribe(
      () => {
        this.notify.success('Успех', `Пользователь ${login} зарегистирован`);

        this.router.navigate(['/user-crosswords']);
      },
      (error) => this.notify.error('Ошибка', error.error.message),
    );
  }
}
