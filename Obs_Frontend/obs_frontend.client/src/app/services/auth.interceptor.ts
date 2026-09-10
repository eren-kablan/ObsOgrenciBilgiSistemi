import { HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';

export class AuthInterceptor implements HttpInterceptor {
  // İsteklere JWT ekleme
  intercept(request: HttpRequest<unknown>, next: HttpHandler) {
    const token = sessionStorage.getItem('token');
    return next.handle(token && !request.headers.has('Authorization')
      ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : request);
  }
}
