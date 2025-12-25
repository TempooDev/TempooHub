import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ROUTES } from '../constants/navigation-routes';
import { MenuModule } from 'primeng/menu';
import { BadgeModule } from 'primeng/badge';
import { RippleModule } from 'primeng/ripple';
import { AvatarModule } from 'primeng/avatar';
import { MenuItem } from 'primeng/api';
import { HasRolePipe } from '../pipes/has-role.pipe';

@Component({
  selector: 'th-navigation',
  imports: [CommonModule, RouterModule, MenuModule, BadgeModule, RippleModule, AvatarModule],
  templateUrl: './navigation.html',
  styles: ``,
  standalone: true,
  providers: [HasRolePipe]
})
export class Navigation implements OnInit {
  menuItems: MenuItem[] = [];
  private hasRolePipe = inject(HasRolePipe);

  ngOnInit(): void {
    this.menuItems = this.processRoutes(ROUTES);
  }

  private processRoutes(routes: MenuItem[]): MenuItem[] {
    return routes.map(item => {
      const newItem = { ...item };
      const hasAccess = this.hasRolePipe.transform(item['data']?.['role']);
      
      if (!hasAccess) {
        newItem.disabled = true;
      }

      if (item.items) {
        newItem.items = this.processRoutes(item.items);
      }
      return newItem;
    });
  }
}


