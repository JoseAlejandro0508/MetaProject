import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { NotificationService } from '../../services/products/notification.service';

@Component({
  selector: 'app-welcome-bonus',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './welcome-bonus.component.html',
  styleUrl: './welcome-bonus.component.scss',
})
export class WelcomeBonusComponent implements OnInit {
  private http = inject(HttpClient);
  private router = inject(Router);
  private notificationService = inject(NotificationService);

  username: string = localStorage.getItem('username') || '';
  showNotification = false;
  isClaiming = false;

  async ngOnInit(): Promise<void> {
    // Verify user is authenticated
    if (!this.username) {
      this.router.navigate(['/login']);
      return;
    }

    // Check if already claimed — if yes, redirect to home
    try {
      const url = `${environment.apiUrl}/Wallet/CheckWelcomeBonus/${this.username}`;
      const response: any = await firstValueFrom(this.http.get(url));
      if (response && response.claimed === true) {
        this.router.navigate(['/home']);
      }
    } catch (error) {
      console.error('Error checking welcome bonus:', error);
    }
  }

  async claimBonus(): Promise<void> {
    if (this.isClaiming) return;
    this.isClaiming = true;

    const url = `${environment.apiUrl}/Wallet/ClaimWelcomeBonus`;
    const data = { email: this.username };

    try {
      const response: any = await firstValueFrom(this.http.post(url, data));
      
      // Show floating notification
      this.showNotification = true;
      setTimeout(() => {
        this.showNotification = false;
      }, 4000);

      // Mark as claimed in localStorage
      localStorage.setItem('welcomeBonusClaimed', 'true');

      // Navigate to home after a brief delay
      setTimeout(() => {
        this.router.navigate(['/home']);
      }, 2000);
    } catch (error: any) {
      console.error('Error claiming bonus:', error);
      this.notificationService.errorMessage('Error al reclamar bono');
    } finally {
      this.isClaiming = false;
    }
  }
}
