import { Component, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService, NotificationData } from '../../services/products/notification.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-top-notification',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './top-notification.component.html',
  styleUrl: './top-notification.component.scss',
})
export class TopNotificationComponent implements OnDestroy {
  visible = false;
  data: NotificationData | null = null;

  private sub: Subscription;

  constructor(private notificationService: NotificationService) {
    this.sub = this.notificationService.notification.subscribe((notif) => {
      if (notif) {
        this.data = notif;
        // Small delay to ensure DOM has the element before adding .show class
        // so the CSS transition fires properly
        setTimeout(() => {
          this.visible = true;
        }, 10);
      } else {
        this.visible = false;
      }
    });
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }

  dismiss(): void {
    this.visible = false;
    this.notificationService.dismiss();
  }

  get statusLabel(): string {
    return this.data?.type === 'success' ? 'Éxito' : 'Denegado';
  }

  get icon(): string {
    return this.data?.type === 'success' ? '✔' : '✕';
  }
}