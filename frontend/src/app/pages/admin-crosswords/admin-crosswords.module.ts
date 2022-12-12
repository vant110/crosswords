import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminCrosswordsComponent } from './admin-crosswords.component';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [{ path: '', component: AdminCrosswordsComponent }];

@NgModule({
  imports: [CommonModule, RouterModule.forChild(routes)],
  declarations: [AdminCrosswordsComponent],
})
export class AdminCrosswordsModule {}
