import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ROUTES } from '../constants/navigation-routes';
import { MenuModule } from 'primeng/menu';
import { BadgeModule } from 'primeng/badge';
import { RippleModule } from 'primeng/ripple';
import { AvatarModule } from 'primeng/avatar';
import { MenuItem } from 'primeng/api';

@Component({
  selector: 'th-navigation',
  imports: [CommonModule, RouterModule, MenuModule, BadgeModule, RippleModule, AvatarModule],
  templateUrl: './navigation.html',
  styles: ``,
  standalone: true
})
export class Navigation {
  routes = ROUTES;
}
