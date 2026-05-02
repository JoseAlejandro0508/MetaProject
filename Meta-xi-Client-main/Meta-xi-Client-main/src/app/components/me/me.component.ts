import { Component, OnInit, AfterViewInit } from '@angular/core';
import { AbstractControl, FormControl, FormGroup, ValidationErrors, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { CommonModule } from '@angular/common';
import { trigger, transition, style, animate } from '@angular/animations';
import { NotificationService } from '../../services/products/notification.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-me',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './me.component.html',
  styleUrl: './me.component.scss',
  animations: [
    trigger('slideIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(20px)' }),
        animate('0.4s cubic-bezier(0.16, 1, 0.3, 1)', style({ opacity: 1, transform: 'translateY(0)' })),
      ]),
    ]),
  ],
})
export class MeComponent implements OnInit, AfterViewInit {
  // ─── User & Balance ──────────────────────────────────
  username: string = localStorage.getItem('username') || '';
  balanceCOP: number = 0;
  balanceUSD: string = '0.00';
  referralLink: string = '';

  // ─── View State ──────────────────────────────────────
  currentView: 'main' | 'pass' | 'phone' = 'main';
  submitting = false;

  // ─── Captcha ──────────────────────────────────────────
  captchaPass = '';
  captchaPhone = '';

  // ─── Password Visibility ─────────────────────────────
  passFieldTypeOld = 'password';
  passFieldTypeNew = 'password';
  passFieldTypeConfirm = 'password';

  // ─── Form Validity Flags ──────────────────────────────
  passFormValid = false;
  phoneFormValid = false;

  // ─── Password Form ───────────────────────────────────
  passForm = new FormGroup(
    {
      oldPassword: new FormControl('', [Validators.required, Validators.minLength(6)]),
      newPassword: new FormControl('', [Validators.required, Validators.minLength(6), this.passwordStrengthValidator]),
      confirmPassword: new FormControl('', [Validators.required]),
      captchaPass: new FormControl('', [Validators.required]),
    },
    { validators: this.passwordMatchValidator }
  );

  // ─── Phone Form ──────────────────────────────────────
  phoneForm = new FormGroup({
    oldPhone: new FormControl('', [Validators.required, Validators.minLength(7)]),
    newPhone: new FormControl('', [Validators.required, Validators.minLength(7)]),
    captchaPhone: new FormControl('', [Validators.required]),
  });

  constructor(
    private http: HttpClient,
    private notification: NotificationService,
    private router: Router
  ) {
    this.captchaPass = this.generateCode();
    this.captchaPhone = this.generateCode();
  }

  // ─── Lifecycle ──────────────────────────────────────
  ngOnInit(): void {
    this.getMyBalance();
    this.getReferralLink();
  }

  ngAfterViewInit(): void {
    this.refreshIcons();
  }

  // ─── Icon Refresh (Lucide via CDN) ───────────────────
  private refreshIcons(): void {
    setTimeout(() => {
      try {
        const w = window as any;
        if (w.lucide && typeof w.lucide.createIcons === 'function') {
          w.lucide.createIcons();
        }
      } catch {
        // Lucide CDN may not be loaded yet, ignore
      }
    }, 100);
  }

  // ─── Captcha ─────────────────────────────────────────
  private generateCode(): string {
    return Math.floor(1000 + Math.random() * 9000).toString();
  }

  // ─── View Switching ──────────────────────────────────
  switchView(view: 'main' | 'pass' | 'phone'): void {
    this.currentView = view;

    if (view !== 'main') {
      this.resetFormFields();
      this.captchaPass = this.generateCode();
      this.captchaPhone = this.generateCode();
    }

    // Refresh Lucide icons after Angular renders new DOM
    this.refreshIcons();
  }

  private resetFormFields(): void {
    this.passForm.reset();
    this.phoneForm.reset();
    this.passFormValid = false;
    this.phoneFormValid = false;
    this.passFieldTypeOld = 'password';
    this.passFieldTypeNew = 'password';
    this.passFieldTypeConfirm = 'password';
  }

  // ─── Password Visibility Toggle ──────────────────────
  togglePassVisibility(field: 'old' | 'new' | 'confirm'): void {
    switch (field) {
      case 'old':
        this.passFieldTypeOld = this.passFieldTypeOld === 'password' ? 'text' : 'password';
        break;
      case 'new':
        this.passFieldTypeNew = this.passFieldTypeNew === 'password' ? 'text' : 'password';
        break;
      case 'confirm':
        this.passFieldTypeConfirm = this.passFieldTypeConfirm === 'password' ? 'text' : 'password';
        break;
    }
  }

  // ─── Form Validation: Password ──────────────────────
  validatePassForm(): void {
    const captchaValue = this.passForm.get('captchaPass')?.value || '';
    const captchaValid = captchaValue === this.captchaPass;

    if (captchaValid) {
      this.passForm.get('captchaPass')?.setErrors(null);
    } else if (captchaValue.length > 0) {
      this.passForm.get('captchaPass')?.setErrors({ captchaMismatch: true });
    }

    // Check full form validity
    this.passFormValid =
      this.passForm.get('oldPassword')?.valid === true &&
      this.passForm.get('newPassword')?.valid === true &&
      this.passForm.get('confirmPassword')?.valid === true &&
      !this.passForm.hasError('passwordMismatch') &&
      captchaValid &&
      !this.submitting;
  }

  // ─── Form Validation: Phone ──────────────────────────
  validatePhoneForm(): void {
    const captchaValue = this.phoneForm.get('captchaPhone')?.value || '';
    const captchaValid = captchaValue === this.captchaPhone;

    if (captchaValid) {
      this.phoneForm.get('captchaPhone')?.setErrors(null);
    } else if (captchaValue.length > 0) {
      this.phoneForm.get('captchaPhone')?.setErrors({ captchaMismatch: true });
    }

    this.phoneFormValid =
      this.phoneForm.get('oldPhone')?.valid === true &&
      this.phoneForm.get('newPhone')?.valid === true &&
      captchaValid &&
      !this.submitting;
  }

  // ─── Custom Validators ──────────────────────────────
  private passwordStrengthValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.value;
    if (!password) return null;

    const hasUpper = /[A-Z]/.test(password);
    const hasLower = /[a-z]/.test(password);
    const hasDigit = /\d/.test(password);
    const hasSpecial = /[^\da-zA-Z]/.test(password);

    const errors: string[] = [];
    if (!hasUpper) errors.push('una mayúscula');
    if (!hasLower) errors.push('una minúscula');
    if (!hasDigit) errors.push('un número');
    if (!hasSpecial) errors.push('un carácter especial');

    if (errors.length > 0) {
      return {
        passwordStrength: `La contraseña debe tener al menos ${errors.join(', ')}`,
      };
    }

    return null;
  }

  private passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const newPassword = control.get('newPassword')?.value;
    const confirmPassword = control.get('confirmPassword')?.value;

    if (newPassword && confirmPassword && newPassword !== confirmPassword) {
      return { passwordMismatch: true };
    }
    return null;
  }

  // ─── API: Get Balance ────────────────────────────────
  async getMyBalance(): Promise<void> {
    const url = `${environment.apiUrl}/Wallet/GetBalanceUsdAndCop/${this.username}`;
    try {
      const response: any = await firstValueFrom(this.http.get(url));
      this.balanceCOP = response.balanceInCop || 0;
      this.balanceUSD = response.balanceInUsd || '0.00';
    } catch (error: any) {
      const message = error?.error?.message || error?.message || 'Error al obtener el balance';
      console.error(message);
    }
  }

  // ─── API: Get Referral Link ──────────────────────────
  async getReferralLink(): Promise<void> {
    const url = `${environment.apiUrl}/User/GetLink/${this.username}`;
    try {
      const response: any = await firstValueFrom(this.http.get(url));
      this.referralLink = response.link || '';
    } catch (error: any) {
      console.error('Error al obtener link de referido:', error);
    }
  }

  // ─── API: Submit Password Change ─────────────────────
  async submitPassword(): Promise<void> {
    if (!this.passFormValid || this.submitting) return;

    this.submitting = true;
    const oldPassword = this.passForm.get('oldPassword')?.value;
    const newPassword = this.passForm.get('newPassword')?.value;

    try {
      const response: any = await this.changePassword({
        Username: this.username,
        OldPassword: oldPassword,
        NewPassword: newPassword,
      });

      this.notification.correct(response.message || 'Contraseña actualizada correctamente');
      this.switchView('main');
    } catch (error: any) {
      const message = error?.error?.message || error || 'Error al actualizar la contraseña';
      this.notification.errorMessage(message);
    } finally {
      this.submitting = false;
    }
  }

  private async changePassword(data: {
    Username: string;
    OldPassword: string;
    NewPassword: string;
  }): Promise<any> {
    const url = `${environment.apiUrl}/User/UpdatePassword`;
    const response = await firstValueFrom(
      this.http.patch<any>(url, data, { observe: 'response' })
    );

    if (response.status === 200) {
      return response.body;
    }
    throw new Error('Error inesperado');
  }

  // ─── API: Submit Phone Change ─────────────────────────
  async submitPhone(): Promise<void> {
    if (!this.phoneFormValid || this.submitting) return;

    this.submitting = true;
    const oldPhone = this.phoneForm.get('oldPhone')?.value;
    const newPhone = this.phoneForm.get('newPhone')?.value;

    try {
      const response: any = await this.changePhoneNumber({
        Username: this.username,
        OldPhoneNumber: `+57${oldPhone}`,
        NewPhoneNumber: `+57${newPhone}`,
      });

      this.notification.correct(response.message || 'Número actualizado correctamente');
      this.switchView('main');
    } catch (error: any) {
      const message = error?.error?.message || error || 'Error al actualizar el número';
      this.notification.errorMessage(message);
    } finally {
      this.submitting = false;
    }
  }

  private async changePhoneNumber(data: {
    Username: string;
    OldPhoneNumber: string;
    NewPhoneNumber: string;
  }): Promise<any> {
    const url = `${environment.apiUrl}/User/UpdatePhoneNumber`;
    const response = await firstValueFrom(
      this.http.patch<any>(url, data, { observe: 'response' })
    );

    if (response.status === 200) {
      return response.body;
    }
    throw new Error('Error inesperado');
  }

  // ─── API: Logout ──────────────────────────────────────
  async logout(): Promise<void> {
    const url = `${environment.apiUrl}/User/Logout/${this.username}`;
    try {
      const response: any = await firstValueFrom(this.http.get(url));
      localStorage.removeItem('username');
      localStorage.removeItem('token');
      this.notification.correct(response.message || 'Sesión cerrada correctamente');
      setTimeout(() => this.router.navigate(['/login']), 3000);
    } catch {
      // Even if the API call fails, clear local data and redirect
      localStorage.removeItem('username');
      localStorage.removeItem('token');
      this.router.navigate(['/login']);
    }
  }

  // ─── Copy Referral Link ───────────────────────────────
  async copyReferralLink(): Promise<void> {
    if (!this.referralLink) {
      await this.getReferralLink();
    }

    if (this.referralLink) {
      try {
        await navigator.clipboard.writeText(this.referralLink);
        this.notification.correct('Link copiado al portapapeles');
      } catch {
        const textArea = document.createElement('textarea');
        textArea.value = this.referralLink;
        document.body.appendChild(textArea);
        textArea.select();
        document.execCommand('copy');
        document.body.removeChild(textArea);
        this.notification.correct('Link copiado al portapapeles');
      }
    } else {
      this.notification.errorMessage('No se pudo obtener el link de referido');
    }
  }
}