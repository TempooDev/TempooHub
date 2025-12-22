import { Component, computed, effect, inject, signal, Signal } from '@angular/core';
import { AvatarModule } from 'primeng/avatar';
import { BadgeModule } from 'primeng/badge';
import { OverlayBadgeModule } from "primeng/overlaybadge";
import { AuthService } from '../auth.service';
import { Router } from '@angular/router';
import { TitleCasePipe } from '@angular/common';

@Component({
  selector: 'th-header',
  imports: [AvatarModule, BadgeModule, OverlayBadgeModule, TitleCasePipe],
  templateUrl: './header.html',
})
export class HeaderComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  user = this.authService.currentUser;

  authenticated = computed(() => !!this.user());

  login() {
    this.router.navigate(['/login']);
  }

  logout() {
    this.authService.logout().subscribe({
      next: () => this.router.navigate(['/login']),
    });
  }
}
