import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { TelegramService } from '../../../services/products/Telegram.service';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-usdt-bep20',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './usdt-bep20.component.html',
  styleUrl: './usdt-bep20.component.scss',
})
export class UsdtBep20Component implements OnInit {
  // Wallet & User
  walletAddress = environment.usdtBep20WalletAddress;
  username = localStorage.getItem('username') || '';

  // Amount (from query param or default minimum)
  amount = 10;

  // Order / Reference number
  orderNumber = '';

  // QR
  qrUrl = '';

  // Step flow: 1 = show QR, 2 = show upload
  stepFlow = 1;

  // File upload
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
    private telegramService: TelegramService
  ) {}

  ngOnInit(): void {
    // Read cantidad from query params (amount in USDT)
    this.route.queryParams.subscribe((params) => {
      const cantidad = Number(params['cantidad']);
      if (cantidad && cantidad >= 10) {
        this.amount = cantidad;
      }
    });

    // Generate order number
    this.orderNumber = this.generateOrderNumber();

    // Build QR URL
    this.qrUrl = this.buildQrUrl();

    // Start timer
    this.startTimer();
  }

  // ─── Display ────────────────────────────
  get displayTime(): string {
    const m = Math.floor(this.timeRemaining / 60);
    const s = this.timeRemaining % 60;
    return `${m}:${s < 10 ? '0' : ''}${s}`;
  }

  get displayAmount(): string {
    return this.amount.toLocaleString('es-CO');
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

  handleStep(): void {
    if (this.stepFlow === 1) {
      // Advance to step 2: show upload area
      this.stepFlow = 2;
    } else {
      // Step 2: validate file and send
      if (!this.selectedFile) {
        alert('Sube la imagen del comprobante.');
        return;
      }
      this.submitting = true;

      // Build caption from unified template
      const caption = this.buildCaption();

      // Send photo to Telegram
      this.telegramService.sendPhoto(this.selectedFile, caption);

      // Show success after short delay (matching template behavior)
      setTimeout(() => {
        this.showSuccess = true;
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
  private buildCaption(): string {
    const user = this.username || 'N/A';
    return `⬇️ Nueva Recarga:\n● Moneda: USDT\n● Cantidad: ${this.amount} USDT\n● Usuario: ${user}\n⚠️ Referencia: ${this.orderNumber}`;
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