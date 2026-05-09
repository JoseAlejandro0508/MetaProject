import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../../environments/environment';

interface AccountSummaryDTO {
  totalEarned: number;
  totalInvested: string;
  totalRecharged: number;
  totalWithdrawn: number;
  taskEarnings: number;
  planEarnings: number;
  referralEarnings: number;
  accountStatus: string;
}

@Component({
  selector: 'app-account',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './account.component.html',
  styleUrl: './account.component.scss',
})
export class AccountComponent {
  username: string = localStorage.getItem('username') || '';

  totalEarned = 0;
  totalInvested = 'N/A';
  totalRecharged = 0;
  totalWithdrawn = 0;
  taskEarnings = 0;
  planEarnings = 0;
  referralEarnings = 0;
  accountStatus = 'VERIFICADA';

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.getAccountSummary();
  }

  async getAccountSummary(): Promise<void> {
    const url = `${environment.apiUrl}/Wallet/GetAccountSummary/${this.username}`;
    try {
      const response: any = await firstValueFrom(this.http.get(url));
      this.totalEarned = response.totalEarned ?? 0;
      this.totalInvested = response.totalInvested ?? 'N/A';
      this.totalRecharged = response.totalRecharged ?? 0;
      this.totalWithdrawn = response.totalWithdrawn ?? 0;
      this.taskEarnings = response.taskEarnings ?? 0;
      this.planEarnings = response.planEarnings ?? 0;
      this.referralEarnings = response.referralEarnings ?? 0;
      this.accountStatus = response.accountStatus ?? 'VERIFICADA';
    } catch (error: any) {
      console.error('Error al cargar resumen de cuenta:', error);
    }
  }

  formatCurrency(value: number): string {
    return value.toLocaleString('es-CO', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + ' COP';
  }
}