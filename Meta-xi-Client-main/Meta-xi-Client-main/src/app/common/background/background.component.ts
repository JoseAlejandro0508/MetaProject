import { Component, OnInit } from '@angular/core';
import { Router, RouterOutlet, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { filter } from 'rxjs/operators';
import { NavComponent } from '../../shared/nav/nav.component';
import { ButtonsComponent } from '../../shared/buttons/buttons.component';
import { TopNotificationComponent } from '../../shared/top-notification/top-notification.component';

@Component({
  selector: 'app-background',
  standalone: true,
  imports: [RouterOutlet, NavComponent, ButtonsComponent, TopNotificationComponent, CommonModule],
  templateUrl: './background.component.html',
  styleUrl: './background.component.scss',
})
export class BackgroundComponent implements OnInit {
  hideNavAndButtons = false;
  noPadding = false;

  private readonly hideRoutes = [
    'withdraw/nequi',
    'withdraw/usdt-bep20',
    'nequi/nequi',
    'deposit/usdt-bep20',
    'deposit/breb',
    'breb/breb',
    'deposit/nequi',
    'welcome',
    "login?"
  ];
    private readonly nonPadding = [

    'breb/breb',

  ];

  constructor(private router: Router) {}

  ngOnInit(): void {
    this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe((event) => {
        const url = event.urlAfterRedirects || event.url;
        this.hideNavAndButtons = this.hideRoutes.some((route) =>
          url.includes(route)
        );
        this.noPadding = this.nonPadding.some((route) =>
          url.includes(route)
        );
      });
  }
}
