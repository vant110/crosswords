import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminCrosswordsComponent } from './admin-crosswords.component';
import { RouterModule, Routes } from '@angular/router';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzTableModule } from 'ng-zorro-antd/table';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { CrosswordWidthPipe } from './crossword-width.pipe';
import { IsSelectedPipe } from './is-selected.pipe';

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
    NzTableModule,
    NzButtonModule,
  ],
  declarations: [AdminCrosswordsComponent, CrosswordWidthPipe, IsSelectedPipe],
})
export class AdminCrosswordsModule {}
