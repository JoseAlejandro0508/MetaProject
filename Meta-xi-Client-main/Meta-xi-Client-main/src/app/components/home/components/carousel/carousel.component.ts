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
    { id: 0, image: 'https://images.unsplash.com/photo-1614850523296-d8c1af93d400?q=80&w=800', title: 'Experiencia Exclusiva', subtitle: 'Membresía Platinum' },
    { id: 1, image: 'https://images.unsplash.com/photo-1633174524827-db00a6b7bc74?q=80&w=800', title: 'Inversiones Premium', subtitle: 'Acceso Anticipado' },
    { id: 2, image: 'https://images.unsplash.com/photo-1550745165-9bc0b252726f?q=80&w=800', title: 'Tecnología Élite', subtitle: 'Hardware Pro' },
    
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