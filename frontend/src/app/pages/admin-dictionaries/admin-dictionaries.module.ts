import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminDictionariesComponent } from './admin-dictionaries.component';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [{ path: '', component: AdminDictionariesComponent }];

@NgModule({
  imports: [CommonModule, RouterModule.forChild(routes)],
  declarations: [AdminDictionariesComponent],
})
export class AdminDictionariesModule {}
