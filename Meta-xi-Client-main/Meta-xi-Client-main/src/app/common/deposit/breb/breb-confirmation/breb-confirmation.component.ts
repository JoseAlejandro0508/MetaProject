import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { TelegramService } from '../../../../services/products/Telegram.service';
import { NotificationService } from '../../../../services/products/notification.service';
import { RouterLink } from '@angular/router';
@Component({
  selector: 'app-breb-confirmation',
  standalone: true,
  imports: [CommonModule, FormsModule,RouterLink],
  templateUrl: './breb-confirmation.component.html',
  styleUrl: './breb-confirmation.component.scss'

})
export class BrebConfirmationComponent implements OnInit {
  // Amount from query params (set by BrebComponent)
  montoRecarga = 0;
  // Unique order number with BRE prefix
  orderNumber = '';
  // User input: transaction reference
  reference = '';
  // Toggle between payment view and success view
  showSuccess = false;
  // QR data URL
  qrUrl = '';
  // Loading state during submission
  submitting = false;
  // Username from localStorage
  username = '';
  // Bre-b account number
  brebAccount = '94520204';
  // Timer (20 minutes)
  private readonly TIMER_SECONDS = 20 * 60;
  private timerInterval: ReturnType<typeof setInterval> | null = null;
  timeRemaining = this.TIMER_SECONDS;
  showExpired = false;

  constructor(
    private route: ActivatedRoute,
    private telegramService: TelegramService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    // Read cantidad from query params
    this.route.queryParams.subscribe((params) => {
      this.montoRecarga = Number(params['cantidad']) || 0;
    });

    // Generate a unique order number with BRE prefix
    this.orderNumber = this.generateOrderNumber();

    // Generate QR with payment info
    this.qrUrl = this.buildQrUrl();

    // Read username from localStorage
    this.username = localStorage.getItem('username') || '';

    // Start 20-minute timer
    this.startTimer();
  }

  get displayAmount(): string {
    return '$ ' + this.montoRecarga.toLocaleString('es-CO');
  }

  get canConfirm(): boolean {
    return this.reference.trim().length > 0 && !this.submitting && !this.showExpired;
  }

  get displayTime(): string {
    const m = Math.floor(this.timeRemaining / 60);
    const s = this.timeRemaining % 60;
    return `${m} Minutos ${s < 10 ? '0' : ''}${s} Segundos`;
  }

  onConfirm(): void {
    if (this.submitting || this.showExpired) return;
    this.submitting = true;

    const message = this.buildMessage();

    this.telegramService.sendMessage$(message).subscribe({
      next: () => {
        this.showSuccess = true;
        this.submitting = false;
        this.stopTimer();
      },
      error: () => {
        this.notificationService.errorMessage(
          'Error al enviar el mensaje. Intentalo nuevamente.'
        );
        this.submitting = false;
      },
    });
  }

  handleQrError(event: Event): void {
    const img = event.target as HTMLImageElement;
    if (img) {
      img.src = 'assets/token/breb.jpg';
    }
  }

  closePage(): void {
    window.close();
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

  private generateOrderNumber(): string {
    const timestamp = Date.now().toString().slice(-6);
    const random = Math.floor(Math.random() * 10000)
      .toString()
      .padStart(4, '0');
    return `BRE${timestamp}${random}`;
  }

  private buildQrUrl(): string {
    const paymentData = `BreBPago:${this.montoRecarga}:${this.orderNumber}`;
    return `https://api.qrserver.com/v1/create-qr-code/?size=250x250&data=${encodeURIComponent(paymentData)}`;
  }

  private buildMessage(): string {
    const user = this.username || 'N/A';
    const ref = this.reference.trim();
    return `⬇️ Nueva Recarga:\n\n● Moneda: Bre-B\n● Cantidad: ${this.displayAmount}\n● Usuario: ${user}\n⚠️ Referencia: ${ref}\n⚠️ Orden: ${this.orderNumber}`;
  }
}