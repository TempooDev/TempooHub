import { Component, effect, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../shared/auth.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'th-login-component',
  imports: [FormsModule],
  templateUrl: './login-component.html',
})
export class LoginComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  // Signals para manejar el estado de la UI
  email = signal('');
  password = signal('');
  errorMessage = signal<string | null>(null);
  isLoading = signal(false);

  onSubmit() {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.authService.login(this.email(), this.password()).subscribe({
      next: () => {
        // Al ser exitoso, el AuthService ya actualizó la signal currentUser
        this.authService.checkStatus().subscribe(() => {
        this.router.navigate(['/']); 
      });
      },
      error: (err) => {
        this.isLoading.set(false);
        // Manejo de errores de .NET Identity
        this.errorMessage.set('Credenciales incorrectas o servidor no disponible');
        console.error('Login error:', err);
      }
    });
  }
}
