import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

const currentRole = () => (sessionStorage.getItem('role') || '').trim().toLocaleLowerCase('tr-TR');

export const authGuard: CanActivateFn = () => {
  const router = inject(Router);
  if (sessionStorage.getItem('token')) return true;
  return router.createUrlTree(['/login']);
};

export const adminGuard: CanActivateFn = () => {
  const router = inject(Router);
  return currentRole() === 'admin' ? true : router.createUrlTree(['/anasayfa']);
};

export const lecturerGuard: CanActivateFn = () => {
  const router = inject(Router);
  return ['lecturer', 'akademisyen'].includes(currentRole())
    ? true
    : router.createUrlTree(['/anasayfa']);
};

export const studentGuard: CanActivateFn = () => {
  const router = inject(Router);
  return ['student', 'ogrenci', 'öğrenci'].includes(currentRole())
    ? true
    : router.createUrlTree(['/anasayfa']);
};
