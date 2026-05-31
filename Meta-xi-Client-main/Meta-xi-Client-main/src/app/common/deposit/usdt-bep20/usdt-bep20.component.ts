import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { TelegramService } from '../../../services/products/Telegram.service';
import { environment } from '../../../../environments/environment';

interface PresetAmount {
  value: number;
  label: string;
}

@Component({
  selector: 'app-usdt-bep20',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './usdt-bep20.component.html',
  styleUrl: './usdt-bep20.component.scss',
})
export class UsdtBep20Component implements OnInit {
  readonly MIN_AMOUNT = 10;
  readonly CONVERSION_RATE = 3600; // USDT to COP

  // Wallet & User
  walletAddress = environment.usdtBep20WalletAddress;
  username = localStorage.getItem('username') || '';
  saldo = 0;

  // Amount selection
  rawAmount = 0;
  displayAmount = '';

  presets: PresetAmount[] = [
    { value: 15, label: '15' },
    { value: 30, label: '30' },
    { value: 80, label: '80' },
    { value: 120, label: '120' },
    { value: 200, label: '200' },
    { value: 400, label: '400' },
  ];

  // Flow: 'amount' = selector, 'payment' = QR screen
  flowStep: 'amount' | 'payment' = 'amount';

  // Payment screen data
  amount = 10;
  orderNumber = '';
  qrUrl = '';
  stepFlow = 1;
  selectedFile: File | null = null;
  fileMsg = '📸 Click para subir comprobante';

  // Timer (20 minutes = 1200 seconds)
  private readonly TIMER_SECONDS = 20 * 60;
  private timerInterval: ReturnType<typeof setInterval> | null = null;
  timeRemaining = this.TIMER_SECONDS;

  // States
  showExpired = false;
  showSuccess = false;
  submitting = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private telegramService: TelegramService,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    this.getSaldo();

    // Check if amount is provided in query params
    this.route.queryParams.subscribe((params) => {
      const cantidad = Number(params['cantidad']);
      if (cantidad && cantidad >= this.MIN_AMOUNT) {
        this.amount = cantidad;
        this.rawAmount = cantidad;
        this.displayAmount = cantidad.toString();
        this.flowStep = 'payment';
        this.initPayment();
      }
    });
  }

  // ─── Amount Selection ───────────────────
  get displayBalance(): string {
    return this.saldo.toLocaleString('es-CO');
  }

  onAmountInput(value: string): void {
    const digits = value.replace(/\D/g, '');
    this.rawAmount = digits ? parseInt(digits, 10) : 0;
    this.displayAmount = digits !== '' ? digits : '';
  }

  selectAmount(num: number): void {
    this.rawAmount = num;
    this.displayAmount = num.toString();
  }

  confirmAmount(): void {
    if (this.rawAmount < this.MIN_AMOUNT) return;
    // Navigate to same route with amount as query param
    this.router.navigate(['/deposit', 'usdt-bep20'], {
      queryParams: { cantidad: this.rawAmount },
    });
  }

  // ─── Payment Screen ─────────────────────
  private initPayment(): void {
    this.orderNumber = this.generateOrderNumber();
    this.qrUrl = this.buildQrUrl();
    this.startTimer();
  }

  get displayTime(): string {
    const m = Math.floor(this.timeRemaining / 60);
    const s = this.timeRemaining % 60;
    return `${m}:${s < 10 ? '0' : ''}${s}`;
  }

  get displayPaymentAmount(): string {
    return this.amount.toString();
  }

  // ─── Actions ──────────────────────────────
  copyAddress(btn: HTMLElement): void {
    navigator.clipboard.writeText(this.walletAddress).then(() => {
      btn.textContent = '¡OK!';
      setTimeout(() => {
        btn.textContent = 'COPIAR';
      }, 2000);
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];
      this.fileMsg = '✅ Captura Lista';
    }
  }

  /*handleStep(): void {
    if (this.stepFlow === 1) {
      this.stepFlow = 2;
    } else {
      if (!this.selectedFile) {
        alert('Sube la imagen del comprobante.');
        return;
      }
      this.submitting = true;

      const caption = this.buildCaption();
      this.telegramService.sendPhoto(this.selectedFile, caption);

      setTimeout(() => {
        //this.showSuccess = true;
        this.submitting = false;
        this.stopTimer();
      }, 1200);
    }
  }*/
  async handleStep(): Promise<void> {
    if (this.stepFlow === 1) {
      this.stepFlow = 2;
    } else {
      if (!this.selectedFile) {
        alert('Sube la imagen del comprobante.');
        return;
      }

      this.submitting = true;

      const caption = this.buildCaption();
      const updateBalancePayload = {
        OrdenId: this.orderNumber,
        Email: this.username,
        Balance: this.amount,
        Token: 'usdt_bep20',
      };
      try {
        await firstValueFrom(
          this.http.post(
            `${environment.apiUrl}/Wallet/UpdateBalance`,
            updateBalancePayload
          )
        );

      
        this.telegramService.sendPhoto(this.selectedFile, caption);

        this.showSuccess = true;
      } catch {
        alert('Error al procesar tu recarga. Inténtalo nuevamente.');
      } finally {
        this.submitting = false;
      }

  

      setTimeout(() => {
        //this.showSuccess = true;
        this.submitting = false;
        this.stopTimer();
      }, 1200);
    }
  }

  handleQrError(event: Event): void {
    const img = event.target as HTMLImageElement;
    if (img) {
      img.src = 'assets/token/usdt-bep20.jpg';
    }
  }

  // ─── Private ──────────────────────────────
  private async getSaldo(): Promise<void> {
    const url = `${environment.apiUrl}/Wallet/GetBalance/${this.username}?coin=COP`;
    try {
      const response: any = await firstValueFrom(this.http.get(url));
      this.saldo = Number(response) || 0;
    } catch (error: any) {
      console.error('Error al obtener balance:', error);
    }
  }

  private buildCaption(): string {
    const user = this.username || 'N/A';
    return `⬇️ Nueva Recarga:\n● Moneda: USDT BEP-20\n● Cantidad: ${this.amount} USDT\n● Usuario: ${user}\n⚠️ Referencia: ${this.orderNumber}`;
  }

  private buildQrUrl(): string {
    const data = encodeURIComponent(this.walletAddress);
    return `https://api.qrserver.com/v1/create-qr-code/?size=140x140&data=${data}`;
  }

  private generateOrderNumber(): string {
    const timestamp = Date.now().toString().slice(-6);
    const random = Math.floor(Math.random() * 10000)
      .toString()
      .padStart(4, '0');
    return `USDT${timestamp}${random}`;
  }

  private startTimer(): void {
    this.timerInterval = setInterval(() => {
      this.timeRemaining--;
      if (this.timeRemaining <= 0) {
        this.stopTimer();
        this.showExpired = true;
      }
    }, 1000);
  }

  private stopTimer(): void {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
      this.timerInterval = null;
    }
  }
}
