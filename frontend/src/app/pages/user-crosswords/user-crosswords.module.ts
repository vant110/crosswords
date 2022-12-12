import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserCrosswordsComponent } from './user-crosswords.component';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [{ path: '', component: UserCrosswordsComponent }];

@NgModule({
  imports: [CommonModule, RouterModule.forChild(routes)],
  declarations: [UserCrosswordsComponent],
})
export class UserCrosswordsModule {}
