import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { tap } from 'rxjs';
import { loginUrl } from './login-page';

// The server owns the session, so any 401 means "log in again"
export const unauthorizedInterceptor: HttpInterceptorFn = (request, next) => {
  const router = inject(Router);
  return next(request).pipe(
    tap({
      error: (error: unknown) => {
        if (error instanceof HttpErrorResponse && error.status === 401 && request.url !== loginUrl)
          void router.navigate(['/login']);
      },
    }),
  );
};
