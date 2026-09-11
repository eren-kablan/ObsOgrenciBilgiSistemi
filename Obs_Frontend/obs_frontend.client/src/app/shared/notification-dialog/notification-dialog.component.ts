import { CommonModule } from '@angular/common';
import { Component, HostListener } from '@angular/core';
import { NotificationService, NotificationType } from '../../services/notification.service';

@Component({
  selector: 'app-notification-dialog',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notification-dialog.component.html',
  styleUrls: ['./notification-dialog.component.css']
})
export class NotificationDialogComponent {
  readonly notification$ = this.notificationService.notification$;
  inputValue = '';

  constructor(readonly notificationService: NotificationService) {}

  @HostListener('document:keydown.escape')
  close(): void {
    this.inputValue = '';
    this.notificationService.close();
  }

  accept(value = ''): void {
    this.notificationService.accept(value);
    this.inputValue = '';
  }

  iconFor(type: NotificationType): string {
    const icons: Record<NotificationType, string> = {
      success: 'pi-check',
      error: 'pi-times',
      warning: 'pi-exclamation-triangle',
      info: 'pi-info'
    };

    return icons[type];
  }
}
