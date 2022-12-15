import { Component, Input } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { NzModalRef } from 'ng-zorro-antd/modal';
import { CrosswordTheme } from 'src/app/core/models/theme';

@UntilDestroy()
@Component({
  selector: 'app-theme-add',
  templateUrl: './theme-add.component.html',
  styleUrls: ['./theme-add.component.scss'],
})
export class ThemeAddComponent {
  @Input() set theme(value: CrosswordTheme) {
    if (!value) return;
    this.form.setValue({
      name: value.name,
    });
  }

  form = this.formBuilder.group({
    name: ['', [Validators.required]],
  });

  constructor(private formBuilder: FormBuilder, private modalRef: NzModalRef) {
    this.form.statusChanges.pipe(untilDestroyed(this)).subscribe((status) => {
      const isValid = status === 'VALID';
      this.modalRef.updateConfig({ nzOkDisabled: !isValid });
    });
  }
}
