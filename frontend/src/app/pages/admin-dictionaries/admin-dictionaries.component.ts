import { AfterViewInit, Component, ViewChild } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { NzModalService } from 'ng-zorro-antd/modal';
import { NzNotificationService } from 'ng-zorro-antd/notification';
import { NzTableComponent } from 'ng-zorro-antd/table';
import { BehaviorSubject, debounceTime, filter, switchMap, tap } from 'rxjs';
import { Dictionary } from 'src/app/core/models/dictionary';
import { DictionaryWord } from 'src/app/core/models/word';
import { ApiService } from 'src/app/core/services/api.service';
import { DictionaryAddComponent } from 'src/app/modals/dictionary-add/dictionary-add.component';

enum WordSort {
  ASC_APLHABET = 'ascAlphabet',
  DESC_APLHABET = 'descAlphabet',
  DESC_LENGTH = 'descLength',
  ASC_LENGTH = 'ascLength',
}

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

  sortOptions = [
    { id: WordSort.ASC_APLHABET, name: 'По алфавиту (А-Я)' },
    { id: WordSort.DESC_APLHABET, name: 'По алфавиту (Я-А)' },
    { id: WordSort.ASC_LENGTH, name: 'По длине (возр.)' },
    { id: WordSort.DESC_LENGTH, name: 'По длине (убыв.)' },
  ];

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
          console.log(index);
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
    const id = this.selectedDictionary as any;

    const name = this.dictionaries$.value.find((item) => item.id === id)?.name;

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
      const id = this.selectedDictionary as any;

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
    const id = this.selectedDictionary as any;

    this.api.deleteDictionary(id).subscribe(
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

  onAddWord() {}

  onEditWord() {}

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
