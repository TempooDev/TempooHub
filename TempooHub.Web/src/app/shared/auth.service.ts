import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { catchError, of, tap } from 'rxjs';

export interface UserProfile {
  email: string;
  roles: string[];
  claims: { type: string, value: string }[];
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private readonly API_URL = '/api/auth';

  currentUser = signal<UserProfile | null>(null);

  // Método para verificar sesión al cargar la app
  checkStatus() {
    return this.http.get<UserProfile>(`${this.API_URL}/manage/user-details`).pipe(
      tap(user => this.currentUser.set(user)),
      catchError(() => {
        this.currentUser.set(null);
        return of(null);
      })
    );
  }

  login(email: string, password: string) {
    return this.http.post<any>(`${this.API_URL}/auth/login`, { email, password }).pipe(
      tap(response => {
        if (response && response.accessToken) {
          localStorage.setItem('token', response.accessToken);
          this.checkStatus().subscribe();
        }
      })
    );
  }

  logout() {
    return this.http.post(`${this.API_URL}/logout`, {}).pipe(
      tap(() => {
        localStorage.removeItem('token');
        this.router.navigate(['/login']);
      })
    );
  }
}