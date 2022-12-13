import { Component, Input } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { NzModalRef } from 'ng-zorro-antd/modal';
import { DictionaryWord } from 'src/app/core/models/word';

@UntilDestroy()
@Component({
  selector: 'app-word-add',
  templateUrl: './word-add.component.html',
  styleUrls: ['./word-add.component.scss'],
})
export class WordAddComponent {
  @Input() isEdit = false;
  @Input() set word(value: DictionaryWord) {
    if (!value) return;
    this.form.setValue({
      word: value.name,
      definition: value.definition,
    });
  }

  form = this.formBuilder.group({
    word: ['', [Validators.required]],
    definition: ['', [Validators.required]],
  });

  constructor(private formBuilder: FormBuilder, private modalRef: NzModalRef) {
    this.form.statusChanges.pipe(untilDestroyed(this)).subscribe((status) => {
      const isValid = status === 'VALID';
      this.modalRef.updateConfig({ nzOkDisabled: !isValid });
    });
  }
}
