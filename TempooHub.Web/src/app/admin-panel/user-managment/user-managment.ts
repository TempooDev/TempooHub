import { HttpClient } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { ResultObject } from '../../dashboard-page/dashboard-page';

@Component({
  selector: 'th-user-managment',
  imports: [],
  templateUrl: './user-managment.html',
  styles: ``,
})
export class UserManagment {
  http = inject(HttpClient);
  apiStatus = signal('Loading...');

  constructor() {
    this.loadApiStatus();
  }

  loadApiStatus() {
    this.http.get<ResultObject>('/api/user').subscribe({
      next: result => { this.apiStatus.set(result.message); },
      error: err => { this.apiStatus.set(`Error: ${err.message}`); }
    });
  }
}
