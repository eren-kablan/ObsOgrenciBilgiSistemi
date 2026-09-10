import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { HttpClient } from '@angular/common/http';

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
            <input type="password" id="currentPassword" [(ngModel)]="currentPassword" name="currentPassword" required />
          </div>
          <div class="form-group">
            <label for="password">Yeni Şifre</label>
            <input
              type="password"
              id="password"
              [(ngModel)]="password"
              name="password"
              placeholder="••••••••"
              required
            />
          </div>

          <div class="form-group">
            <label for="confirmPassword">Şifre Tekrarı</label>
            <input
              type="password"
              id="confirmPassword"
              [(ngModel)]="confirmPassword"
              name="confirmPassword"
              placeholder="••••••••"
              required
            />
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
  loading: boolean = false;
  activationToken: string = '';

  constructor(
    private http: HttpClient,
    private router: Router,
    private route: ActivatedRoute // URL'deki token'ı yakalamak için eklendi
  ) { }

  ngOnInit() {
    // E-postadan gelen linkin sonundaki "?token=..." kısmını otomatik yakalıyoruz
    this.route.queryParams.subscribe(params => {
      this.activationToken = params['token'] || '';
    });
  }

  setNewPassword() {
    if (!this.password || !this.confirmPassword) {
      alert('Lütfen tüm alanları doldurun.');
      return;
    }

    if (this.password !== this.confirmPassword) {
      alert('Girdiğiniz şifreler birbiriyle uyuşmuyor!');
      return;
    }
    if (!this.activationToken && !this.currentPassword) { alert('Geçici şifrenizi girin.'); return; }

    this.loading = true;

    // YANLIŞ: this.http.post('https://localhost:7066/api/Auth/ChangePassword' ...)
    // DOĞRU: Artık direkt ActivateAccount servisine gidiyoruz ve token'ı yolluyoruz!
    const request = this.activationToken
      ? this.http.post('https://localhost:7066/api/Auth/ActivateAccount', { token: this.activationToken, password: this.password })
      : this.http.post('https://localhost:7066/api/Auth/ChangePassword', { currentPassword: this.currentPassword, newPassword: this.password });
    request.subscribe({
      next: () => {
        alert('Hesabınız başarıyla aktifleştirildi! Yeni şifrenizle giriş yapabilirsiniz.');
        this.router.navigate(['/login']);
      },
      error: (err) => {
        console.error('Aktivasyon hatası:', err);
        const errMsg = err.error?.message || err.error || 'Aktivasyon güncellenemedi.';
        alert('İşlem başarısız: ' + errMsg);
        this.loading = false;
      }
    });
  }
}
