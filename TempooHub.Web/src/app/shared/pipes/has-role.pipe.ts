import { inject, Pipe, PipeTransform } from '@angular/core';
import { AuthService } from '../auth.service';

@Pipe({
  name: 'hasClaim',
  standalone: true,
})
export class HasClaimPipe implements PipeTransform {
  private authService = inject(AuthService);

  transform(claimType: string, claimValue?: string): boolean {
    if (!claimType) {
      return true;
    }
    const user = this.authService.currentUser();
    if (!user || !user.claims) {
      return false;
    }

    return user.claims.some(claim => {
      if (claim.type === claimType) {
        return claimValue ? claim.value === claimValue : true;
      }
      return false;
    });
  }
}
