import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReferenceComponent } from './reference/reference.component';
import { DictionaryAddComponent } from './dictionary-add/dictionary-add.component';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzInputModule } from 'ng-zorro-antd/input';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { WordAddComponent } from './word-add/word-add.component';
import { ThemeAddComponent } from './theme-add/theme-add.component';

@NgModule({
  declarations: [
    ReferenceComponent,
    DictionaryAddComponent,
    WordAddComponent,
    ThemeAddComponent,
  ],
  imports: [
    CommonModule,
    NzIconModule,
    NzInputModule,
    FormsModule,
    ReactiveFormsModule,
  ],
  exports: [
    ReferenceComponent,
    DictionaryAddComponent,
    WordAddComponent,
    ThemeAddComponent,
  ],
})
export class ModalsModule {}
