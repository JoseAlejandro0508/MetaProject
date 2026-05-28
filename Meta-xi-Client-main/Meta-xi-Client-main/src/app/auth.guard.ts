import { CanActivateFn, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { environment } from '../environments/environment';

export const authGuard: CanActivateFn = async () => {
  const http = inject(HttpClient);
  const router = inject(Router);
  
  const token = localStorage.getItem('token');
  const username = localStorage.getItem('username');
  
  if (!token || !username) {
    router.navigate(['/login']);
    return false;
  }

  const currentUrl = window.location.pathname;
  
  // If already on welcome page, allow
  if (currentUrl.includes('/welcome')) {
    return true;
  }

  // Check welcome bonus status directly from API
  try {
    const url = `${environment.apiUrl}/Wallet/CheckWelcomeBonus/${username}`;
    const response: any = await firstValueFrom(http.get(url));
    
    if (response && response.claimed === true) {
      return true;
    } else {
      // Not claimed — redirect to welcome page
      router.navigate(['/welcome']);
      return false;
    }
  } catch (error) {
    // If API fails, allow access (fail open) to avoid locking user out
    console.error('Error checking welcome bonus in authGuard:', error);
    return true;
  }
};
