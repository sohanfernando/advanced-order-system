import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';
import { AuthService } from './auth.service';

const DEFAULT_URL = '/orders';

// Only allow in-app paths, so ?returnUrl= can't send users to another site
export function safeReturnUrl(url: string | null | undefined): string {
  const isInternal =
    !!url && url.startsWith('/') && !url.startsWith('//') && !url.startsWith('/\\');

  return isInternal && !url.startsWith('/login') && !url.startsWith('/register')
    ? url
    : DEFAULT_URL;
}

export const authGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  return auth.ensureSession().pipe(
    map((signedIn) =>
      signedIn
        ? true
        : router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } }),
    ),
  );
};

// Signed-in admins don't need the login / register pages
export const guestGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  return auth
    .ensureSession()
    .pipe(map((signedIn) => (signedIn ? router.createUrlTree([DEFAULT_URL]) : true)));
};
