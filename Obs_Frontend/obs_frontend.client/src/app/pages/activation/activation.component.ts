import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-activation',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="activation-container">
      <div class="activation-card">
        <div class="card-header">
          <img src="assets/duzce-logo.png" alt="Düzce Üniversitesi Logo" class="logo" />
          <h2>Şifre Belirleme & Aktivasyon</h2>
          <p>Lütfen hesabınız için yeni bir şifre belirleyin.</p>
        </div>

        <form (ngSubmit)="setNewPassword()">
          <div class="form-group" *ngIf="!activationToken">
            <label for="currentPassword">Geçici / Mevcut Şifre</label>
            <div class="password-input-wrapper">
              <input
                [type]="showCurrentPassword ? 'text' : 'password'"
                id="currentPassword"
                [(ngModel)]="currentPassword"
                name="currentPassword"
                placeholder="••••••••"
                required
              />
              <button
                type="button"
                class="password-toggle"
                (click)="showCurrentPassword = !showCurrentPassword"
                [attr.aria-label]="showCurrentPassword ? 'Geçici şifreyi gizle' : 'Geçici şifreyi göster'"
                [attr.aria-pressed]="showCurrentPassword"
                [title]="showCurrentPassword ? 'Şifreyi gizle' : 'Şifreyi göster'"
              >
                <i class="pi" [ngClass]="showCurrentPassword ? 'pi-eye-slash' : 'pi-eye'"></i>
              </button>
            </div>
          </div>
          <div class="form-group">
            <label for="password">Yeni Şifre</label>
            <div class="password-input-wrapper">
              <input
                [type]="showPassword ? 'text' : 'password'"
                id="password"
                [(ngModel)]="password"
                name="password"
                placeholder="••••••••"
                required
              />
              <button
                type="button"
                class="password-toggle"
                (click)="showPassword = !showPassword"
                [attr.aria-label]="showPassword ? 'Yeni şifreyi gizle' : 'Yeni şifreyi göster'"
                [attr.aria-pressed]="showPassword"
                [title]="showPassword ? 'Şifreyi gizle' : 'Şifreyi göster'"
              >
                <i class="pi" [ngClass]="showPassword ? 'pi-eye-slash' : 'pi-eye'"></i>
              </button>
            </div>
          </div>

          <div class="form-group">
            <label for="confirmPassword">Şifre Tekrarı</label>
            <div class="password-input-wrapper">
              <input
                [type]="showConfirmPassword ? 'text' : 'password'"
                id="confirmPassword"
                [(ngModel)]="confirmPassword"
                name="confirmPassword"
                placeholder="••••••••"
                required
              />
              <button
                type="button"
                class="password-toggle"
                (click)="showConfirmPassword = !showConfirmPassword"
                [attr.aria-label]="showConfirmPassword ? 'Şifre tekrarını gizle' : 'Şifre tekrarını göster'"
                [attr.aria-pressed]="showConfirmPassword"
                [title]="showConfirmPassword ? 'Şifreyi gizle' : 'Şifreyi göster'"
              >
                <i class="pi" [ngClass]="showConfirmPassword ? 'pi-eye-slash' : 'pi-eye'"></i>
              </button>
            </div>
          </div>

          <button type="submit" [disabled]="loading" class="btn-submit">
            {{ loading ? 'Aktifleştiriliyor...' : 'Şifreyi Kaydet ve Aktifleştir' }}
          </button>
        </form>
      </div>
    </div>
  `,
  styles: [`
    .activation-container {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: #f8fafc;
      font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
      padding: 1rem;
    }
    .activation-card {
      background: #ffffff;
      padding: 2.5rem;
      border-radius: 20px;
      box-shadow: 0 20px 40px rgba(15, 23, 42, 0.08);
      width: 100%;
      max-width: 420px;
      border: 1px solid #e2e8f0;
      box-sizing: border-box;
    }
    .card-header {
      text-align: center;
      margin-bottom: 2rem;
    }
    .logo {
      max-height: 60px;
      margin-bottom: 1rem;
    }
    .card-header h2 {
      margin: 0 0 0.5rem 0;
      color: #0f172a;
      font-size: 1.4rem;
    }
    .card-header p {
      color: #64748b;
      font-size: 0.88rem;
      margin: 0;
      line-height: 1.4;
    }
    .form-group {
      margin-bottom: 1.25rem;
      display: flex;
      flex-direction: column;
      gap: 0.4rem;
    }
    label {
      font-size: 0.85rem;
      font-weight: 600;
      color: #334155;
    }
    input {
      width: 100%;
      padding: 0.8rem 1rem;
      border: 1px solid #cbd5e1;
      border-radius: 10px;
      font-size: 0.9rem;
      outline: none;
      box-sizing: border-box;
      transition: border-color 0.2s;
    }
    input:focus {
      border-color: #2563eb;
      box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.12);
    }
    .password-input-wrapper {
      position: relative;
      display: flex;
      align-items: center;
    }
    .password-input-wrapper input {
      padding-right: 3rem;
    }
    .password-toggle {
      position: absolute;
      right: 0.45rem;
      width: 2.25rem;
      height: 2.25rem;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      border: 0;
      border-radius: 8px;
      background: transparent;
      color: #64748b;
      cursor: pointer;
      transition: color 0.2s, background 0.2s;
    }
    .password-toggle:hover {
      color: #2563eb;
      background: #eff6ff;
    }
    .password-toggle:focus-visible {
      outline: 2px solid #2563eb;
      outline-offset: 1px;
    }
    .btn-submit {
      width: 100%;
      padding: 0.85rem;
      background: #2563eb;
      color: #ffffff;
      border: none;
      border-radius: 10px;
      font-weight: 700;
      font-size: 0.9rem;
      cursor: pointer;
      margin-top: 0.5rem;
      transition: background 0.2s;
    }
    .btn-submit:hover {
      background: #1d4ed8;
    }
    .btn-submit:disabled {
      background: #94a3b8;
      cursor: not-allowed;
    }
  `]
})
export class ActivationComponent implements OnInit {
  password: string = '';
  currentPassword: string = '';
  confirmPassword: string = '';
  showCurrentPassword: boolean = false;
  showPassword: boolean = false;
  showConfirmPassword: boolean = false;
  loading: boolean = false;
  activationToken: string = '';

  constructor(
    private http: HttpClient,
    private router: Router,
    private route: ActivatedRoute, // URL'deki token'ı yakalamak için eklendi
    private notification: NotificationService
  ) { }

  ngOnInit() {
    // E-postadan gelen linkin sonundaki "?token=..." kısmını otomatik yakalıyoruz
    this.route.queryParams.subscribe(params => {
      this.activationToken = params['token'] || '';
    });
  }

  setNewPassword() {
    if (!this.password || !this.confirmPassword) {
      this.notification.warning('Lütfen tüm alanları doldurun.');
      return;
    }

    if (this.password !== this.confirmPassword) {
      this.notification.warning('Girdiğiniz şifreler birbiriyle uyuşmuyor.');
      return;
    }
    if (!this.activationToken && !this.currentPassword) {
      this.notification.warning('Geçici şifrenizi girin.');
      return;
    }

    this.loading = true;

    const request = this.activationToken
      ? this.http.post(`${environment.apiBaseUrl}/Auth/ActivateAccount`, { token: this.activationToken, password: this.password })
      : this.http.post(`${environment.apiBaseUrl}/Auth/ChangePassword`, { currentPassword: this.currentPassword, newPassword: this.password });
    request.subscribe({
      next: () => {
        this.notification.success('Hesabınız aktifleştirildi. Yeni şifrenizle giriş yapabilirsiniz.');
        this.router.navigate(['/login']);
      },
      error: (err) => {
        console.error('Aktivasyon hatası:', err);
        const errMsg = err.error?.message || err.error || 'Aktivasyon güncellenemedi.';
        this.notification.error(errMsg, 'Aktivasyon başarısız');
        this.loading = false;
      }
    });
  }
}
