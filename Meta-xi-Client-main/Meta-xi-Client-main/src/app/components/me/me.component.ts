import { Component } from '@angular/core';
import {  Router, RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { NotificationService } from '../../services/products/notification.service';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-me',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './me.component.html',
  styleUrl: './me.component.scss',
})
export class MeComponent {
  username : string = localStorage.getItem('username') || '';
  monto = '0.00';
  total = 0;
  constructor(private http: HttpClient,
    private notification: NotificationService,
    private router: Router
  ) {}
  ngOnInit(): void{
    this.GetMyBalance();
  }
  async GetMyBalance(){
    const url = `${environment.apiUrl}/Wallet/GetBalanceUsdAndCop/`+this.username;
    try {
      const response : any = await firstValueFrom(this.http.get(url));
      this.total = response.balanceInCop;
      this.monto = response.balanceInUsd;
    } catch (error : any) {
      const response = error.error.message;
      console.error(response);
    }
  }
  async Logout(){
    const url = `${environment.apiUrl}/User/Logout/`+this.username;
    try {
      const response : any = await firstValueFrom(this.http.get(url));
      localStorage.removeItem('username');
      localStorage.removeItem('token');
      this.notification.correct(response.message);
      setTimeout(()=>
        this.router.navigate(['/login']),6000
      )
    } catch (error : any) {
      const response = error.error.message;
      this.notification.errorMessage(response);
    }
  }
}
