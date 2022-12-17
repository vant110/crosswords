import { Component } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { NzModalService } from 'ng-zorro-antd/modal';
import { NzNotificationService } from 'ng-zorro-antd/notification';
import { BehaviorSubject, map, Observable, switchMap } from 'rxjs';
import { Crossword, CrosswordList } from 'src/app/core/models/crossword';
import { Dictionary } from 'src/app/core/models/dictionary';
import { CrosswordTheme } from 'src/app/core/models/theme';
import { ApiService } from 'src/app/core/services/api.service';
import { CrosswordAddComponent } from 'src/app/modals/crossword-add/crossword-add.component';
import { ThemeAddComponent } from 'src/app/modals/theme-add/theme-add.component';

@UntilDestroy()
@Component({
  selector: 'app-admin-crosswords',
  templateUrl: './admin-crosswords.component.html',
  styleUrls: ['./admin-crosswords.component.scss'],
})
export class AdminCrosswordsComponent {
  themes$ = new BehaviorSubject<CrosswordTheme[]>([]);
  themesForm = this.formBuilder.group({
    theme: [null],
  });

  crosswords$ = new BehaviorSubject<CrosswordList[]>([]);
  crosswordsForm = this.formBuilder.group({
    crossword: [null],
  });

  selectedCrossword$ = new BehaviorSubject<Crossword | null>(null);
  crosswordDictionaryName$: Observable<string>;

  private _dictionaries!: Dictionary[];

  get selectedThemeId() {
    return this.themesForm.get('theme')?.value as unknown as number;
  }

  get selectedCrosswordId() {
    return this.crosswordsForm.get('crossword')?.value as unknown as number;
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
        this.crosswordsForm
          .get('crossword')
          ?.setValue(null, { emitEvent: false });
        this.selectedCrossword$.next(null);
        this.updateCrosswordsList(themeId as unknown as number);
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
    crossword.themeId = this.selectedThemeId;
    crossword.name =
      this.crosswords$.value.find(
        (item) => item.id === this.selectedCrosswordId,
      )?.name || '';

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
      crossword.words = this.selectedCrossword$.value?.words || [];

      this.api.putCrossword(crossword, this.selectedCrosswordId).subscribe(
        () => {
          this.notify.success(
            'Успех',
            `Кроссворд ${crossword.name} успешно изменен`,
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

  onDeleteCrosword() {
    if (!this.selectedCrossword$.value || !this.selectedCrosswordId) return;

    this.api.deleteCrossword(this.selectedCrosswordId).subscribe(
      () => {
        this.notify.success('Успех', 'Кроссворд успешно удален');
        this.updateCrosswordsList(this.selectedThemeId);
        this.crosswordsForm
          .get('crossword')
          ?.setValue(null, { emitEvent: false });
        this.selectedCrossword$.next(null);
      },
      (error) => {
        this.notify.error('Ошибка', error.error.message);
      },
    );
  }

  onGenerateCrossword() {}

  onSaveCrossword() {}

  private updateThemes() {
    this.api.getThemes().subscribe((result) => this.themes$.next(result));
  }

  private updateCrosswordsList(themeId: number) {
    this.api
      .getCrosswordsList(themeId)
      .subscribe((result) => this.crosswords$.next(result));
  }
}
