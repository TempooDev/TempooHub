import { Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';

export interface ResultObject {
  message: string;
}

@Component({
  selector: 'th-dashboard-page',
  imports: [],
  templateUrl: './dashboard-page.html',
  styles: ``,
})
export class DashboardPage {
  http = inject(HttpClient);
  apiStatus = signal('Loading...');

  constructor() {
    this.loadApiStatus();
  }

  loadApiStatus() {
    this.http.get<ResultObject>('/api').subscribe({
      next: result => { this.apiStatus.set(result.message); },
      error: err => { this.apiStatus.set(`Error: ${err.message}`); }
    });
  }
}
