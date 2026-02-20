import { Component } from '@angular/core';

export interface AboutLibrary {
  name: string;
  version: string;
}

@Component({
  selector: 'app-about-libraries',
  standalone: true,
  imports: [],
  templateUrl: './about-libraries.component.html',
})
export class AboutLibrariesComponent {
  libraries: AboutLibrary[] = [
    { name: '@angular/core', version: '^21.1.0' },
    { name: '@angular/common', version: '^21.1.0' },
    { name: '@angular/router', version: '^21.1.0' },
    { name: '@angular/forms', version: '^21.1.0' },
    { name: 'bootstrap', version: '^5.3.8' },
    { name: 'rxjs', version: '~7.8.0' },
    { name: 'tslib', version: '^2.3.0' },
  ];
}
