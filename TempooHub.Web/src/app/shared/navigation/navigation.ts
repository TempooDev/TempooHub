import { Component } from '@angular/core';
import { ROUTES } from '../constants/navigation-routes';
import { MenuModule } from 'primeng/menu';
import { BadgeModule } from 'primeng/badge';
import { RippleModule } from 'primeng/ripple';
import { AvatarModule } from 'primeng/avatar';

@Component({
  selector: 'th-navigation',
  imports: [MenuModule, BadgeModule, RippleModule, AvatarModule],
  templateUrl: './navigation.html',
  styles: ``,
})
export class Navigation {

  routes = ROUTES;
}
