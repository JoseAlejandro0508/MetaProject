import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../../../environments/environment';

@Component({
  selector: 'app-level',
  standalone: true,
  imports: [],
  templateUrl: './level.component.html',
  styleUrl: './level.component.scss',
})
export class LevelComponent {
  username : string = localStorage.getItem("username") || '';
  constructor(private http: HttpClient) {}
  ngOnInit(){
    this.GettingRefers();
  };
  async GettingRefers(){
    const url = `${environment.apiUrl}/Refer/GetReferrer/`+this.username;
    try {
      const response: any = await firstValueFrom(this.http.get(url));
      this.lvl1.register = response.quantityRefersLevel1 || 0;
      this.lvl1.totalIncome = response.level1Earnings || 0;
      this.lvl2.register = response.quantityRefersLevel2 || 0;
      this.lvl2.totalIncome = response.level2Earnings || 0;
      this.lvl3.register = response.quantityRefersLevel3 || 0;
      this.lvl3.totalIncome = response.level3Earnings || 0;
      console.log(response);
    } catch (error) {
      console.error("Error al obtener los referidos", error);
    }
  }
  lvl1 = {
    register: 0,
    totalIncome: 0,
  };
  lvl2 = {
    register: 0,
    totalIncome: 0,
  };
  lvl3 = {
    register: 0,
    totalIncome: 0,
  };
}
