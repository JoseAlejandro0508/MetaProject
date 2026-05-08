import { Component, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';

interface SlideData {
  id: number;
  image: string;
  title: string;
  subtitle: string;
}

@Component({
  selector: 'app-carousel',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './carousel.component.html',
  styleUrl: './carousel.component.scss',
})
export class CarouselComponent implements OnDestroy {
  slides: SlideData[] = [
    { id: 0, image: 'assets/carousel/01.webp', title: 'Experiencia Exclusiva', subtitle: 'Membresía Platinum' },
    { id: 1, image: 'assets/carousel/02.webp', title: 'Inversiones Premium', subtitle: 'Acceso Anticipado' },
    { id: 2, image: 'assets/carousel/03.webp', title: 'Tecnología Élite', subtitle: 'Hardware Pro' },
    { id: 3, image: 'assets/carousel/04.webp', title: 'Experiencia Exclusiva', subtitle: 'Membresía Platinum' },
    { id: 4, image: 'assets/carousel/05.webp', title: 'Inversiones Premium', subtitle: 'Acceso Anticipado' },
    { id: 5, image: 'assets/carousel/06.webp', title: 'Tecnología Élite', subtitle: 'Hardware Pro' },
    
  ];

  currentIndex = 0;
  progressWidth = '0%';
  progressTransition = 'none';

  private intervalId: ReturnType<typeof setInterval> | null = null;
  private readonly SLIDE_TIME = 3600;

  constructor() {
    this.resetProgressBar();
    this.startAutoSlide();
  }

  ngOnDestroy(): void {
    if (this.intervalId) {
      clearInterval(this.intervalId);
    }
  }

  handleImgError(event: Event): void {
    const img = event.target as HTMLImageElement;
    if (img) {
      img.src = 'assets/images/movil/1.jpg';
    }
  }

  private nextSlide(): void {
    this.currentIndex = (this.currentIndex + 1) % this.slides.length;
    this.resetProgressBar();
  }

  private resetProgressBar(): void {
    // Reset instantly
    this.progressWidth = '0%';
    this.progressTransition = 'none';

    // Animate to 100% after a microtask so the browser registers the reset
    setTimeout(() => {
      this.progressWidth = '100%';
      this.progressTransition = `width ${this.SLIDE_TIME}ms linear`;
    }, 50);
  }

  private startAutoSlide(): void {
    this.intervalId = setInterval(() => {
      this.nextSlide();
    }, this.SLIDE_TIME);
  }
}