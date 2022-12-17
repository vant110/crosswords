import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { NzModalRef } from 'ng-zorro-antd/modal';
import { Crossword } from 'src/app/core/models/crossword';
import { Dictionary } from 'src/app/core/models/dictionary';
import { CrosswordTheme } from 'src/app/core/models/theme';

@UntilDestroy()
@Component({
  selector: 'app-crossword-add',
  templateUrl: './crossword-add.component.html',
  styleUrls: ['./crossword-add.component.scss'],
})
export class CrosswordAddComponent implements OnInit {
  @Input() themes: CrosswordTheme[] = [];
  @Input() dictionaries: Dictionary[] = [];
  @Input() crossword: Crossword | null = null;

  form = this.formBuilder.group({
    name: ['', [Validators.required, Validators.maxLength(30)]],
    themeId: [-1, [Validators.required]],
    dictionaryId: [-1, [Validators.required]],
    size: this.formBuilder.group({
      width: [
        10,
        [Validators.required, Validators.min(10), Validators.max(20)],
      ],
      height: [
        10,
        [Validators.required, Validators.min(10), Validators.max(20)],
      ],
    }),
    promptCount: [0, [Validators.required]],
  });

  maxPromptCount = 0;

  constructor(private formBuilder: FormBuilder, private modalRef: NzModalRef) {
    this.form.statusChanges.pipe(untilDestroyed(this)).subscribe((status) => {
      const isValid = status === 'VALID';
      this.modalRef.updateConfig({ nzOkDisabled: !isValid });
    });
  }

  ngOnInit(): void {
    if (!this.crossword) return;

    this.form.patchValue(this.crossword);

    const letterCount = this.crossword.words.reduce(
      (accumulator, value) => accumulator + value.name.length,
      0,
    );
    this.maxPromptCount = Math.round(letterCount * 0.1);
  }
}
