import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'crosswordWidth',
})
export class CrosswordWidthPipe implements PipeTransform {
  transform(value: string[][]): number {
    return value[0]?.length * 31;
  }
}
