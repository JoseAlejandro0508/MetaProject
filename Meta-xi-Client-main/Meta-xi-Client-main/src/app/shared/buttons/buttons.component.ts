import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive } from '@angular/router';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-buttons',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './buttons.component.html',
  styleUrl: './buttons.component.scss',
})
export class ButtonsComponent implements OnInit, OnDestroy {
  isLogined = false;
  currentUrl = '';

  private router = inject(Router);
  private routerSub!: Subscription;

  ngOnInit(): void {
    // Set initial active state
    this.currentUrl = this.router.url;

    // Track route changes for active state
    this.routerSub = this.router.events.subscribe((event) => {
      if (event instanceof NavigationEnd) {
        this.currentUrl = event.urlAfterRedirects;
        this.isLogined = this.currentUrl === '/login';
      }
    });
  }

  ngOnDestroy(): void {
    if (this.routerSub) {
      this.routerSub.unsubscribe();
    }
  }

  isActive(path: string): boolean {
    return this.currentUrl === path;
  }
}