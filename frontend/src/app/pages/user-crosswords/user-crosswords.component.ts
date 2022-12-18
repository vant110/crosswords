import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { NzModalService } from 'ng-zorro-antd/modal';
import { NzNotificationService } from 'ng-zorro-antd/notification';
import { BehaviorSubject, filter, of, switchMap } from 'rxjs';
import {
  CrosswordGrid,
  CrosswordList,
  CrosswordWord,
  UserCrossword,
} from 'src/app/core/models/crossword';
import { CrosswordTheme } from 'src/app/core/models/theme';
import { ApiService } from 'src/app/core/services/api.service';
import { getWordsForPoint } from 'src/app/core/utils/utils';

enum CrosswordType {
  UNSTARTED = 'unstarted',
  STARTED = 'started',
}

@UntilDestroy()
@Component({
  selector: 'app-user-crosswords',
  templateUrl: './user-crosswords.component.html',
  styleUrls: ['./user-crosswords.component.scss'],
})
export class UserCrosswordsComponent implements OnInit {
  themes$ = new BehaviorSubject<CrosswordTheme[]>([]);
  filterForm = this.formBuilder.group({
    theme: [0],
    crosswordType: [CrosswordType.STARTED],
  });

  crosswordTypeOptions = [
    { id: CrosswordType.STARTED, name: 'Начатые' },
    { id: CrosswordType.UNSTARTED, name: 'Неначатые' },
  ];

  crosswords$ = new BehaviorSubject<CrosswordList[]>([]);
  crosswordsForm = this.formBuilder.group({
    crossword: [0],
  });

  selectedCrossword$ = new BehaviorSubject<UserCrossword | null>(null);
  crosswordMatrix$ = new BehaviorSubject<string[][]>([[]]);
  solvedMatrix: boolean[][] = [];

  selectedCell: { x: number; y: number } | null = null;
  selectedWordsIds: number[] = [];

  get selectedThemeId() {
    return this.filterForm.get('theme')?.value as number;
  }

  get selectedCrosswordId() {
    return this.crosswordsForm.get('crossword')?.value as number;
  }

  get selectedCrosswordType() {
    return this.filterForm.get('crosswordType')?.value as CrosswordType;
  }

  get filtersPresent() {
    return (
      !!this.filterForm.get('theme')?.value &&
      !!this.filterForm.get('crosswordType')?.value
    );
  }

  get isCellSelected() {
    return (
      Number.isInteger(this.selectedCell?.x) &&
      Number.isInteger(this.selectedCell?.y)
    );
  }

  get isSelectedCellSolved() {
    if (!this.selectedCell) return false;

    const x = this.selectedCell?.x;
    const y = this.selectedCell?.y;
    return this.solvedMatrix[y][x];
  }

  constructor(
    private api: ApiService,
    private modal: NzModalService,
    private formBuilder: FormBuilder,
    private notify: NzNotificationService,
  ) {
    this.filterForm.valueChanges
      .pipe(untilDestroyed(this))
      .subscribe((filters) => {
        if (!filters.crosswordType || !filters.theme) return;

        this.updateCrosswordsList(filters.theme, filters.crosswordType);
        this.deselectCrossword();
      });

    this.crosswordsForm
      .get('crossword')
      ?.valueChanges.pipe(
        untilDestroyed(this),
        switchMap((id) => {
          if (!id) return of(null);

          const request$ =
            this.selectedCrosswordType === CrosswordType.STARTED
              ? this.api.getStartedCrossword(id)
              : this.api.getUnstartedCrossword(id);

          return request$;
        }),
        filter((item) => !!item),
      )
      .subscribe((crossword) => this.selectedCrossword$.next(crossword));

    this.selectedCrossword$
      .pipe(untilDestroyed(this))
      .subscribe((crossword) => {
        if (!crossword) return;

        this.updateCrossword(
          crossword.size.height,
          crossword.size.width,
          crossword.words,
          crossword.grid,
        );
      });

    this.updateThemes();
  }

  ngOnInit(): void {}

  private updateCrossword(
    height: number,
    width: number,
    words: CrosswordWord[],
    grid: CrosswordGrid[] = [],
  ) {
    const matrix: string[][] = [];
    const solvedMatrix: boolean[][] = [];
    for (let index = 0; index < height; index++) {
      matrix[index] = new Array(width);
      solvedMatrix[index] = new Array(width);
    }

    for (const word of words) {
      if (word.p1.x !== word.p2.x) {
        for (let index = 0; index <= word.p2.x - word.p1.x; index++) {
          const y = word.p1.y;
          const x = word.p1.x + index;
          matrix[y][x] = '';
          if (word.isSolved) solvedMatrix[y][x] = true;
        }
      }

      if (word.p1.y !== word.p2.y) {
        for (let index = 0; index <= word.p2.y - word.p1.y; index++) {
          const y = word.p1.y + index;
          const x = word.p1.x;
          matrix[y][x] = '';
          if (word.isSolved) solvedMatrix[y][x] = true;
        }
      }
    }

    if (grid.length) {
      for (const item of grid) {
        matrix[item.y][item.x] = item.l === ' ' ? '' : item.l;
      }
    }

    this.solvedMatrix = solvedMatrix;
    this.crosswordMatrix$.next(matrix);
  }

  onTakePrompt() {
    const x = this.selectedCell?.x as number;
    const y = this.selectedCell?.y as number;

    this.api.takePrompt(this.selectedCrosswordId, x, y).subscribe(
      (result) => {
        if (this.selectedCrossword$.value) {
          this.selectedCrossword$.value.promptCount--;
        }

        const matrix: string[][] = [];
        const oldMatrix = this.crosswordMatrix$.value;
        for (let index = 0; index < oldMatrix.length; index++) {
          matrix[index] = [...oldMatrix[index]];
        }
        matrix[y][x] = result.letter;
        this.crosswordMatrix$.next(matrix);
        // this.crosswordMatrix$.value[y][x] = result.letter;

        if (result.solvedWords?.length) {
          this.applySolvedWords(result.solvedWords);
        }
      },
      (error) => {
        this.notify.error('Ошибка', error.error.message);
      },
    );
  }

  onLetterClick(y: number, x: number, letter: string | undefined) {
    if (letter === undefined) return;

    this.selectedCell = { x, y };
    const selectedCrossword = this.selectedCrossword$.value;
    if (!selectedCrossword) return;

    this.selectedWordsIds = getWordsForPoint(
      selectedCrossword?.words,
      x,
      y,
    ).map((word) => word.id);
  }

  onLetterChange(letter: string, y: number, x: number) {
    // this.crosswordMatrix$.value[y][x] = letter;

    const crosswordId = this.selectedCrosswordId;
    this.api.changeLetter(crosswordId, x, y, letter || ' ').subscribe(
      (result) => {
        if (!result.solvedWords?.length) return;

        this.applySolvedWords(result.solvedWords);
      },
      (error) => {
        this.notify.error('Ошибка', error.error.message);
      },
    );
  }

  private applySolvedWords(solvedWords: { id: number }[]) {
    for (const solvedWord of solvedWords) {
      const word = this.selectedCrossword$.value?.words.find(
        (item) => item.id === solvedWord.id,
      );
      if (!word) continue;

      if (word.p1.x !== word.p2.x) {
        for (let index = 0; index <= word.p2.x - word.p1.x; index++) {
          this.solvedMatrix[word.p1.y][word.p1.x + index] = true;
        }
      }

      if (word.p1.y !== word.p2.y) {
        for (let index = 0; index <= word.p2.y - word.p1.y; index++) {
          this.solvedMatrix[word.p1.y + index][word.p1.x] = true;
        }
      }
    }
  }

  private updateThemes() {
    this.api.getThemes().subscribe((result) => this.themes$.next(result));
  }

  private updateCrosswordsList(themeId: number, type: CrosswordType) {
    const request$ =
      type === CrosswordType.STARTED
        ? this.api.getStartedCrosswordList(themeId)
        : this.api.getUnstartedCrosswordList(themeId);

    request$.subscribe((result) => this.crosswords$.next(result));
  }

  private deselectCrossword() {
    this.selectedCell = null;

    this.crosswordsForm.get('crossword')?.setValue(null, { emitEvent: false });
    this.selectedCrossword$.next(null);
    this.crosswordMatrix$.next([[]]);
  }
}
