import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Header } from "../header/header";

@Component({
  selector: 'th-layout',
  imports: [CommonModule, RouterOutlet, Header],
  templateUrl: './layout.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LayoutComponent {

}
