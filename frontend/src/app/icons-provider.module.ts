import { NgModule } from '@angular/core';
import { NZ_ICONS, NzIconModule } from 'ng-zorro-antd/icon';

import {
  MenuFoldOutline,
  MenuUnfoldOutline,
  FormOutline,
  DashboardOutline,
  AppstoreOutline,
  QuestionCircleOutline,
  LogoutOutline,
  FileAddOutline,
  EditOutline,
  DeleteOutline,
  UploadOutline,
} from '@ant-design/icons-angular/icons';

const icons = [
  MenuFoldOutline,
  MenuUnfoldOutline,
  DashboardOutline,
  FormOutline,
  AppstoreOutline,
  QuestionCircleOutline,
  LogoutOutline,
  FileAddOutline,
  EditOutline,
  DeleteOutline,
  UploadOutline,
];

@NgModule({
  imports: [NzIconModule],
  exports: [NzIconModule],
  providers: [{ provide: NZ_ICONS, useValue: icons }],
})
export class IconsProviderModule {}
