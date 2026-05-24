import { CommonModule, NgClass } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { NotificationService } from '../../services/products/notification.service';
import { environment } from '../../../environments/environment';
import { RouterLink, ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-tasks',
  standalone: true,
  imports: [NgClass, HttpClientModule, RouterLink, CommonModule],
  templateUrl: './glasses.component.html',
  styleUrls: ['./glasses.component.scss'],
})
export class TasksComponent implements OnInit {
  balance = '0.00';
  todayProfits = 0;
  profits = 0;
  list: any[] = [];
  mine = true;

  // API base URL for serving images (strips /api suffix from apiUrl)
  apiUrl = environment.apiUrl;
  apiBaseUrl = environment.apiUrl.replace('/api', '');

  // Modal state
  showBuyModal = false;
  showDetailsModal = false;
  selectedPlan: any = null;
  selectedMyPlan: any = null;

  private http = inject(HttpClient);
  username = localStorage.getItem('username');
  private notificationService = inject(NotificationService);

  constructor(private route: ActivatedRoute) {}

  ngOnInit(): void {
    this.gafasVR().then(() => {
      // Check if there's a plan query param to auto-open
      this.route.queryParams.subscribe(params => {
        const planId = Number(params['plan']);
        if (planId) {
          const plan = this.list.find((p: any) => p.idPlan === planId);
          if (plan) {
            this.openBuyModal(plan);
          }
        }
      });
    });
    this.GetBenefits();
  }

  // --- Formatting helpers ---
  formatCOP(value: number): string {
    return value.toLocaleString('es-CO');
  }

  // --- API calls ---
  async GetBenefits() {
    try {
      if (this.username !== null) {
        const response = await this.GetBenefitsToServer(this.username);
        this.balance = response.acumulatedTotalBenefit;
        this.profits = response.acumulatedTotalBenefit;
        this.todayProfits = response.acumulatedBenefitperHour;
      } else {
        console.log('no hay usuario');
      }
    } catch (error: any) {
      console.error('Error al obtener los beneficios: ', error);
    }
  }

  async gafasVR() {
    try {
      const data = await this.GetPlans();
      this.list = data.sort((a: any, b: any) => a.idPlan - b.idPlan);
      this.mine = true;
    } catch (error) {
      console.error('Error al obtener los planes: ', error);
    }
  }

  async myGafas() {
    try {
      const data = await this.GetMyPlans();
      this.list = data.sort((a: any, b: any) => a.idPlan - b.idPlan);
      this.mine = false;
    } catch (error) {
      console.error('Error al obtener mis planes: ', error);
    }
  }

  async GetMyPlans(): Promise<any> {
    const url = `${environment.apiUrl}/UserPlans/GetUserPlans/` + this.username;
    try {
      const response = await firstValueFrom(this.http.get(url));
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

  async GetPlans(): Promise<any> {
    const url = `${environment.apiUrl}/Plans/Plans/` + this.username;
    try {
      const response = await firstValueFrom(this.http.get(url));
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

  async buyPlan(name: string): Promise<void> {
    const url = `${environment.apiUrl}/UserPlans/UserBuyPlans`;
    const body = {
      idPlan: name,
      username: this.username,
    };
    try {
      const response = await firstValueFrom(this.http.post<any>(url, body));
      if (response) this.notificationService.correct(response.message);
      this.closeBuyModal();
      // Refresh lists after purchase
      if (this.mine) {
        this.gafasVR();
      } else {
        this.myGafas();
      }
      this.GetBenefits();
    } catch (error: any) {
      let errorMsg = 'Error desconocido';
      if (error.error && error.error.message) {
        errorMsg = error.error.message;
      }
      this.notificationService.errorMessage(errorMsg);
    }
  }

  async GetBenefitsToServer(name: string): Promise<any> {
    const url = `${environment.apiUrl}/UserPlans/GetBalaceToUser/` + this.username;
    try {
      const response = await firstValueFrom(this.http.get(url));
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

  // --- Modal controls ---
  openBuyModal(plan: any) {
    this.selectedPlan = plan;
    this.showBuyModal = true;
  }

  closeBuyModal() {
    this.showBuyModal = false;
    this.selectedPlan = null;
  }

  openDetailsModal(plan: any) {
    this.selectedMyPlan = plan;
    this.showDetailsModal = true;
  }

  closeDetailsModal() {
    this.showDetailsModal = false;
    this.selectedMyPlan = null;
  }

  handleImageError(event: Event) {
    const img = event.target as HTMLImageElement;
    img.src = 'assets/glasses/tinified/vr1.png';
  }

  getImageSrc(plan: any): string {
    if (plan.imageUrl) {
      return this.apiBaseUrl + plan.imageUrl;
    }
    return 'assets/glasses/tinified/vr' + plan.idPlan + '.png';
  }

  onModalBackdropClick(event: Event) {
    const target = event.target as HTMLElement;
    if (target.classList.contains('modal')) {
      this.closeBuyModal();
      this.closeDetailsModal();
    }
  }

  // --- Computed values for buy modal ---
  get selectedPlanMonthlyEarnings(): number {
    return this.selectedPlan ? Math.round(this.selectedPlan.dailyBenefit * 30) : 0;
  }

  get selectedPlanYearlyEarnings(): number {
    return this.selectedPlan ? Math.round(this.selectedPlan.dailyBenefit * 365) : 0;
  }
}