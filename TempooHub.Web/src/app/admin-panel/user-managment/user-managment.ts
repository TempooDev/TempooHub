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
  checkAdminRole = signal('Checking...');

  constructor() {
    this.loadApiStatus();
    this.loadAdminRole();
  }

  loadAdminRole() {
    this.http.get<ResultObject>('/api/v1/admin').subscribe({
      next: result => { this.checkAdminRole.set(result.message); },
      error: err => { this.checkAdminRole.set(`Error: ${err.message}`); }
    });
  }

  loadApiStatus() {
    this.http.get<ResultObject>('/api/v1/user').subscribe({
      next: result => { this.apiStatus.set(result.message); },
      error: err => { this.apiStatus.set(`Error: ${err.message}`); }
    });
  }
}
