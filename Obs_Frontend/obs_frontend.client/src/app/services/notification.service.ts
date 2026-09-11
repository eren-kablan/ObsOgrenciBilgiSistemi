import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export type NotificationType = 'success' | 'error' | 'warning' | 'info';
export type NotificationMode = 'notice' | 'confirm' | 'prompt';

export interface AppNotification {
  title: string;
  message: string;
  type: NotificationType;
  mode: NotificationMode;
  confirmLabel: string;
  placeholder?: string;
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly notificationSubject = new BehaviorSubject<AppNotification | null>(null);
  private resolver?: (value: boolean | string | null) => void;
  readonly notification$ = this.notificationSubject.asObservable();

  success(message: string, title = 'İşlem başarılı'): void {
    this.show(message, 'success', title);
  }

  error(message: string, title = 'İşlem tamamlanamadı'): void {
    this.show(message, 'error', title);
  }

  warning(message: string, title = 'Dikkat'): void {
    this.show(message, 'warning', title);
  }

  info(message: string, title = 'Bilgilendirme'): void {
    this.show(message, 'info', title);
  }

  close(): void {
    this.finish(null);
  }

  confirm(message: string, title = 'Onay gerekiyor', confirmLabel = 'Onayla'): Promise<boolean> {
    return new Promise(resolve => {
      this.dismissPending();
      this.resolver = value => resolve(value === true);
      this.notificationSubject.next({
        message,
        title,
        type: 'warning',
        mode: 'confirm',
        confirmLabel
      });
    });
  }

  prompt(message: string, title = 'Bilgi gerekli', placeholder = ''): Promise<string | null> {
    return new Promise(resolve => {
      this.dismissPending();
      this.resolver = value => resolve(typeof value === 'string' ? value : null);
      this.notificationSubject.next({
        message,
        title,
        type: 'info',
        mode: 'prompt',
        confirmLabel: 'Gönder',
        placeholder
      });
    });
  }

  accept(value = ''): void {
    const notification = this.notificationSubject.value;
    if (!notification || notification.mode === 'notice') {
      this.finish(null);
      return;
    }

    this.finish(notification.mode === 'confirm' ? true : value.trim());
  }

  private show(message: string, type: NotificationType, title: string): void {
    this.dismissPending();
    this.notificationSubject.next({
      message,
      type,
      title,
      mode: 'notice',
      confirmLabel: 'Tamam'
    });
  }

  private dismissPending(): void {
    if (this.notificationSubject.value) {
      this.finish(null);
    }
  }

  private finish(value: boolean | string | null): void {
    const resolve = this.resolver;
    this.resolver = undefined;
    this.notificationSubject.next(null);
    resolve?.(value);
  }
}
