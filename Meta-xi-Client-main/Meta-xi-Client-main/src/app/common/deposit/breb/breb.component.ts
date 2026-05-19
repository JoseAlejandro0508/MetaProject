import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../../environments/environment';

interface PresetAmount {
  value: number;
  label: string;
}

@Component({
  selector: 'app-breb',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './breb.component.html',
  styleUrl: './breb.component.scss',
})
export class BrebComponent implements OnInit {
  readonly MIN_AMOUNT = 50000;

  saldo = 0;
  username: string = localStorage.getItem('username') || '';

  rawAmount = 0;
  displayAmount = '';

  presets: PresetAmount[] = [
    { value: 50000, label: '50,000' },
    { value: 120000, label: '120,000' },
    { value: 250000, label: '250,000' },
    { value: 500000, label: '500,000' },
    { value: 1000000, label: '1,000,000' },
    { value: 1500000, label: '1,500,000' },
  ];

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.getSaldo();
  }

  get displayBalance(): string {
    return this.saldo.toLocaleString('es-CO');
  }

  private async getSaldo(): Promise<void> {
    const url = `${environment.apiUrl}/Wallet/GetBalance/${this.username}`;
    try {
      const response: any = await firstValueFrom(this.http.get(url));
      this.saldo = Number(response) || 0;
    } catch (error: any) {
      console.error('Error al obtener balance:', error);
    }
  }

  onAmountInput(value: string): void {
    const digits = value.replace(/\D/g, '');
    this.rawAmount = digits ? parseInt(digits, 10) : 0;
    this.displayAmount = digits !== '' ? Number(digits).toLocaleString('en-US') : '';
  }

  selectAmount(num: number): void {
    this.rawAmount = num;
    this.displayAmount = num.toLocaleString('en-US');
  }
}