import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminCrosswordsComponent } from './admin-crosswords.component';
import { RouterModule, Routes } from '@angular/router';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NzInputModule } from 'ng-zorro-antd/input';

const routes: Routes = [{ path: '', component: AdminCrosswordsComponent }];

@NgModule({
  imports: [
    CommonModule,
    RouterModule.forChild(routes),
    NzSelectModule,
    NzIconModule,
    ReactiveFormsModule,
    FormsModule,
    NzInputModule,
  ],
  declarations: [AdminCrosswordsComponent],
})
export class AdminCrosswordsModule {}
