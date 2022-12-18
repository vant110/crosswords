import { Pipe, PipeTransform } from '@angular/core';
import { SelectedPoint } from 'src/app/core/models/selected-point';

@Pipe({
  name: 'isSelected',
})
export class IsSelectedPipe implements PipeTransform {
  transform(
    [y, x]: [number, number],
    start: SelectedPoint,
    end: SelectedPoint,
  ): boolean {
    const isSelected = y === start.y && x === start.x;
    let isInRange = false;
    if (
      start.x !== null &&
      start.y !== null &&
      end.x !== null &&
      end.y !== null
    ) {
      if (start.x === end.x && start.x === x) {
        isInRange =
          (y >= start.y && y <= end.y) || (y <= start.y && y >= end.y);
      }
      if (start.y === end.y && start.y === y) {
        isInRange =
          (x >= start.x && x <= end.x) || (x <= start.x && x >= end.x);
      }
    }
    return isSelected || isInRange;
  }
}
