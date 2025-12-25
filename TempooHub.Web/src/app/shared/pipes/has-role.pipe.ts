import { inject, Pipe, PipeTransform } from '@angular/core';
import { AuthService } from '../auth.service';

@Pipe({
  name: 'hasRole',
  standalone: true,
})
export class HasRolePipe implements PipeTransform {
  private authService = inject(AuthService);

  transform(role: string | undefined): boolean {
    if (!role) {
      return true;
    }
    const user = this.authService.currentUser();
    if (!user) {
      return false;
    }
    const userRoles: string[] = user.roles || [];
    return userRoles.includes(role);
  }
}
