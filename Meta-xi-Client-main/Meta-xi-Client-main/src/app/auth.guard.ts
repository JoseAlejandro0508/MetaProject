import { CanActivateFn } from '@angular/router';

export const authGuard: CanActivateFn = () => {
  const token = localStorage.getItem('token');
  const username = localStorage.getItem('username');
  if (token && username) {
    // Check welcome bonus status — redirect to welcome if not claimed
    const claimed = localStorage.getItem('welcomeBonusClaimed');
    const currentUrl = window.location.pathname;
    
    if (claimed !== 'true' && !currentUrl.includes('/welcome')) {
      window.location.href = '/welcome';
      return false;
    }
    
    return true;
  } else {
    window.location.href = '/login';
    return false;
  }
};
