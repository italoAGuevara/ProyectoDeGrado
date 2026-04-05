import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.requireAuth() === false) {
    return true;
  }

  const token = auth.getToken();
  if (!token) {
    router.navigate(['/login']);
    return false;
  }

  if (auth.isAccessTokenExpired(token)) {
    auth.logout();
    router.navigate(['/login']);
    return false;
  }

  return true;
};
