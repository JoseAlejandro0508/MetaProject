import { Component } from '@angular/core';
import { NavComponent } from './components/nav/nav.component';
import { firstValueFrom } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { NotificationService } from '../../services/products/notification.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-vip',
  standalone: true,
  imports: [NavComponent],
  templateUrl: './vip.component.html',
  styleUrl: './vip.component.scss',
})
export class VipComponent {
  cliam = '0.00';
  todayMission = 0;
  missions = 0;
  username = localStorage.getItem('username');
  constructor(private http: HttpClient,
    private notification :NotificationService
  ) { }
  ngOnInit(): void {
    this.Obteneretalles();
  }
  async Obteneretalles(){
    const url = `${environment.apiUrl}/MisionsUser/GetDates/`+this.username;
    try {
      const response : any = await firstValueFrom(this.http.get(url));
      this.cliam = response.disponibility;
      this.todayMission = response.quantityMisionsToday;
      this.missions = response.quantityMisions;
    } catch (error) {
      console.error('Error al obtener las misiones:', error);
    }
  }
  async Claim(){
    const url = `${environment.apiUrl}/MisionsUser/UpdateWallet/`+this.username;
    try {
      const response = await firstValueFrom(this.http.get(url));
      console.log('Respuesta de la API:', response);
      this.notification.correct('Billetera actualizada correctamente');
    } catch (error: any) {
      console.error('Error al reclamar la misión:', error);
    }
  }
}


