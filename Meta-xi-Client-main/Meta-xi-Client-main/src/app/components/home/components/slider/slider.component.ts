import { Component, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';

interface CardData {
  id: number;
  image: string;
  label: string;
}

@Component({
  selector: 'app-slider',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './slider.component.html',
  styleUrl: './slider.component.scss',
})
export class SliderComponent implements OnDestroy {
  cards: CardData[] = [
    { id: 0, image: 'https://images.unsplash.com/photo-1600585154340-be6161a56a0c?auto=format&fit=crop&w=600&q=80', label: 'COMPRAR' },
    { id: 1, image: 'https://images.unsplash.com/photo-1512917774080-9991f1c4c750?auto=format&fit=crop&w=600&q=80', label: 'COMPRAR' },
    { id: 2, image: 'https://images.unsplash.com/photo-1613490493576-7fde63acd811?auto=format&fit=crop&w=600&q=80', label: 'COMPRAR' },
  ];

  // Positions: which class each card has
  // Matches original: c1=active, c2=back-right, c3=back-left
  positions: string[] = ['active', 'back-right', 'back-left'];
  step = 0;

  private intervalId: ReturnType<typeof setInterval> | null = null;

  constructor() {
    this.startRotation();
  }

  ngOnDestroy(): void {
    if (this.intervalId) {
      clearInterval(this.intervalId);
    }
  }

  /** Returns the CSS class for a given card based on current step */
  getPosition(cardId: number): string {
    const posIndex = (cardId + this.step) % 3;
    return this.positions[posIndex];
  }

  onBuy(cardId: number): void {
    console.log(`Buy card ${cardId + 1}`);
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

  // Exact replica of the original carta6.html JS:
  // cards.forEach((card, index) => {
  //   card.classList.remove('active', 'back-right', 'back-left');
  //   const nextPos = (index + step) % 3;
  //   card.classList.add(positions[nextPos]);
  // });
  // step = (step + 1) % 3;
  private rotateStage(): void {
    this.step = (this.step + 1) % 3;
  }
}