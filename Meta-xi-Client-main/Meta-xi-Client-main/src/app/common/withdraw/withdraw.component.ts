import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NotificationService } from '../../services/products/notification.service';
import { TelegramService } from '../../services/products/Telegram.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-withdraw',
  standalone: true,
  imports: [RouterLink, FormsModule, CommonModule],
  templateUrl: './withdraw.component.html',
  styleUrl: './withdraw.component.scss',
})
export class WithdrawComponent implements OnInit, OnDestroy {
  @Input('token') token: string = '';

  // ─── Form Fields ───────────────────────────
  balance = '0.00';
  username: string = localStorage.getItem('username') || '';
  amount: number | null = null;
  accountNumber: string = '';
  password: string = '';
  captchaInput: string = '';

  // ─── Captcha ───────────────────────────────
  activeCaptcha = '0000';
  captchaFlipping = false;

  // ─── Computed Displays ──────────────────────
  withdrawalFee = 0;
  amountToReceive = 0;
  submitting = false;

  // ─── Validation State ──────────────────────
  amountValid = false;
  insuficient = false;
  amountInvalid = false;
  amountHintColor = 'rgba(255,255,255,0.3)';
  canSubmit = false;

  // ─── Constants (base values in COP) ──────────
  private readonly MIN_AMOUNT_COP = 20000;
  private readonly MAX_AMOUNT_COP = 10000000;
  private readonly USDT_CONVERSION_RATE = 3600; // COP to USDT
  private readonly FEE_RATE = 0.12; // 8% commission

  // ─── Computed Min/Max based on currency ──────
  get MIN_AMOUNT(): number {
    return this.isNequi ? this.MIN_AMOUNT_COP : this.MIN_AMOUNT_COP / this.USDT_CONVERSION_RATE;
  }

  get MAX_AMOUNT(): number {
    return this.isNequi ? this.MAX_AMOUNT_COP : this.MAX_AMOUNT_COP / this.USDT_CONVERSION_RATE;
  }

  constructor(
    private http: HttpClient,
    private notification: NotificationService,
    private telegram: TelegramService
  ) {}

  ngOnInit(): void {
    this.getBalance();
    this.regenerateCaptcha();
  }

  ngOnDestroy(): void {
    // Cleanup handled by Angular
  }

  // ─── Getters ────────────────────────────────
  get isNequi(): boolean {
    return this.token === 'nequi';
  }

  get methodName(): string {
    return this.isNequi ? 'NEQUI' : 'USDT';
  }

  get currency(): string {
    return this.isNequi ? 'COP' : 'USDT';
  }

  get feePercent(): number {
    return Math.round(this.FEE_RATE * 100);
  }

  private formatAmount(value: number): string {
    if (this.isNequi) {
      // COP: no decimals, Colombian format
      return Math.round(value).toLocaleString('es-CO');
    } else {
      // USDT: 2 decimals, international format
      return value.toFixed(2);
    }
  }

  get displayBalance(): string {
    const num = parseFloat(this.balance);
    if (isNaN(num)) return this.balance;
    return this.formatAmount(num);
  }

  get displayFee(): string {
    if (this.withdrawalFee === 0) return '0';
    return this.formatAmount(this.withdrawalFee);
  }

  get displayNet(): string {
    if (this.amountToReceive === 0) return '0';
    return this.formatAmount(this.amountToReceive);
  }

  // ─── Balance API ────────────────────────────
  private async getBalance(): Promise<void> {
    const coin = this.isNequi ? 'COP' : 'USDT';
    const url = `${environment.apiUrl}/Wallet/GetBalance/${this.username}?coin=${coin}`;
    try {
      const response: any = await firstValueFrom(this.http.get(url));
      this.balance = response;
    } catch (error: any) {
      console.error('Error al obtener balance:', error);
    }
  }

  // ─── Captcha ────────────────────────────────
  regenerateCaptcha(): void {
    this.captchaFlipping = true;
    const chars = '0123456789';
    let code = '';
    for (let i = 0; i < 4; i++) {
      code += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    this.activeCaptcha = code;
    this.captchaInput = '';
    setTimeout(() => {
      this.captchaFlipping = false;
      this.validate();
    }, 400);
  }

  // ─── Paste from Clipboard ───────────────────
  async pasteFromClipboard(): Promise<void> {
    try {
      const text = await navigator.clipboard.readText();
      this.accountNumber = text;
      this.validate();
    } catch {
      // Fallback: prompt user to paste manually
      this.notification.errorMessage('Mantén presionado el campo para pegar.');
    }
  }

  // ─── Validation Engine ──────────────────────
  validate(): void {
    const amt = this.amount ?? 0;

    // Amount validation
    if (this.amount !== null && this.amount !== undefined && String(this.amount).length > 0) {
      if (amt < this.MIN_AMOUNT) {
        this.amountValid = false;
        this.amountInvalid = true;
        this.insuficient = false;
        this.amountHintColor = 'var(--red-alert, #ff3333)';
      }
      if(Number(this.balance) < amt ){
        this.amountValid = false;
        this.amountInvalid = true;
        this.insuficient = true;
        this.amountHintColor = 'var(--red-alert, #ff3333)';

      } else {
        this.amountValid = true;
        this.amountInvalid = false;
        this.insuficient = false;
        this.amountHintColor = 'var(--green-neon, #00ff88)';
      }
    } else {
      this.amountValid = false;
      this.amountInvalid = false;
      this.insuficient = false;
      this.amountHintColor = 'rgba(255,255,255,0.3)';
    }

    // Ledger calculation
    if (amt > 0 && amt >= this.MIN_AMOUNT && amt <= this.MAX_AMOUNT) {
      this.withdrawalFee = amt * this.FEE_RATE;
      this.amountToReceive = amt - this.withdrawalFee;
    } else {
      this.withdrawalFee = 0;
      this.amountToReceive = 0;
    }

    // Captcha validation
    const capValid = this.captchaInput === this.activeCaptcha;
    if (this.captchaInput.length > 0) {
      // Dynamic validation handled by CSS classes in template
    }

    // Button unlock logic
    const allFilled = String(this.amount ?? '').length > 0
      && this.accountNumber.length > 0
      && this.password.length > 0
      && this.captchaInput.length > 0;

    this.canSubmit = allFilled && amt >= this.MIN_AMOUNT && capValid;
  }

  // ─── Withdrawal Request ─────────────────────
  async requestWithdrawal(): Promise<void> {
    if (!this.canSubmit || this.submitting) return;

    this.submitting = true;

    // Convert amount to COP for backend if currency is USDT
    const amountToSend = this.isNequi 
      ? (this.amount ?? 0) 
      : ((this.amount ?? 0) * this.USDT_CONVERSION_RATE);

    // Call backend FIRST to deduct balance and record withdrawal
    try {
      const withdrawalUrl = `${environment.apiUrl}/Wallet/RequestWithdrawal`;
      const withdrawalBody = {
        Email: this.username,
        Amount: amountToSend,
        AccountNumber: this.accountNumber,
        Token: this.token,
        Password: this.password
      };

      await firstValueFrom(this.http.post(withdrawalUrl, withdrawalBody));

      // Backend returned 200 — send Telegram notification
      const message = `⬆️ Nuevo Retiro:\n\n● Moneda: ${this.token}\n● Cantidad a enviar: ${Math.round(this.amountToReceive)}\n●Ususario: ${this.username}\n\n⚠️ Cuenta: ${this.accountNumber}`;
      this.telegram.sendMessage(message);
      this.notification.correct('Solicitud de retiro enviada correctamente');
    } catch (error: any) {
      // Backend rejected — show error, do NOT send Telegram
      const msg = error?.error?.message || error?.error || 'Error al procesar el retiro';
      this.notification.errorMessage(typeof msg === 'string' ? msg : 'Error al procesar el retiro');
    }

    this.submitting = false;
  }

  private async verifyPassword(): Promise<boolean> {
    const url = `${environment.apiUrl}/User/VerifyPassword`;
    // The app stores phone number in localStorage as 'username'
    // API supports both Email and PhoneNumber lookup
    const body = { Email: this.username, Password: this.password ,PhoneNumber:this.username};
    try {
      await firstValueFrom(this.http.post(url, body));
      return true;
    } catch (error: any) {
      const msg = error?.error?.message || error?.error || 'Error al verificar contraseña';
      this.notification.errorMessage(typeof msg === 'string' ? msg : 'Error al verificar contraseña');
      return false;
    }
  }
}