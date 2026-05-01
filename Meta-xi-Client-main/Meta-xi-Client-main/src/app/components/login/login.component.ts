import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormComponent } from './form/form.component';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormComponent],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  activeTab: 'registro' | 'login' = 'registro';

  showForm(type: 'registro' | 'login') {
    this.activeTab = type;
  }
}