import { AfterViewInit, Component, ViewChild } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { NzModalService } from 'ng-zorro-antd/modal';
import { NzNotificationService } from 'ng-zorro-antd/notification';
import { NzTableComponent } from 'ng-zorro-antd/table';
import {
  BehaviorSubject,
  debounceTime,
  filter,
  map,
  Observable,
  switchMap,
  tap,
} from 'rxjs';
import {
  Crossword,
  CrosswordList,
  CrosswordWord,
} from 'src/app/core/models/crossword';
import { Dictionary } from 'src/app/core/models/dictionary';
import { SelectedPoint } from 'src/app/core/models/selected-point';
import { sortingOptions, WordSort } from 'src/app/core/models/sorting';
import { CrosswordTheme } from 'src/app/core/models/theme';
import { AvailableWord } from 'src/app/core/models/word';
import { ApiService } from 'src/app/core/services/api.service';
import { getWordsForPoint } from 'src/app/core/utils/utils';
import { CrosswordAddComponent } from 'src/app/modals/crossword-add/crossword-add.component';
import { ThemeAddComponent } from 'src/app/modals/theme-add/theme-add.component';

@UntilDestroy()
@Component({
  selector: 'app-admin-crosswords',
  templateUrl: './admin-crosswords.component.html',
  styleUrls: ['./admin-crosswords.component.scss'],
})
export class AdminCrosswordsComponent implements AfterViewInit {
  @ViewChild('availableWordsTable', { static: false })
  nzTableComponent?: NzTableComponent<AvailableWord>;

  themes$ = new BehaviorSubject<CrosswordTheme[]>([]);
  themesForm = this.formBuilder.group({
    theme: [null],
  });

  crosswords$ = new BehaviorSubject<CrosswordList[]>([]);
  crosswordsForm = this.formBuilder.group({
    crossword: [null],
  });

  selectedCrossword$ = new BehaviorSubject<Crossword | null>(null);
  crosswordMatrix$ = new BehaviorSubject<string[][]>([[]]);
  crosswordDictionaryName$: Observable<string>;

  selectedCell: SelectedPoint = { x: null, y: null };
  selectedEndCell: SelectedPoint = { x: null, y: null };

  availableWords$ = new BehaviorSubject<AvailableWord[]>([]);

  wordsForm = this.formBuilder.group({
    search: [''],
    sort: [WordSort.ASC_APLHABET],
    mask: [''],
  });
  sortOptions = sortingOptions;

  private _dictionaries!: Dictionary[];
  private _words: CrosswordWord[] = [];
  private _gridLoading = false;

  get selectedThemeId() {
    return this.themesForm.get('theme')?.value as unknown as number;
  }

  get selectedCrosswordId() {
    return this.crosswordsForm.get('crossword')?.value as unknown as number;
  }

  get words() {
    return this._words;
  }

  constructor(
    private api: ApiService,
    private modal: NzModalService,
    private formBuilder: FormBuilder,
    private notify: NzNotificationService,
  ) {
    this.themesForm
      .get('theme')
      ?.valueChanges.pipe(untilDestroyed(this))
      .subscribe((themeId) => {
        this.updateCrosswordsList(themeId as unknown as number);
        this.deselectCrossword();
      });

    this.crosswordsForm
      .get('crossword')
      ?.valueChanges.pipe(
        untilDestroyed(this),
        switchMap((crosswordId) =>
          this.api.getCrossword(crosswordId as unknown as number),
        ),
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
        );
      });

    this.wordsForm.valueChanges
      .pipe(untilDestroyed(this), debounceTime(500))
      .subscribe((value) => {
        const sort = value.sort as WordSort;
        const search = value.search as string;
        const mask = value.mask as string;

        const dictionaryId = this.selectedCrossword$.value
          ?.dictionaryId as number;

        this.api
          .getAvailableWords(dictionaryId, sort, search, mask)
          .subscribe((result) => {
            this.nzTableComponent?.cdkVirtualScrollViewport?.scrollToIndex(0);
            this.availableWords$.next(result);
          });
      });

    this.api
      .getDictionaries()
      .subscribe((result) => (this._dictionaries = result));

    this.crosswordDictionaryName$ = this.selectedCrossword$.pipe(
      map(
        (crossword) =>
          this._dictionaries.find((item) => item.id === crossword?.dictionaryId)
            ?.name || '',
      ),
    );

    this.updateThemes();
  }

  ngAfterViewInit(): void {
    this.nzTableComponent?.cdkVirtualScrollViewport?.scrolledIndexChange
      .pipe(
        untilDestroyed(this),
        filter((index) => {
          const loaded = this.availableWords$.value.length;

          return index > loaded - 25 && loaded >= 25 && !this._gridLoading;
        }),
        tap(() => (this._gridLoading = true)),
        switchMap(() => {
          const filters = this.wordsForm.value;
          const loaded = this.availableWords$.value.length;

          const sort = filters.sort as WordSort;
          const search = filters.search as string;
          const mask = filters.mask as string;
          const lastLoaded = this.availableWords$.value[loaded - 1];

          return this.api.getAvailableWords(
            this.selectedCrossword$.value?.dictionaryId as number,
            sort,
            search,
            mask,
            lastLoaded.name,
          );
        }),
      )
      .subscribe((data) => {
        this._gridLoading = false;
        const loadedWords = this.availableWords$.value;
        this.availableWords$.next([...loadedWords, ...data]);
      });
  }

  private updateCrossword(
    height: number,
    width: number,
    words: CrosswordWord[],
  ) {
    this._words = [...words];

    const matrix: string[][] = [];
    for (let index = 0; index < height; index++) {
      matrix[index] = new Array(width);
    }

    for (const word of words) {
      if (word.p1.x !== word.p2.x) this.writeVerticalWord(matrix, word);

      if (word.p1.y !== word.p2.y) this.writeHorizontalWord(matrix, word);
    }

    this.crosswordMatrix$.next(matrix);
    // const printMatrix = matrix.map((d) => d.join('\t')).join('\n');
    // console.log(printMatrix);
  }

  private writeVerticalWord(matrix: string[][], word: CrosswordWord) {
    for (let index = 0; index < word.name.length; index++) {
      const y = word.p1.y;
      const x = word.p2.x > word.p1.x ? word.p1.x + index : word.p1.x - index;
      matrix[y][x] = word.name[index];
    }
  }

  private writeHorizontalWord(matrix: string[][], word: CrosswordWord) {
    for (let index = 0; index < word.name.length; index++) {
      const y = word.p2.y > word.p1.y ? word.p1.y + index : word.p1.y - index;
      const x = word.p1.x;
      matrix[y][x] = word.name[index];
    }
  }

  cellClick(y: number, x: number) {
    if (y === this.selectedCell.y && x === this.selectedCell.x) return;

    if (y === this.selectedCell.y || x === this.selectedCell.x) {
      this.selectedEndCell = { y, x };
      this.updateMask();
      return;
    }
    this.selectedCell = { y, x };
    this.selectedEndCell = { y: null, x: null };
  }

  private updateMask() {
    const [start, end] = this.getFunctionalPoints(
      this.selectedCell,
      this.selectedEndCell,
    );
    if (!start || !end) return;

    const matrixMask = [];
    const matrix = this.crosswordMatrix$.value;
    if (
      start.x !== null &&
      start.y !== null &&
      end.x !== null &&
      end.y !== null
    ) {
      if (start.x === end.x) {
        for (let index = 0; index <= end.y - start.y; index++) {
          matrixMask.push(matrix[start.y + index][start.x]);
        }
      }

      if (start.y === end.y) {
        for (let index = 0; index <= end.x - start.x; index++) {
          matrixMask.push(matrix[start.y][start.x + index]);
        }
      }
    }

    const adaptedMask = matrixMask.map((value) => value ?? '.').join('');
    this.wordsForm.get('mask')?.setValue(adaptedMask);
  }

  cellDoubleclick(y: number, x: number) {
    const matchedWords = getWordsForPoint(this._words, x, y);

    if (matchedWords.length !== 1) return;

    this._words = [
      ...this._words.filter((word) => word.id !== matchedWords[0].id),
    ];
    const selectedCrossword = this.selectedCrossword$.value;
    const width = selectedCrossword?.size.width ?? 0;
    const height = selectedCrossword?.size.height ?? 0;
    this.updateCrossword(height, width, this._words);
  }

  onAddTheme() {
    this.modal.create({
      nzContent: ThemeAddComponent,
      nzTitle: 'Добавить тему',
      nzOkText: 'Сохранить',
      nzCancelText: 'Отмена',
      nzWidth: 420,
      nzOkDisabled: true,
      nzOnOk: (instance: ThemeAddComponent) => this.createTheme(instance),
    });
  }

  private createTheme(instance: ThemeAddComponent) {
    return new Promise((resolve) => {
      const data = instance.form.value;
      this.api.createTheme(data.name as string).subscribe(
        () => {
          this.notify.success('Успех', `Тема ${data.name} успешно создана`);
          this.updateThemes();
          resolve(true);
        },
        (error) => {
          this.notify.error('Ошибка', error.error.message);
          resolve(true);
        },
      );
    });
  }

  onEditTheme() {
    if (!this.selectedThemeId) return;

    const theme = this.themes$.value.find(
      (item) => item.id === this.selectedThemeId,
    );

    this.modal.create({
      nzContent: ThemeAddComponent,
      nzComponentParams: {
        theme,
      },
      nzTitle: 'Редактировать тему',
      nzOkText: 'Сохранить',
      nzCancelText: 'Отмена',
      nzWidth: 420,
      nzOkDisabled: true,
      nzOnOk: (instance: ThemeAddComponent) => this.editTheme(instance),
    });
  }

  private editTheme(instance: ThemeAddComponent) {
    return new Promise((resolve) => {
      const name = instance.form.value.name as string;

      this.api.putTheme(this.selectedThemeId, name).subscribe(
        () => {
          this.notify.success('Успех', `Тема ${name} успешно изменена`);
          this.updateThemes();
          resolve(true);
        },
        (error) => {
          this.notify.error('Ошибка', error.error.message);
          resolve(true);
        },
      );
    });
  }

  onDeleteTheme() {
    if (!this.selectedThemeId) return;

    this.api.deleteTheme(this.selectedThemeId).subscribe(
      () => {
        this.notify.success('Успех', 'Тема успешно удалена');
        this.updateThemes();
        this.themesForm.get('theme')?.setValue(null, { emitEvent: false });
      },
      (error) => {
        this.notify.error('Ошибка', error.error.message);
      },
    );
  }

  onAddCrossword() {
    this.modal.create({
      nzContent: CrosswordAddComponent,
      nzComponentParams: {
        dictionaries: this._dictionaries,
        themes: this.themes$.value,
      },
      nzTitle: 'Создать кроссворд',
      nzOkText: 'Сохранить',
      nzCancelText: 'Отмена',
      nzWidth: 420,
      nzOkDisabled: true,
      nzOnOk: (instance: CrosswordAddComponent) =>
        this.createCrossword(instance),
    });
  }

  private createCrossword(instance: CrosswordAddComponent) {
    return new Promise((resolve) => {
      const crossword = instance.form.value as Crossword;
      crossword.words = [];

      this.api.createCrossword(crossword).subscribe(
        () => {
          this.notify.success(
            'Успех',
            `Кроссворд ${crossword.name} успешно создан`,
          );
          this.updateCrosswordsList(this.selectedThemeId);
          resolve(true);
        },
        (error) => {
          this.notify.error('Ошибка', error.error.message);
          resolve(true);
        },
      );
    });
  }

  onEditCrossword() {
    if (!this.selectedCrossword$.value || !this.selectedCrosswordId) return;

    const crossword = this.selectedCrossword$.value;
    crossword.words = this._words;

    this.modal.create({
      nzContent: CrosswordAddComponent,
      nzComponentParams: {
        dictionaries: this._dictionaries,
        themes: this.themes$.value,
        crossword,
      },
      nzTitle: 'Изменить кроссворд',
      nzOkText: 'Сохранить',
      nzCancelText: 'Отмена',
      nzWidth: 420,
      nzOkDisabled: true,
      nzOnOk: (instance: CrosswordAddComponent) => this.editCrossword(instance),
    });
  }

  private editCrossword(instance: CrosswordAddComponent) {
    return new Promise((resolve) => {
      const crossword = instance.form.value as Crossword;
      const selectedCrossword = this.selectedCrossword$.value;
      const sameDictionary =
        selectedCrossword?.dictionaryId === crossword.dictionaryId;
      const sameHeight =
        selectedCrossword?.size.height === crossword.size.height;
      const sameWidth = selectedCrossword?.size.width === crossword.size.width;

      crossword.words =
        sameDictionary && sameHeight && sameWidth
          ? selectedCrossword?.words || []
          : [];

      this.api.putCrossword(crossword, this.selectedCrosswordId).subscribe(
        () => {
          this.notify.success(
            'Успех',
            `Кроссворд ${crossword.name} успешно изменен`,
          );
          this.updateCrosswordsList(this.selectedThemeId);
          this.selectedCrossword$.next(crossword);
          resolve(true);
        },
        (error) => {
          this.notify.error('Ошибка', error.error.message);
          resolve(true);
        },
      );
    });
  }

  onDeleteCrosword() {
    if (!this.selectedCrossword$.value || !this.selectedCrosswordId) return;

    this.api.deleteCrossword(this.selectedCrosswordId).subscribe(
      () => {
        this.notify.success('Успех', 'Кроссворд успешно удален');
        this.updateCrosswordsList(this.selectedThemeId);
        this.deselectCrossword();
      },
      (error) => {
        this.notify.error('Ошибка', error.error.message);
      },
    );
  }

  onGenerateCrossword() {
    const selectedCrossword = this.selectedCrossword$.value;
    const width = selectedCrossword?.size.width ?? 0;
    const height = selectedCrossword?.size.height ?? 0;

    this.selectedCell = { x: null, y: null };
    this.selectedEndCell = { x: null, y: null };

    this.api
      .generateCrossword(width, height, selectedCrossword?.dictionaryId ?? 0)
      .subscribe(
        (result) => {
          this.updateCrossword(height, width, result);
        },
        (error) => {
          this.notify.error('Ошибка', error.error.message);
        },
      );
  }

  onSaveCrossword() {
    if (!this.selectedCrossword$.value || !this.selectedCrosswordId) return;

    const crossword: Crossword = {
      ...this.selectedCrossword$.value,
      words: this._words,
    };

    this.api.putCrossword(crossword, this.selectedCrosswordId).subscribe(
      () => {
        this.notify.success(
          'Успех',
          `Кроссворд ${crossword.name} успешно сохранен`,
        );
      },
      (error) => {
        this.notify.error('Ошибка', error.error.message);
      },
    );
  }

  trackByIndex(_: number, data: AvailableWord): number {
    return data.id;
  }

  onAvailableWordClick(word: AvailableWord) {
    if (!word) return;
    if (this._words.some((item) => item.id === word.id)) return;

    const [start, end] = this.getFunctionalPoints(
      this.selectedCell,
      this.selectedEndCell,
    );
    if (!start || !end) return;

    const crosswordWord: CrosswordWord = {
      id: word.id,
      definition: word.definition,
      name: word.name,
      p1: { x: start.x as number, y: start.y as number },
      p2: { x: end.x as number, y: end.y as number },
    };

    if (word.offset) {
      if (crosswordWord.p1.x === crosswordWord.p2.x)
        crosswordWord.p1.y += word.offset;

      if (crosswordWord.p1.y === crosswordWord.p2.y)
        crosswordWord.p1.x += word.offset;
    }

    this._words.push(crosswordWord);
    this._words = [...this._words];

    const selectedCrossword = this.selectedCrossword$.value;
    const width = selectedCrossword?.size.width ?? 0;
    const height = selectedCrossword?.size.height ?? 0;
    this.updateCrossword(height, width, this._words);
    this.updateMask();
  }

  private updateThemes() {
    this.api.getThemes().subscribe((result) => this.themes$.next(result));
  }

  private updateCrosswordsList(themeId: number) {
    this.api
      .getCrosswordsList(themeId)
      .subscribe((result) => this.crosswords$.next(result));
  }

  private deselectCrossword() {
    this.selectedCell = { x: null, y: null };
    this.selectedEndCell = { x: null, y: null };

    this.crosswordsForm.get('crossword')?.setValue(null, { emitEvent: false });
    this.selectedCrossword$.next(null);
    this.crosswordMatrix$.next([[]]);
  }

  private getFunctionalPoints(start: SelectedPoint, end: SelectedPoint) {
    if (
      start.x === null ||
      start.y === null ||
      end.x === null ||
      end.y === null
    )
      return [null, null];

    if (start.x === end.x) {
      return start.y < end.y ? [start, end] : [end, start];
    } else if (start.y === end.y) {
      return start.x < end.x ? [start, end] : [end, start];
    } else return [null, null];
  }
}
