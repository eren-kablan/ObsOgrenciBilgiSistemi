import { Component, ViewEncapsulation } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  // Login ekranı
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  encapsulation: ViewEncapsulation.None,
  template: `
    <div class="login-split-container">
      <!-- Sol Taraf: Logo ve Slogan -->
      <div class="left-hero">
        <div class="hero-bg-overlay"></div>
        <div class="hero-content-center">
          <img src="assets/duzce-logo.png" alt="Düzce Üniversitesi Logo" class="hero-logo" />
          <h1>Öğrenci Bilgi Sistemi</h1>
          <div class="hero-line"></div>
          <p class="slogan-text">"Değer Üreten Üniversite"</p>
        </div>
      </div>

      <!-- Sağ Taraf: Kullanıcı Girişi Kartı -->
      <div class="right-section">
        <div class="login-card">
          <div class="card-header">
            <img src="assets/duzce-logo.png" alt="Düzce Üniversitesi Logo" class="card-logo" />
            <h2 class="portal-title">Kullanıcı Girişi</h2>
            <div class="header-line"></div>
          </div>

          <form class="login-form" (ngSubmit)="login()">
            <div class="form-group">
              <div class="input-wrapper">
                <i class="pi pi-envelope input-icon"></i>
                <input
                  type="email"
                  id="email"
                  name="email"
                  [(ngModel)]="email"
                  placeholder="E-posta adresi"
                  required
                />
              </div>
            </div>

            <div class="form-group">
              <div class="input-wrapper">
                <i class="pi pi-lock input-icon"></i>
                <input
                  [type]="showPassword ? 'text' : 'password'"
                  id="password"
                  name="password"
                  [(ngModel)]="password"
                  placeholder="Şifre"
                  required
                />
                <i
                  class="pi toggle-icon"
                  [ngClass]="showPassword ? 'pi-eye-slash' : 'pi-eye'"
                  (click)="togglePasswordVisibility()"
                ></i>
              </div>
              <div class="forgot-password">
                <a href="javascript:void(0)">Şifremi Unuttum?</a>
              </div>
            </div>

            <button type="submit" class="btn-login">
              <span>GİRİŞ YAP</span>
              <i class="pi pi-arrow-right"></i>
            </button>
          </form>

          <div class="divider-or">
            <span>veya</span>
          </div>

          <button type="button" class="btn-secondary" (click)="goToActivation()">
            <i class="pi pi-user-plus"></i>
            <span>Yeni Öğrenci / Şifre Aktivasyonu</span>
          </button>

          <div class="login-footer">
            <a href="javascript:void(0)">Sistem Durumu</a>
            <span class="dot">•</span>
            <a href="javascript:void(0)">Yardım</a>
            <span class="dot">•</span>
            <a href="javascript:void(0)">İletişim</a>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    html, body {
      margin: 0 !important;
      padding: 0 !important;
      width: 100% !important;
      height: 100% !important;
      overflow: hidden !important;
      font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
    }

    .login-split-container {
      position: fixed;
      top: 0;
      left: 0;
      width: 100vw;
      height: 100vh;
      display: flex;
      z-index: 9999;
      background: #ffffff;
    }

    .left-hero {
      width: 48%;
      height: 100%;
      background-image: url('/assets/campus-bg.jpg');
      background-size: cover;
      background-position: center;
      padding: 3.5rem 4rem;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      box-sizing: border-box;
      position: relative;
      color: #ffffff;
      border-top-right-radius: 60px;
      border-bottom-right-radius: 60px;
      box-shadow: 12px 0 35px rgba(0,0,0,0.18);
      overflow: hidden;
      text-align: center;
      z-index: 2;
    }

    .hero-bg-overlay {
      position: absolute;
      top: 0; left: 0; right: 0; bottom: 0;
      background: linear-gradient(135deg, rgba(29, 78, 216, 0.62) 0%, rgba(30, 58, 138, 0.75) 50%, rgba(15, 23, 42, 0.88) 100%);
      z-index: 1;
    }

    .hero-content-center {
      position: relative;
      z-index: 2;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
    }

    .hero-logo {
      max-height: 75px;
      object-fit: contain;
      filter: brightness(0) invert(1);
      margin-bottom: 1.5rem;
    }

    .hero-content-center h1 {
      font-size: 2.8rem;
      font-weight: 800;
      line-height: 1.2;
      margin: 0 0 1rem 0;
      color: #ffffff;
      letter-spacing: -0.5px;
      text-shadow: 0 4px 12px rgba(0,0,0,0.4);
    }

    .hero-line {
      width: 60px;
      height: 4px;
      background: #38bdf8;
      border-radius: 2px;
      margin-bottom: 1.25rem;
    }

    .slogan-text {
      font-size: 1.25rem;
      color: #7dd3fc;
      font-weight: 600;
      font-style: italic;
      margin: 0;
      letter-spacing: 0.5px;
      text-shadow: 0 2px 8px rgba(0,0,0,0.3);
    }

    .right-section {
      flex: 1;
      height: 100%;
      display: flex;
      align-items: center;
      justify-content: center;
      background-image: url('/assets/right-bg.jpg');
      background-size: cover;
      background-position: center;
      padding: 2rem;
      box-sizing: border-box;
      position: relative;
      z-index: 1;
    }

    .login-card {
      background: #ffffff;
      border-radius: 24px;
      box-shadow: 0 25px 50px rgba(15, 23, 42, 0.12);
      padding: 2.8rem 2.5rem;
      width: 100%;
      max-width: 410px;
      box-sizing: border-box;
      border: 1px solid rgba(226, 232, 240, 0.8);
      position: relative;
      z-index: 2;
    }

    .card-header {
      text-align: center;
      margin-bottom: 1.8rem;
      display: flex;
      flex-direction: column;
      align-items: center;
    }

    .card-logo {
      max-height: 70px;
      object-fit: contain;
      margin-bottom: 0.75rem;
    }

    .portal-title {
      margin: 0;
      font-size: 1.2rem;
      color: #0f172a;
      font-weight: 800;
    }

    .header-line {
      width: 36px;
      height: 3px;
      background: #2563eb;
      border-radius: 2px;
      margin-top: 0.5rem;
    }

    .login-form {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }

    .input-wrapper {
      position: relative;
      display: flex;
      align-items: center;
    }

    .input-wrapper input {
      width: 100%;
      padding: 0.85rem 1rem 0.85rem 2.6rem;
      border: 1px solid #e2e8f0;
      background-color: #ffffff;
      border-radius: 12px;
      font-size: 0.88rem;
      color: #1e293b;
      outline: none;
      box-sizing: border-box;
      transition: all 0.2s;
    }

    .input-wrapper input::placeholder {
      color: #94a3b8;
    }

    .input-wrapper input:focus {
      border-color: #2563eb;
      box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.12);
    }

    .input-icon {
      position: absolute;
      left: 0.95rem;
      color: #94a3b8;
      font-size: 0.95rem;
    }

    .toggle-icon {
      position: absolute;
      right: 0.95rem;
      color: #94a3b8;
      font-size: 0.95rem;
      cursor: pointer;
    }

    .forgot-password {
      text-align: right;
      margin-top: 0.3rem;
    }

    .forgot-password a {
      font-size: 0.78rem;
      color: #2563eb;
      text-decoration: none;
      font-weight: 600;
    }

    .forgot-password a:hover {
      text-decoration: underline;
    }

    .btn-login {
      margin-top: 0.4rem;
      background: #2563eb;
      color: #ffffff;
      border: none;
      padding: 0.85rem;
      border-radius: 12px;
      font-size: 0.88rem;
      font-weight: 700;
      cursor: pointer;
      width: 100%;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 0.6rem;
      transition: background 0.2s, transform 0.1s;
    }

    .btn-login:hover {
      background: #1d4ed8;
    }

    .btn-login:active {
      transform: scale(0.99);
    }

    .divider-or {
      position: relative;
      text-align: center;
      margin: 1.25rem 0;
    }

    .divider-or::before {
      content: '';
      position: absolute;
      top: 50%;
      left: 0;
      right: 0;
      height: 1px;
      background: #e2e8f0;
    }

    .divider-or span {
      position: relative;
      background: #ffffff;
      padding: 0 0.75rem;
      font-size: 0.78rem;
      color: #94a3b8;
    }

    .btn-secondary {
      width: 100%;
      background: #ffffff;
      border: 1px solid #e2e8f0;
      color: #2563eb;
      padding: 0.8rem;
      border-radius: 12px;
      font-size: 0.85rem;
      font-weight: 700;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 0.5rem;
      transition: all 0.2s;
    }

    .btn-secondary:hover {
      background: #f8fafc;
      border-color: #cbd5e1;
    }

    .login-footer {
      margin-top: 1.8rem;
      text-align: center;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 0.4rem;
      font-size: 0.75rem;
    }

    .login-footer a {
      color: #64748b;
      text-decoration: none;
      font-weight: 500;
    }

    .login-footer a:hover {
      color: #2563eb;
    }

    .login-footer .dot {
      color: #cbd5e1;
    }
  `]
})
export class LoginComponent {
  // Form alanları
  email: string = '';
  password: string = '';
  showPassword: boolean = false;

  // Giriş işlemleri
  constructor(private authService: AuthService, private router: Router) { }

  togglePasswordVisibility() {
    this.showPassword = !this.showPassword;
  }

  goToActivation() {
    this.router.navigate(['/aktivasyon']);
  }

  login() {
    this.authService.login({ email: this.email, password: this.password }).subscribe({
      next: (response: any) => {
        const token = response.token || response.Token;
        const role = response.role || response.Role || '';
        const email = response.email || response.Email || this.email;
        const isFirstLogin = response.isFirstLogin ?? response.IsFirstLogin ?? false;

        const unvan = response.unvan || response.Unvan || '';
        const emailPrefix = email.split('@')[0];
        const formattedName = emailPrefix
          .split('.')
          .map((p: string) => p.charAt(0).toUpperCase() + p.slice(1))
          .join(' ');

        const rawName = response.ad ? `${response.ad} ${response.soyad}` : formattedName;
        const fullName = unvan ? `${unvan} ${rawName}` : rawName;

        sessionStorage.setItem('token', token);
        sessionStorage.setItem('userEmail', email);
        sessionStorage.setItem('role', role);
        sessionStorage.setItem('userName', fullName);
        sessionStorage.setItem('unvan', unvan);
        sessionStorage.setItem('userFaculty', response.fakulte || response.Fakulte || 'Mühendislik Fakültesi');
        sessionStorage.setItem('userDepartment', response.bolum || response.Bolum || 'Bilgisayar Mühendisliği');

        const normalizedRole = role.toLowerCase();

        // 1. ADMIN KONTROLÜ
        if (normalizedRole === 'admin' || email.includes('admin')) {
          this.router.navigate(['/admin-dashboard']);
          return;
        }

        // 2. İLK GİRİŞ / ŞİFRE DEĞİŞTİRME KONTROLÜ
        if (isFirstLogin) {
          alert('Sisteme ilk kez geçici şifrenizle giriş yaptınız. Güvenliğiniz için lütfen yeni şifrenizi belirleyiniz.');
          this.router.navigate(['/aktivasyon']);
          return;
        }

        // 3. ANA SAYFAYA YÖNLENDİRME
        this.router.navigate(['/anasayfa']);
      },
      error: (err: any) => {
        console.error('Giriş hatası:', err);
        alert('Giriş başarısız! E-posta veya şifre yanlış.');
      }
    });
  }
}
