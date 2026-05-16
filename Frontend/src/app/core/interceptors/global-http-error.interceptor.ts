import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

export const globalHttpErrorInterceptor: HttpInterceptorFn = (request, next) => {
  return next(request).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse && error.status === 401) {
        localStorage.removeItem('logiflow.accessToken');
        localStorage.removeItem('logiflow.user');

        if (!request.url.includes('/auth/login')) {
          window.location.href = '/login';
        }
      }

      return throwError(() => error);
    })
  );
};
