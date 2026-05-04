import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { CommonModule } from '@angular/common';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-team',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './team.component.html',
  styleUrl: './team.component.scss',
})
export class TeamComponent implements OnInit {
  username: string = localStorage.getItem('username') || '';

  // ─── Referral ────────────────────────────────────
  referralCode: string = '...';
  referralLink: string = '...';
  codeCopied = false;
  linkCopied = false;

  // ─── Team Stats ─────────────────────────────────
  teamSize = 0;
  newTeamToday = 0;
  teamRecharge = 0;
  teamWithdraw = 0;

  // ─── Level Data ─────────────────────────────────
  lvl1Register = 0;
  lvl1Income = 0;
  lvl2Register = 0;
  lvl2Income = 0;
  lvl3Register = 0;
  lvl3Income = 0;

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.loadReferralData();
    this.loadTeamStats();
    this.loadLevelData();
  }

  // ─── API: Referral Link & Code ──────────────────
  private async loadReferralData(): Promise<void> {
    const url = `${environment.apiUrl}/User/GetLink/${this.username}`;
    try {
      const response: any = await firstValueFrom(this.http.get(url));
      this.referralLink = response.link || '';
      this.referralCode = this.extractCodeFromUrl(this.referralLink);
    } catch (error: any) {
      console.error('Error al obtener datos de referido:', error);
      this.referralCode = '---';
      this.referralLink = '---';
    }
  }

  private extractCodeFromUrl(url: string): string {
    try {
      const urlParams = new URL(url).searchParams;
      return urlParams.get('code') || '';
    } catch {
      return '';
    }
  }

  // ─── API: Team Parameters ──────────────────────
  private async loadTeamStats(): Promise<void> {
    const url = `${environment.apiUrl}/Refer/GetTeamParameters/${this.username}`;
    try {
      const response: any = await firstValueFrom(this.http.get(url));
      this.teamSize = response.teamSize || 0;
      this.teamRecharge = response.teamRecharge || 0;
      this.newTeamToday = response.newTeamToday || 0;
      this.teamWithdraw = response.teamWithdraw || 0;
    } catch (error: any) {
      console.error('Error al obtener parámetros del equipo:', error);
    }
  }

  // ─── API: Level Data ────────────────────────────
  private async loadLevelData(): Promise<void> {
    const url = `${environment.apiUrl}/Refer/GetReferrer/${this.username}`;
    try {
      const response: any = await firstValueFrom(this.http.get(url));
      this.lvl1Register = response.quantityRefersLevel1 || 0;
      this.lvl1Income = response.level1Earnings || 0;
      this.lvl2Register = response.quantityRefersLevel2 || 0;
      this.lvl2Income = response.level2Earnings || 0;
      this.lvl3Register = response.quantityRefersLevel3 || 0;
      this.lvl3Income = response.level3Earnings || 0;
    } catch (error: any) {
      console.error('Error al obtener datos de niveles:', error);
    }
  }

  // ─── Clipboard: Copy Code ───────────────────────
  async copyCode(): Promise<void> {
    if (!this.referralCode || this.referralCode === '...' || this.referralCode === '---') return;
    try {
      await navigator.clipboard.writeText(this.referralCode);
      this.codeCopied = true;
      setTimeout(() => (this.codeCopied = false), 2000);
    } catch {
      this.fallbackCopy(this.referralCode);
      this.codeCopied = true;
      setTimeout(() => (this.codeCopied = false), 2000);
    }
  }

  // ─── Clipboard: Copy Link ──────────────────────
  async copyLink(): Promise<void> {
    if (!this.referralLink || this.referralLink === '...' || this.referralLink === '---') return;
    try {
      await navigator.clipboard.writeText(this.referralLink);
      this.linkCopied = true;
      setTimeout(() => (this.linkCopied = false), 2000);
    } catch {
      this.fallbackCopy(this.referralLink);
      this.linkCopied = true;
      setTimeout(() => (this.linkCopied = false), 2000);
    }
  }

  private fallbackCopy(text: string): void {
    const textArea = document.createElement('textarea');
    textArea.value = text;
    document.body.appendChild(textArea);
    textArea.select();
    document.execCommand('copy');
    document.body.removeChild(textArea);
  }
}