import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReferenceComponent } from './reference/reference.component';
import { DictionaryAddComponent } from './dictionary-add/dictionary-add.component';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzInputModule } from 'ng-zorro-antd/input';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { WordAddComponent } from './word-add/word-add.component';
import { ThemeAddComponent } from './theme-add/theme-add.component';
import { CrosswordAddComponent } from './crossword-add/crossword-add.component';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzInputNumberModule } from 'ng-zorro-antd/input-number';

@NgModule({
  declarations: [
    ReferenceComponent,
    DictionaryAddComponent,
    WordAddComponent,
    ThemeAddComponent,
    CrosswordAddComponent,
  ],
  imports: [
    CommonModule,
    NzIconModule,
    NzInputModule,
    FormsModule,
    ReactiveFormsModule,
    NzSelectModule,
    NzInputNumberModule,
  ],
  exports: [
    ReferenceComponent,
    DictionaryAddComponent,
    WordAddComponent,
    ThemeAddComponent,
    CrosswordAddComponent,
  ],
})
export class ModalsModule {}
