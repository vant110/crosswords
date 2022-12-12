import { Component, Input } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { NzModalRef } from 'ng-zorro-antd/modal';

@UntilDestroy()
@Component({
  selector: 'app-dictionary-add',
  templateUrl: './dictionary-add.component.html',
  styleUrls: ['./dictionary-add.component.scss'],
})
export class DictionaryAddComponent {
  @Input() isEdit = false;
  @Input() set name(value: string) {
    if (!value) return;
    this.form.get('name')?.setValue(value);
  }

  fileName: string | null = null;
  file?: File;

  form = this.formBuilder.group({
    file: [null],
    name: ['', [Validators.required]],
  });

  constructor(private formBuilder: FormBuilder, private modalRef: NzModalRef) {
    this.form.statusChanges.pipe(untilDestroyed(this)).subscribe((status) => {
      const isValid = status === 'VALID';
      this.modalRef.updateConfig({ nzOkDisabled: !isValid });
    });
  }

  upload(event: Event) {
    const target = event.target as HTMLInputElement;

    this.file = target.files?.[0];
    this.fileName = this.file?.name as string;

    this.form.patchValue({ file: this.file as any, name: this.fileName });
  }
}
