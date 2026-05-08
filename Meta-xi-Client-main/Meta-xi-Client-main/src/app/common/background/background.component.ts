import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavComponent } from '../../shared/nav/nav.component';
import { ButtonsComponent } from '../../shared/buttons/buttons.component';
import { TopNotificationComponent } from '../../shared/top-notification/top-notification.component';

@Component({
  selector: 'app-background',
  standalone: true,
  imports: [RouterOutlet, NavComponent, ButtonsComponent, TopNotificationComponent],
  templateUrl: './background.component.html',
  styleUrl: './background.component.scss',
})
export class BackgroundComponent {}
