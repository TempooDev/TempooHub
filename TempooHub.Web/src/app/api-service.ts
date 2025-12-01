import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ApiService {
  private http = inject(HttpClient);

  getApiStatus(): string {
    let result = '';
    this.http.get<string>('/api').subscribe({
      next: result => { result = result; },
      error: err => { result = `Error: ${err.message}`; }
    });
    return result;
  }
}
