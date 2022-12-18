import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CrosswordWidthPipe } from './crossword-width.pipe';
import { IsSelectedPipe } from './is-selected.pipe';

@NgModule({
  declarations: [CrosswordWidthPipe, IsSelectedPipe],
  imports: [CommonModule],
  exports: [CrosswordWidthPipe, IsSelectedPipe],
})
export class PipesModule {}
