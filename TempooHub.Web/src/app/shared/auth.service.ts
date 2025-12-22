import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { catchError, of, tap } from 'rxjs';

export interface UserProfile {
  email: string;
  roles: string[];
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private readonly API_URL = '/api';

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
    return this.http.post(`${this.API_URL}/login?useCookies=true`, { email, password }).pipe(
      tap(() => {
        // Tras el login, actualizamos la signal de usuario
        this.checkStatus().subscribe();
      })
    );
  }

  logout() {
    return this.http.post(`${this.API_URL}/logout`, {}).pipe(
      tap(() => {
        this.currentUser.set(null);
        this.router.navigate(['/login']);
      })
    );
  }
}