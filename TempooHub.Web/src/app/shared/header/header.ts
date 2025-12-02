import { Component } from '@angular/core';
import { Navigation } from "../navigation/navigation";
import { AvatarModule } from 'primeng/avatar';
import { BadgeDirective, BadgeModule } from "primeng/badge";
import { OverlayBadgeModule } from "primeng/overlaybadge";

@Component({
  selector: 'th-header',
  imports: [AvatarModule, BadgeModule, OverlayBadgeModule],
  templateUrl: './header.html',
  styles: ``,
})
export class Header {

}
