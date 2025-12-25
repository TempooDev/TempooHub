import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ROUTES } from '../constants/navigation-routes';
import { MenuModule } from 'primeng/menu';
import { BadgeModule } from 'primeng/badge';
import { RippleModule } from 'primeng/ripple';
import { AvatarModule } from 'primeng/avatar';
import { MenuItem } from 'primeng/api';
import { HasClaimPipe } from '../pipes/has-role.pipe';

@Component({
  selector: 'th-navigation',
  imports: [CommonModule, RouterModule, MenuModule, BadgeModule, RippleModule, AvatarModule],
  templateUrl: './navigation.html',
  styles: ``,
  standalone: true,
  providers: [HasClaimPipe]
})
export class Navigation implements OnInit {
  menuItems: MenuItem[] = [];
  private hasClaimPipe = inject(HasClaimPipe);

  ngOnInit(): void {
    this.menuItems = this.processRoutes(ROUTES);
  }

  private processRoutes(routes: MenuItem[]): MenuItem[] {
    return routes.map(item => {
      const newItem = { ...item };
      const claimType = item['data']?.['claimType'];
      const claimValue = item['data']?.['claimValue'];

      if (claimType) {
        const hasAccess = this.hasClaimPipe.transform(claimType, claimValue);
        if (!hasAccess) {
          newItem.disabled = true;
        }
      }

      if (item.items) {
        newItem.items = this.processRoutes(item.items);
      }
      return newItem;
    });
  }
}


