import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserCrosswordsComponent } from './user-crosswords.component';
import { RouterModule, Routes } from '@angular/router';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzTableModule } from 'ng-zorro-antd/table';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { PipesModule } from 'src/app/core/pipes/pipes.module';

const routes: Routes = [{ path: '', component: UserCrosswordsComponent }];

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
    PipesModule,
  ],
  declarations: [UserCrosswordsComponent],
})
export class UserCrosswordsModule {}
