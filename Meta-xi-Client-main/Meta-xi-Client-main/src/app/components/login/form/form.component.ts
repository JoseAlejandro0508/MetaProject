import { NgClass } from '@angular/common';
import { Component, inject, Input, OnChanges, SimpleChanges } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { LoginData } from './login.model';
import { CommonModule } from '@angular/common';
import { NotificationService } from '../../../services/products/notification.service';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-form',
  standalone: true,
  imports: [ReactiveFormsModule, NgClass, HttpClientModule, CommonModule],
  templateUrl: './form.component.html',
  styleUrls: ['./form.component.scss'],
})
export class FormComponent implements OnChanges {
  private fb = inject(FormBuilder);
  private http = inject(HttpClient);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private notification = inject(NotificationService);

  @Input() activeTab: 'registro' | 'login' = 'registro';

  // Registration form
  regForm!: FormGroup;
  regCaptchaA = 0;
  regCaptchaB = 0;
  regCaptchaResult = 0;

  // Login form
  loginForm!: FormGroup;
  logCaptchaA = 0;
  logCaptchaB = 0;
  logCaptchaResult = 0;

  passwordFieldType = 'password';
  codeReferrer: string | null = null;

  isSubmitting = false;

  constructor() {
    this.initForms();
  }

  ngOnInit(): void {
    this.route.queryParamMap.subscribe(params => {
      this.codeReferrer = params.get('code');
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['activeTab']) {
      // Regenerate captcha on tab switch
      this.generateCaptcha('reg');
      this.generateCaptcha('log');
    }
  }

  private initForms() {
    // Registration form: phone, name, password, confirmPassword, captcha
    this.regForm = this.fb.group({
      phone: ['', [Validators.required, Validators.minLength(10), Validators.pattern(/^\d+$/)]],
      name: ['', [Validators.required, Validators.minLength(5)]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]],
      captchaInput: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });

    // Login form: phone, password, captcha
    this.loginForm = this.fb.group({
      phone: ['', [Validators.required, Validators.minLength(10), Validators.pattern(/^\d+$/)]],
      password: ['', [Validators.required, Validators.minLength(4)]],
      captchaInput: ['', [Validators.required]]
    });

    this.generateCaptcha('reg');
    this.generateCaptcha('log');
  }

  private passwordMatchValidator(form: FormGroup) {
    const password = form.get('password');
    const confirm = form.get('confirmPassword');
    if (password && confirm && password.value !== confirm.value) {
      return { mismatch: true };
    }
    return null;
  }

  generateCaptcha(type: 'reg' | 'log') {
    const a = Math.floor(Math.random() * 9) + 1;
    const b = Math.floor(Math.random() * 9) + 1;
    if (type === 'reg') {
      this.regCaptchaA = a;
      this.regCaptchaB = b;
      this.regCaptchaResult = a + b;
    } else {
      this.logCaptchaA = a;
      this.logCaptchaB = b;
      this.logCaptchaResult = a + b;
    }
  }

  getCaptchaText(type: 'reg' | 'log'): string {
    if (type === 'reg') return `${this.regCaptchaA} + ${this.regCaptchaB} =`;
    return `${this.logCaptchaA} + ${this.logCaptchaB} =`;
  }

  isRegFieldValid(fieldName: string): boolean | null {
    const control = this.regForm.get(fieldName);
    if (!control) return null;
    return control.value && control.valid;
  }

  isRegFieldInvalid(fieldName: string): boolean | null {
    const control = this.regForm.get(fieldName);
    if (!control) return null;
    return control.value && control.invalid && control.touched;
  }

  isRegCaptchaValid(): boolean {
    const input = this.regForm.get('captchaInput');
    if (!input || !input.value) return false;
    return parseInt(input.value, 10) === this.regCaptchaResult;
  }

  isRegCaptchaInvalid(): boolean {
    const input = this.regForm.get('captchaInput');
    if (!input || !input.value) return false;
    return parseInt(input.value, 10) !== this.regCaptchaResult;
  }

  isLogFieldValid(fieldName: string): boolean | null {
    const control = this.loginForm.get(fieldName);
    if (!control) return null;
    return control.value && control.valid;
  }

  isLogCaptchaValid(): boolean {
    const input = this.loginForm.get('captchaInput');
    if (!input || !input.value) return false;
    return parseInt(input.value, 10) === this.logCaptchaResult;
  }

  isLogCaptchaInvalid(): boolean {
    const input = this.loginForm.get('captchaInput');
    if (!input || !input.value) return false;
    return parseInt(input.value, 10) !== this.logCaptchaResult;
  }

  isRegFormValid(): boolean {
    return this.regForm.valid && this.isRegCaptchaValid() &&
           this.regForm.get('password')?.value === this.regForm.get('confirmPassword')?.value;
  }

  isLoginFormValid(): boolean {
    return this.loginForm.valid && this.isLogCaptchaValid();
  }

  togglePasswordVisibility() {
    this.passwordFieldType = this.passwordFieldType === 'password' ? 'text' : 'password';
  }

  async onRegisterSubmit() {
    if (!this.isRegFormValid() || this.isSubmitting) return;

    this.isSubmitting = true;
    const { phone, name, password } = this.regForm.value;

    const registerData: LoginData = {
      email: null,
      phoneNumber: phone,
      password: password,
      codeReferrer: this.codeReferrer || null
    };

    try {
      const response = await this.register(registerData);
      localStorage.setItem('username', phone);
      this.notification.correct(response.message || 'Registro exitoso');
      setTimeout(() => {
        this.activeTab = 'login';
        this.loginForm.reset();
        this.regForm.reset();
        this.generateCaptcha('reg');
        this.generateCaptcha('log');
      }, 3000);
    } catch (error: any) {
      this.notification.errorMessage(`${error}`);
    } finally {
      this.isSubmitting = false;
    }
  }

  async onLoginSubmit() {
    if (!this.isLoginFormValid() || this.isSubmitting) return;

    this.isSubmitting = true;
    const { phone, password } = this.loginForm.value;

    const loginData: LoginData = {
      email: null,
      phoneNumber: phone,
      password: password
    };

    try {
      const response = await this.login(loginData);
      localStorage.setItem('token', response);
      localStorage.setItem('username', phone);
      this.notification.correct('Login exitoso');
      setTimeout(() => {
        this.router.navigate(['/home']);
      }, 2000);
    } catch (error: any) {
      this.notification.errorMessage(`${error}`);
      this.generateCaptcha('log');
      this.loginForm.patchValue({ captchaInput: '' });
    } finally {
      this.isSubmitting = false;
    }
  }

  async login(data: { email: string | null; phoneNumber: string | null; password: string }): Promise<any> {
    const url = `${environment.apiUrl}/User/Login`;
    try {
      const response = await firstValueFrom(this.http.post(url, data, { responseType: 'text' }));
      return response;
    } catch (error: any) {
      let errorMsg = 'Error desconocido';
      if (error.error) {
        if (typeof error.error === 'string') {
          errorMsg = error.error;
        } else if (error.error.message) {
          errorMsg = error.error.message;
        }
      }
      throw errorMsg;
    }
  }

  async register(data: { email: string | null; phoneNumber: string | null; password: string; codeReferrer?: string | null }): Promise<any> {
    const url = `${environment.apiUrl}/User/UserRegister`;
    try {
      const response = await firstValueFrom(this.http.post<any>(url, data, { observe: 'response' }));
      if (response.status === 200) {
        return response.body;
      } else {
        return 'Error: ' + response.body;
      }
    } catch (error: any) {
      let errorMsg = 'Error desconocido';
      if (typeof error.error === 'string') {
        errorMsg = error.error;
      } else if (error.error && error.error.message) {
        errorMsg = error.error.message;
      }
      throw errorMsg;
    }
  }

  allowOnlyNumbers(event: KeyboardEvent): void {
    const key = event.key;
    if (!/^[0-9]$/.test(key) && key !== 'Backspace' && key !== 'Delete') {
      event.preventDefault();
    }
  }
}