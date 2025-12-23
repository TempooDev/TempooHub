import { JsonPipe } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterModule } from '@angular/router';

export interface UserData {
  message: string;
  user?: string | null;
}

@Component({
  selector: 'th-admin-panel',
  imports: [RouterModule, JsonPipe],
  templateUrl: './admin-panel.html',
  styles: ``,
})
export class AdminPanelPage {
  http = inject(HttpClient);

  userData = signal<UserData | null>(null);

  constructor() {
    this.loadApiStatus();
  }

  loadApiStatus() {
    this.http.get<UserData>('/api/v1/data').subscribe({
      next: result => { this.userData.set(result); },
      error: err => { this.userData.set({ message: `Error: ${err.message}` }); }
    });
  }
}
