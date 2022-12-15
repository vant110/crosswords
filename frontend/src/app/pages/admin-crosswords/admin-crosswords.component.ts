import { Component } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { UntilDestroy } from '@ngneat/until-destroy';
import { NzModalService } from 'ng-zorro-antd/modal';
import { NzNotificationService } from 'ng-zorro-antd/notification';
import { BehaviorSubject } from 'rxjs';
import { CrosswordTheme } from 'src/app/core/models/theme';
import { ApiService } from 'src/app/core/services/api.service';
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

  get selectedTheme() {
    return this.themesForm.get('theme')?.value as unknown as number;
  }

  constructor(
    private api: ApiService,
    private modal: NzModalService,
    private formBuilder: FormBuilder,
    private notify: NzNotificationService,
  ) {
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
    if (!this.selectedTheme) return;

    const theme = this.themes$.value.find(
      (item) => item.id === this.selectedTheme,
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

      this.api.putTheme(this.selectedTheme, name).subscribe(
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
    if (!this.selectedTheme) return;

    this.api.deleteTheme(this.selectedTheme).subscribe(
      () => {
        this.notify.success('Успех', 'Тема успешно удалена');
        this.updateThemes();
        this.themesForm.get('theme')?.setValue(null);
      },
      (error) => {
        this.notify.error('Ошибка', error.error.message);
      },
    );
  }

  private updateThemes() {
    this.api.getThemes().subscribe((result) => this.themes$.next(result));
  }
}
