import { Component, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';

interface MemberEntry {
  id: number;
  phoneNumber: number;
  amount: number;
  amountFormatted: string;
  avatarUrl: string;
  isNew: boolean;
}

@Component({
  selector: 'app-memberlist',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './memberlist.component.html',
  styleUrl: './memberlist.component.scss',
})
export class MemberlistComponent implements OnDestroy {
  userlist: MemberEntry[] = [];
  private intervalId: ReturnType<typeof setInterval> | null = null;
  private idCounter = 0;

  constructor(private cdr: ChangeDetectorRef) {
    // Generate initial 5 entries with staggered animation
    for (let i = 0; i < 5; i++) {
      setTimeout(() => {
        this.addEntry(true);
      }, i * 120);
    }
    this.startScrolling();
  }

  ngOnDestroy(): void {
    if (this.intervalId) {
      clearInterval(this.intervalId);
    }
  }

  trackByIndex(index: number): number {
    return index;
  }

  handleAvatarError(event: Event): void {
    const img = event.target as HTMLImageElement;
    if (img) {
      img.src = `https://i.pravatar.cc/80?u=fallback${Math.floor(Math.random() * 100)}`;
    }
  }

  private generatePhoneNumber(): number {
    return Math.floor(Math.random() * 9000) + 1000;
  }

  private generateAmount(): number {
    // Realistic VIP amounts: 10,000 to 50,000 COP
    return Math.floor(Math.random() * 40 + 10) * 1000;
  }

  private formatAmount(amount: number): string {
    return amount.toLocaleString('es-CO');
  }

  private createEntry(isNew: boolean = false): MemberEntry {
    const avatarId = Math.floor(Math.random() * 42);
    const amount = this.generateAmount();
    return {
      id: ++this.idCounter,
      phoneNumber: this.generatePhoneNumber(),
      amount,
      amountFormatted: this.formatAmount(amount),
      avatarUrl: `assets/avatars/${avatarId}.png`,
      isNew,
    };
  }

  private addEntry(isNew: boolean): void {
    const entry = this.createEntry(isNew);
    // Create new array reference so Angular detects the change
    const updated = [entry, ...this.userlist].slice(0, 5);
    this.userlist = updated;
    // Remove isNew flag after animation completes
    if (isNew) {
      setTimeout(() => {
        entry.isNew = false;
        this.cdr.markForCheck();
      }, 500);
    }
  }

  private startScrolling(): void {
    this.intervalId = setInterval(() => {
      this.addEntry(true);
    }, 1500);
  }
}