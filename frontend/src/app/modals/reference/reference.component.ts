import { Component } from '@angular/core';

@Component({
  selector: 'app-reference',
  templateUrl: './reference.component.html',
  styleUrls: ['./reference.component.scss'],
})
export class ReferenceComponent {
  openAboutSystem() {
    window.open(`http://localhost:5062/about-system`, '_blank');
  }
}
