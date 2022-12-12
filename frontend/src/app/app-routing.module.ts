import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AdminGuard } from './core/guards/admin.guard';
import { AuthGuard } from './core/guards/auth.guard';
import { PlayerGuard } from './core/guards/player.guard';

const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: '/user-crosswords' },
  {
    path: 'admin-dictionaries',
    loadChildren: () =>
      import('./pages/admin-dictionaries/admin-dictionaries.module').then(
        (m) => m.AdminDictionariesModule,
      ),
    canActivate: [AuthGuard, AdminGuard],
  },
  {
    path: 'admin-crosswords',
    loadChildren: () =>
      import('./pages/admin-crosswords/admin-crosswords.module').then(
        (m) => m.AdminCrosswordsModule,
      ),
    canActivate: [AuthGuard, AdminGuard],
  },
  {
    path: 'user-crosswords',
    loadChildren: () =>
      import('./pages/user-crosswords/user-crosswords.module').then(
        (m) => m.UserCrosswordsModule,
      ),
    canActivate: [AuthGuard, PlayerGuard],
  },
  {
    path: 'login',
    loadChildren: () =>
      import('./pages/login/login.module').then((m) => m.LoginModule),
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
