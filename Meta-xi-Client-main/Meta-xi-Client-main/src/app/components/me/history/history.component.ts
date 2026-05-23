import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../../environments/environment';

interface TransactionHistoryDTO {
  id: number;
  type: string;
  title: string;
  amount: number;
  signedAmount: string;
  currency: string;
  date: string;
  status: string;
  fee: number | null;
  netAmount: number | null;
}

@Component({
  selector: 'app-history',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './history.component.html',
  styleUrl: './history.component.scss',
})
export class HistoryComponent implements OnInit {
  private http = inject(HttpClient);
  private router = inject(Router);

  transactions: TransactionHistoryDTO[] = [];
  filteredTransactions: TransactionHistoryDTO[] = [];
  activeTab: 'all' | 'deposit' | 'withdraw' = 'all';
  loading = true;
  username: string = localStorage.getItem('username') || '';

  async ngOnInit(): Promise<void> {
    await this.loadHistory();
  }

  async loadHistory(): Promise<void> {
    const url = `${environment.apiUrl}/Wallet/History/${this.username}`;
    try {
      const response: any = await firstValueFrom(this.http.get(url));
      this.transactions = response || [];
      this.filterByTab(this.activeTab);
    } catch (error: any) {
      console.error('Error al cargar historial:', error);
      this.transactions = [];
      this.filteredTransactions = [];
    } finally {
      this.loading = false;
    }
  }

  filterByTab(tab: 'all' | 'deposit' | 'withdraw'): void {
    this.activeTab = tab;
    if (tab === 'all') {
      this.filteredTransactions = [...this.transactions];
    } else {
      this.filteredTransactions = this.transactions.filter((t) => t.type === tab);
    }
  }

  goBack(): void {
    this.router.navigate(['/me']);
  }

  getCardClass(type: string, status: string): string {
    if (type === 'deposit') {
      return status === 'En Proceso' ? 'type-pending' : 'type-deposit';
    }
    return 'type-withdraw';
  }

  getIconClass(type: string, status: string): string {
    if (type === 'deposit') {
      return status === 'En Proceso' ? 'icon-pending' : 'icon-nequi';
    }
    return 'icon-withdraw';
  }
}
