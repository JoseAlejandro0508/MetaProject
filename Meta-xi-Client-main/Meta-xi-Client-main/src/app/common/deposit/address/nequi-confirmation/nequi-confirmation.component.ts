import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { TelegramService } from '../../../../services/products/Telegram.service';
import { NotificationService } from '../../../../services/products/notification.service';

@Component({
  selector: 'app-nequi-confirmation',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './nequi-confirmation.component.html',
  styleUrl: './nequi-confirmation.component.scss',
})
export class NequiConfirmationComponent implements OnInit {
  // Amount from query params (set by NequiComponent)
  montoRecarga = 0;
  // Unique order number
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

    // Generate a unique order number
    this.orderNumber = this.generateOrderNumber();

    // Generate QR with payment info
    this.qrUrl = this.buildQrUrl();

    // Read username from localStorage
    this.username = localStorage.getItem('username') || '';
  }

  get displayAmount(): string {
    return '$ ' + this.montoRecarga.toLocaleString('es-CO');
  }

  get canConfirm(): boolean {
    return this.reference.trim().length > 0 && !this.submitting;
  }

  onConfirm(): void {
    if (this.submitting) return;
    this.submitting = true;

    const message = this.buildMessage();

    this.telegramService.sendMessage$(message).subscribe({
      next: () => {
        this.showSuccess = true;
        this.submitting = false;
      },
      error: () => {
        this.notificationService.errorMessage(
          'Error al enviar el mensaje. Inténtalo nuevamente.'
        );
        this.submitting = false;
      },
    });
  }

  handleQrError(event: Event): void {
    const img = event.target as HTMLImageElement;
    if (img) {
      img.src = 'assets/token/nequi.png';
    }
  }

  closePage(): void {
    // Try to navigate back to home, fallback to close
    window.close();
  }

  private generateOrderNumber(): string {
    const timestamp = Date.now().toString().slice(-6);
    const random = Math.floor(Math.random() * 10000)
      .toString()
      .padStart(4, '0');
    return `NEQ${timestamp}${random}`;
  }

  private buildQrUrl(): string {
    const paymentData = `NequiPago:${this.montoRecarga}:${this.orderNumber}`;
    return `https://api.qrserver.com/v1/create-qr-code/?size=250x250&data=${encodeURIComponent(paymentData)}`;
  }

  private buildMessage(): string {
    const user = this.username || 'N/A';
    return `Nuevo deposito Nequi:\nUsuario: ${user}\nOrden: ${this.orderNumber}\nMonto: ${this.displayAmount}\nReferencia: ${this.reference.trim()}`;
  }
}