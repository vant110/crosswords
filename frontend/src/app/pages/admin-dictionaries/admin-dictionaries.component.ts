import { Component } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { NzModalService } from 'ng-zorro-antd/modal';
import { NzNotificationService } from 'ng-zorro-antd/notification';
import { BehaviorSubject } from 'rxjs';
import { Dictionary } from 'src/app/core/models/dictionary';
import { ApiService } from 'src/app/core/services/api.service';
import { DictionaryAddComponent } from 'src/app/modals/dictionary-add/dictionary-add.component';

@UntilDestroy()
@Component({
  selector: 'app-admin-dictionaries',
  templateUrl: './admin-dictionaries.component.html',
  styleUrls: ['./admin-dictionaries.component.scss'],
})
export class AdminDictionariesComponent {
  dictionaries$ = new BehaviorSubject<Dictionary[]>([]);
  dictionariesForm = this.formBuilder.group({
    dictionary: [null],
  });

  get selectedDictionary() {
    return this.dictionariesForm.get('dictionary')?.value;
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
      .subscribe(console.log);
  }

  updateDictionaries() {
    this.api
      .getDictionaries()
      .subscribe((result) => this.dictionaries$.next(result));
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
}
