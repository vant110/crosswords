import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminDictionariesComponent } from './admin-dictionaries.component';
import { RouterModule, Routes } from '@angular/router';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzTableModule } from 'ng-zorro-antd/table';

const routes: Routes = [{ path: '', component: AdminDictionariesComponent }];

@NgModule({
  imports: [
    CommonModule,
    RouterModule.forChild(routes),
    NzSelectModule,
    NzIconModule,
    ReactiveFormsModule,
    FormsModule,
    NzInputModule,
    NzTableModule,
  ],
  declarations: [AdminDictionariesComponent],
})
export class AdminDictionariesModule {}
