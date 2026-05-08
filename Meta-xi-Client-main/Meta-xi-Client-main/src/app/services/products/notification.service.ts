import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface NotificationData {
  message: string;
  type: 'success' | 'error';
}

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  private notification$ = new BehaviorSubject<NotificationData | null>(null);
  private timeoutId: ReturnType<typeof setTimeout> | null = null;

  /** Observable for the notification component to subscribe to */
  notification = this.notification$.asObservable();

  correct(message: string): void {
    this.show(message, 'success');
  }

  errorMessage(message: string): void {
    this.show(message, 'error');
  }

  showCustomMessage(message: string, type: 'success' | 'error' = 'success'): void {
    this.show(message, type);
  }

  private show(message: string, type: 'success' | 'error'): void {
    // Clear any existing timeout so it resets the timer
    if (this.timeoutId) {
      clearTimeout(this.timeoutId);
    }
    this.notification$.next({ message, type });

    // Auto-hide after 5 seconds
    this.timeoutId = setTimeout(() => {
      this.notification$.next(null);
      this.timeoutId = null;
    }, 5000);
  }

  /** Called by the component when user taps to dismiss */
  dismiss(): void {
    if (this.timeoutId) {
      clearTimeout(this.timeoutId);
      this.timeoutId = null;
    }
    this.notification$.next(null);
  }
}