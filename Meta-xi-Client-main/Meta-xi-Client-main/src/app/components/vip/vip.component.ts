import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { trigger, transition, style, animate } from '@angular/animations';
import { NotificationService } from '../../services/products/notification.service';
import { environment } from '../../../environments/environment';
import { Mission } from './Imisions';
import { Completed } from './ICompleted';

@Component({
  selector: 'app-vip',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './vip.component.html',
  styleUrl: './vip.component.scss',
  animations: [
    trigger('slideIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(10px)' }),
        animate('0.4s ease', style({ opacity: 1, transform: 'translateY(0)' })),
      ]),
    ]),
  ],
})
export class VipComponent implements OnInit {
  username: string = localStorage.getItem('username') || '';

  // ─── Stats ──────────────────────────────────
  availableBalance: number = 0;
  todayMission: number = 0;
  totalMissions: number = 0;
  claiming = false;

  // ─── Tab State ─────────────────────────────
  activeTab: 'missions' | 'trend' | 'completed' = 'missions';
  activeTabIndex = 0;

  // ─── Mission Data ──────────────────────────
  missions: Mission[] = [];
  trends: Mission[] = [];
  completedMissions: Completed[] = [];

  constructor(
    private http: HttpClient,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.loadStats();
    this.loadMissions();
  }

  // ─── Tab Switching ─────────────────────────
  switchTab(tab: 'missions' | 'trend' | 'completed', index: number): void {
    this.activeTab = tab;
    this.activeTabIndex = index;

    if (tab === 'trend' && this.trends.length === 0) {
      this.loadTrends();
    } else if (tab === 'completed' && this.completedMissions.length === 0) {
      this.loadCompleted();
    }
  }

  // ─── API: Load Stats ──────────────────────
  private async loadStats(): Promise<void> {
    const url = `${environment.apiUrl}/MisionsUser/GetDates/${this.username}`;
    try {
      const response: any = await firstValueFrom(this.http.get(url));
      this.availableBalance = response.disponibility || 0;
      this.todayMission = response.quantityMisionsToday || 0;
      this.totalMissions = response.quantityMisions || 0;
    } catch (error: any) {
      console.error('Error al obtener las misiones:', error);
    }
  }

  // ─── API: Load Missions ────────────────────
  private async loadMissions(): Promise<void> {
    const url = `${environment.apiUrl}/MisionsUser/GetMissions/${this.username}`;
    try {
      const response: Mission[] = await firstValueFrom(this.http.get<Mission[]>(url));
      this.missions = response || [];
    } catch (error: any) {
      console.error('Error al obtener las misiones:', error);
    }
  }

  // ─── API: Load Trends ──────────────────────
  private async loadTrends(): Promise<void> {
    const url = `${environment.apiUrl}/TrendUser/GetTendency/${this.username}`;
    try {
      const response: Mission[] = await firstValueFrom(this.http.get<Mission[]>(url));
      this.trends = response || [];
    } catch (error: any) {
      console.error('Error al obtener las tendencias:', error);
    }
  }

  // ─── API: Load Completed ──────────────────
  private async loadCompleted(): Promise<void> {
    const url = `${environment.apiUrl}/TrendUser/GetCompletedMissions/${this.username}`;
    try {
      const response: Completed[] = await firstValueFrom(this.http.get<Completed[]>(url));
      this.completedMissions = response || [];
    } catch (error: any) {
      console.error('Error al obtener misiones completadas:', error);
    }
  }

  // ─── API: Claim Balance ────────────────────
  async claimBalance(): Promise<void> {
    if (this.claiming) return;
    this.claiming = true;

    const url = `${environment.apiUrl}/MisionsUser/UpdateWallet/${this.username}`;
    try {
      const response: any = await firstValueFrom(this.http.get(url));
      this.notification.correct('Billetera actualizada correctamente');
      this.availableBalance = 0;
      this.loadStats();
    } catch (error: any) {
      const message = error?.error?.message || error?.message || 'Error al reclamar';
      this.notification.errorMessage(message);
    } finally {
      this.claiming = false;
    }
  }

  // ─── API: Claim Individual Mission ─────────
  async claimMission(missionId: number): Promise<void> {
    const url = `${environment.apiUrl}/MisionsUser/LogToClaim`;
    const data = { idMission: missionId, username: this.username };
    try {
      await firstValueFrom(this.http.post(url, data));
      this.notification.correct('¡Misión reclamada correctamente!');
      this.loadMissions();
      this.loadStats();
    } catch (error: any) {
      const message = error?.error?.message || error?.message || 'Error al reclamar la misión';
      this.notification.errorMessage(message);
    }
  }

  // ─── API: Claim Trend Mission ──────────────
  async claimTrend(trendId: number): Promise<void> {
    const url = `${environment.apiUrl}/TrendUser/LogToClaim`;
    const data = { idMission: trendId, username: this.username };
    try {
      await firstValueFrom(this.http.post(url, data));
      this.notification.correct('¡Misión reclamada correctamente!');
      this.loadTrends();
      this.loadStats();
    } catch (error: any) {
      const message = error?.error?.message || error?.message || 'Error al reclamar la misión';
      this.notification.errorMessage(message);
    }
  }
}