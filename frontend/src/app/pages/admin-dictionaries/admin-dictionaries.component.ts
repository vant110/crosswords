import { AfterViewInit, Component, ViewChild } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { NzModalService } from 'ng-zorro-antd/modal';
import { NzNotificationService } from 'ng-zorro-antd/notification';
import { NzTableComponent } from 'ng-zorro-antd/table';
import { BehaviorSubject, debounceTime, filter, switchMap, tap } from 'rxjs';
import { Dictionary } from 'src/app/core/models/dictionary';
import { sortingOptions, WordSort } from 'src/app/core/models/sorting';
import { DictionaryWord } from 'src/app/core/models/word';
import { ApiService } from 'src/app/core/services/api.service';
import { DictionaryAddComponent } from 'src/app/modals/dictionary-add/dictionary-add.component';
import { WordAddComponent } from 'src/app/modals/word-add/word-add.component';

@UntilDestroy()
@Component({
  selector: 'app-admin-dictionaries',
  templateUrl: './admin-dictionaries.component.html',
  styleUrls: ['./admin-dictionaries.component.scss'],
})
export class AdminDictionariesComponent implements AfterViewInit {
  @ViewChild('wordsTable', { static: false })
  nzTableComponent?: NzTableComponent<DictionaryWord>;

  words$ = new BehaviorSubject<DictionaryWord[]>([]);

  dictionaries$ = new BehaviorSubject<Dictionary[]>([]);
  dictionariesForm = this.formBuilder.group({
    dictionary: [null],
  });

  wordsForm = this.formBuilder.group({
    search: [null],
    sort: [WordSort.ASC_APLHABET],
  });

  sortOptions = sortingOptions;

  selectedRow: number = -1;

  private _gridIndex = 0;
  private _gridLoading = false;

  get selectedDictionary() {
    return this.dictionariesForm.get('dictionary')?.value as unknown as number;
  }

  constructor(
    private api: ApiService,
    private modal: NzModalService,
    private formBuilder: FormBuilder,
    private notify: NzNotificationService,
  ) {
    this.updateDictionaries();

    this.dictionariesForm
      .get('dictionary')
      ?.valueChanges.pipe(untilDestroyed(this))
      .subscribe((id) => {
        const dictionaryId = id as unknown as number;
        this.updateWords(dictionaryId);
      });

    this.wordsForm.valueChanges
      .pipe(untilDestroyed(this), debounceTime(500))
      .subscribe((value) => {
        const sort = value.sort as WordSort;
        const search = value.search as unknown as string;

        this.api
          .getWords(this.selectedDictionary, sort, search)
          .subscribe((result) => {
            this.nzTableComponent?.cdkVirtualScrollViewport?.scrollToIndex(0);
            this.words$.next(result);
          });
      });
  }

  ngAfterViewInit(): void {
    this.nzTableComponent?.cdkVirtualScrollViewport?.scrolledIndexChange
      .pipe(
        untilDestroyed(this),
        filter((index) => {
          this._gridIndex = index;
          const loaded = this.words$.value.length;

          return index > loaded - 25 && loaded >= 25 && !this._gridLoading;
        }),
        tap(() => (this._gridLoading = true)),
        switchMap(() => {
          const filters = this.wordsForm.value;
          const loaded = this.words$.value.length;

          const sort = filters.sort as WordSort;
          const search = filters.search as unknown as string;
          const lastLoaded = this.words$.value[loaded - 1];

          return this.api.getWords(
            this.selectedDictionary,
            sort,
            search,
            lastLoaded.name,
          );
        }),
      )
      .subscribe((data) => {
        this._gridLoading = false;
        const loadedWords = this.words$.value;
        this.words$.next([...loadedWords, ...data]);
      });
  }

  updateDictionaries() {
    this.api
      .getDictionaries()
      .subscribe((result) => this.dictionaries$.next(result));
  }

  updateWords(dictionaryId: number) {
    const filters = this.wordsForm.value;
    const sort = filters.sort as WordSort;
    const search = filters.search as unknown as string;

    this.api
      .getWords(dictionaryId, sort, search)
      .subscribe((result) => this.words$.next(result));
  }

  onAddDictionary() {
    this.modal.create({
      nzContent: DictionaryAddComponent,
      nzTitle: 'Добавить словарь',
      nzOkText: 'Сохранить',
      nzCancelText: 'Отмена',
      nzWidth: 420,
      nzOkDisabled: true,
      nzOnOk: (instance: DictionaryAddComponent) =>
        this.createDictionary(instance),
    });
  }

  private createDictionary(instance: DictionaryAddComponent) {
    return new Promise((resolve) => {
      const data = instance.form.value;
      const formData = new FormData();
      formData.append('name', data.name as string);
      if (instance.file) formData.append('dictionary', instance.file as File);

      this.api.createDictionary(formData).subscribe(
        () => {
          this.notify.success('Успех', `Словарь ${data.name} успешно создан`);
          this.updateDictionaries();
          resolve(true);
        },
        (error) => {
          this.notify.error('Ошибка', error.error.message);
          resolve(true);
        },
      );
    });
  }

  onEditDictionary() {
    if (!this.selectedDictionary) return;

    const name = this.dictionaries$.value.find(
      (item) => item.id === this.selectedDictionary,
    )?.name;

    this.modal.create({
      nzContent: DictionaryAddComponent,
      nzComponentParams: {
        isEdit: true,
        name: name,
      },
      nzTitle: 'Редактировать словарь',
      nzOkText: 'Сохранить',
      nzCancelText: 'Отмена',
      nzWidth: 420,
      nzOkDisabled: true,
      nzOnOk: (instance: DictionaryAddComponent) =>
        this.editDictionary(instance),
    });
  }

  private editDictionary(instance: DictionaryAddComponent) {
    return new Promise((resolve) => {
      const name = instance.form.value.name as string;
      const id = this.selectedDictionary;

      this.api.editDictionary(id, name).subscribe(
        () => {
          this.notify.success('Успех', `Словарь ${name} успешно изменен`);
          this.updateDictionaries();
          resolve(true);
        },
        (error) => {
          this.notify.error('Ошибка', error.error.message);
          resolve(true);
        },
      );
    });
  }

  onDeleteDictionary() {
    if (!this.selectedDictionary) return;

    this.api.deleteDictionary(this.selectedDictionary).subscribe(
      () => {
        this.notify.success('Успех', `Словарь успешно удален`);
        this.updateDictionaries();
        this.dictionariesForm.get('dictionary')?.setValue(null);
      },
      (error) => {
        this.notify.error('Ошибка', error.error.message);
      },
    );
  }

  onAddWord() {
    if (!this.selectedRow) return;

    this.modal.create({
      nzContent: WordAddComponent,
      nzTitle: 'Добавить слово',
      nzOkText: 'Сохранить',
      nzCancelText: 'Отмена',
      nzWidth: 420,
      nzOkDisabled: true,
      nzOnOk: (instance: WordAddComponent) => this.createWord(instance),
    });
  }

  private createWord(instance: WordAddComponent) {
    return new Promise((resolve) => {
      const data = instance.form.value;

      this.api
        .createWord(
          this.selectedDictionary,
          data.word as string,
          data.definition as string,
        )
        .subscribe(
          () => {
            this.notify.success('Успех', `Слово успешно создано`);
            this.updateWords(this.selectedDictionary);
            resolve(true);
          },
          (error) => {
            this.notify.error('Ошибка', error.error.message);
            resolve(true);
          },
        );
    });
  }

  onEditWord() {
    if (!this.selectedRow) return;
    const word = this.words$.value.find((item) => item.id === this.selectedRow);

    this.modal.create({
      nzContent: WordAddComponent,
      nzComponentParams: {
        isEdit: true,
        word,
      },
      nzTitle: 'Изменить слово',
      nzOkText: 'Сохранить',
      nzCancelText: 'Отмена',
      nzWidth: 420,
      nzOkDisabled: true,
      nzOnOk: (instance: WordAddComponent) => this.editWord(instance),
    });
  }

  private editWord(instance: WordAddComponent) {
    return new Promise((resolve) => {
      const data = instance.form.value;

      this.api.editWord(this.selectedRow, data.definition as string).subscribe(
        () => {
          this.notify.success('Успех', `Слово успешно изменено`);
          this.updateWords(this.selectedDictionary);
          resolve(true);
        },
        (error) => {
          this.notify.error('Ошибка', error.error.message);
          resolve(true);
        },
      );
    });
  }

  onDeleteWord() {
    this.api.deleteWord(this.selectedRow).subscribe(
      () => {
        this.notify.success('Успех', `Слово успешно удалено`);
        this.updateWords(this.selectedDictionary);
      },
      (error) => {
        this.notify.error('Ошибка', error.error.message);
      },
    );
  }

  onRowClick(id: number) {
    this.selectedRow = id;
  }

  trackByIndex(_: number, data: DictionaryWord): number {
    return data.id;
  }
}
