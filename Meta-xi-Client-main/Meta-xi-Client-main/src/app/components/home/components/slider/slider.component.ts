import { Component, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';

interface CardData {
  id: number;
  image: string;
  name: string;
  price: string;
  planId: number; // ID del plan de inversión correspondiente
}

@Component({
  selector: 'app-slider',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './slider.component.html',
  styleUrl: './slider.component.scss',
})
export class SliderComponent implements OnDestroy {
  cards: CardData[] = [
    {
      id: 0,
      image: 'assets/slide/3.webp',
      name: 'Turnberry',
      price: '50,000 COP',
      planId: 1,
    },
    {
      id: 1,
      image: 'assets/slide/2.webp',
      name: 'Trump Tower',
      price: '100,000 COP',
      planId: 2,
    },
    {
      id: 2,
      image: 'assets/slide/1.webp',
      name: 'Casino',
      price: '350,000 COP',
      planId: 5,
    },
  ];

  positions: string[] = ['active', 'back-right', 'back-left'];
  step = 0;

  private intervalId: ReturnType<typeof setInterval> | null = null;

  constructor(private router: Router) {
    this.startRotation();
  }

  ngOnDestroy(): void {
    if (this.intervalId) {
      clearInterval(this.intervalId);
    }
  }

  getPosition(cardId: number): string {
    const posIndex = (cardId + this.step) % 3;
    return this.positions[posIndex];
  }

  onBuy(cardId: number): void {
    const card = this.cards.find(c => c.id === cardId);
    if (card) {
      this.router.navigate(['/tasks'], { queryParams: { plan: card.planId } });
    }
  }

  handleImgError(event: Event): void {
    const img = event.target as HTMLImageElement;
    if (img) {
      img.src = 'assets/carteles/Carta 1 PNG.png';
    }
  }

  private startRotation(): void {
    this.intervalId = setInterval(() => {
      this.rotateStage();
    }, 3000);
  }

  private rotateStage(): void {
    this.step = (this.step + 1) % 3;
  }
}