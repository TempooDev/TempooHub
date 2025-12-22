import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from "../header/header";
import { Navigation } from "../navigation/navigation";

@Component({
  selector: 'th-layout',
  imports: [CommonModule, RouterOutlet, HeaderComponent, Navigation],
  templateUrl: './layout.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LayoutComponent {

}
