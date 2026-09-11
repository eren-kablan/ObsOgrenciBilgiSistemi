import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-not-giris',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="page-shell">
      <div class="container">
        <div class="header-row">
          <div><span class="eyebrow">AKADEMİSYEN İŞLEMLERİ</span><h2>Not Girişi</h2><p class="subtitle">Dersi açarak öğrenci notlarını görüntüleyebilir ve güncelleyebilirsiniz.</p></div>
          <button class="btn-back" (click)="goBack()"><i class="pi pi-arrow-left"></i> Ana Sayfa</button>
        </div>
        <div class="list-summary"><span><i class="pi pi-book"></i> Verdiğiniz Dersler</span><strong>{{ dersler.length }} ders</strong></div>

        <div class="course-list" *ngIf="dersler.length; else emptyCourses">
          <section class="course-item" *ngFor="let ders of dersler" [class.expanded]="isSelected(ders)">
            <button class="course-trigger" (click)="dersSec(ders)">
              <div class="ders-icon"><i class="pi pi-book"></i></div>
              <div class="ders-info"><span>{{ ders.kod || ders.dersKodu }}</span><h3>{{ ders.ad || ders.adi }}</h3><small>Öğrenci listesi ve not girişi</small></div>
              <span class="open-label">{{ isSelected(ders) ? 'Kapat' : 'Dersi Aç' }}</span>
              <i class="pi toggle-icon" [ngClass]="isSelected(ders) ? 'pi-chevron-up' : 'pi-chevron-down'"></i>
            </button>

            <div class="course-content" *ngIf="isSelected(ders)">
              <div class="loading-state" *ngIf="loadingStudents"><i class="pi pi-spin pi-spinner"></i> Öğrenci listesi yükleniyor...</div>
              <div class="empty-students" *ngIf="!loadingStudents && !ogrenciler.length"><i class="pi pi-users"></i><span>Bu derse kayıtlı öğrenci bulunmuyor.</span></div>
              <div class="table-wrap" *ngIf="!loadingStudents && ogrenciler.length">
                <table class="obs-table">
                  <thead><tr><th>Öğrenci</th><th>Öğrenci Numarası</th><th>Vize (%40)</th><th>Final (%60)</th><th>Durum</th></tr></thead>
                  <tbody><tr *ngFor="let ogrenci of ogrenciler">
                    <td><strong>{{ ogrenci.ad }} {{ ogrenci.soyad }}</strong></td>
                    <td><span class="badge-no">{{ ogrenci.ogrenciNumarasi }}</span></td>
                    <td><input type="number" [(ngModel)]="ogrenci.vize" min="0" max="100" class="input-grade" placeholder="-" /></td>
                    <td><input type="number" [(ngModel)]="ogrenci.final" min="0" max="100" class="input-grade" placeholder="-" /></td>
                    <td><span class="status-ready"><i class="pi pi-check"></i> Hazır</span></td>
                  </tr></tbody>
                </table>
                <div class="footer-action"><span>{{ ogrenciler.length }} öğrenci listeleniyor</span><button class="btn-save-all" (click)="topluKaydet()" [disabled]="saving"><i class="pi pi-save"></i> {{ saving ? 'Kaydediliyor...' : 'Notları Kaydet' }}</button></div>
              </div>
            </div>
          </section>
        </div>
        <ng-template #emptyCourses><div class="empty-students page-empty"><i class="pi pi-book"></i><span>Üzerinize atanmış ders bulunmuyor.</span></div></ng-template>
      </div>
    </div>
  `,
  styles: [
    `:host { position: fixed; inset: 0; z-index: 20; display: block; overflow-y: auto; background: #f3f6fb; font-family: Inter, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif; }
    .page-shell { min-height: 100%; padding: 32px; box-sizing: border-box; }
    .container { max-width: 1120px; margin: 0 auto; background: #fff; padding: 2rem; border-radius: 20px; box-shadow: 0 12px 36px rgba(30,55,90,.08); border: 1px solid #e1e8f1; }
    .header-row { display: flex; justify-content: space-between; align-items: flex-start; gap: 1rem; padding-bottom: 1.5rem; border-bottom: 1px solid #e8edf4; }
    .eyebrow { color: #2563eb; font-size: .68rem; font-weight: 800; letter-spacing: 1.3px; }
    h2 { margin: .25rem 0 0; font-size: 1.65rem; color: #10203a; font-weight: 800; }
    .subtitle { color: #718096; font-size: .85rem; margin: .35rem 0 0; }
    .btn-back { background: #f1f5f9; border: none; padding: 0.5rem 1rem; border-radius: 8px; font-size: 0.82rem; font-weight: 600; cursor: pointer; color: #475569; display: flex; align-items: center; gap: 0.4rem; }
    .btn-back:hover { background: #e2e8f0; }
    .list-summary { display: flex; align-items: center; justify-content: space-between; margin: 1.5rem 0 .8rem; color: #334155; font-size: .82rem; font-weight: 700; }.list-summary span{display:flex;gap:.55rem;align-items:center}.list-summary i{color:#2563eb}.list-summary strong{padding:.3rem .65rem;border-radius:99px;background:#eaf2ff;color:#2563eb;font-size:.7rem}
    .course-list { display: grid; gap: .8rem; }.course-item{overflow:hidden;border:1px solid #dee6f0;border-radius:14px;background:#fff;transition:.2s}.course-item:hover{border-color:#b9cef0;box-shadow:0 5px 16px rgba(37,99,235,.06)}.course-item.expanded{border-color:#8db4f3;box-shadow:0 9px 24px rgba(37,99,235,.09)}
    .course-trigger{display:grid;grid-template-columns:48px minmax(0,1fr) auto 18px;align-items:center;gap:1rem;width:100%;padding:1rem 1.15rem;border:0;background:#fff;text-align:left;cursor:pointer;color:#14233d}.expanded .course-trigger{background:linear-gradient(90deg,#f4f8ff,#fff)}
    .ders-icon { display:grid;place-items:center;width:48px;height:48px;background:#e6f0ff;color:#2563eb;border-radius:12px;font-size:1.1rem}.ders-info span{color:#2563eb;font-size:.7rem;font-weight:800}.ders-info h3{margin:.15rem 0;font-size:.92rem}.ders-info small{color:#8491a3;font-size:.7rem}.open-label{color:#64748b;font-size:.72rem;font-weight:700}.toggle-icon{color:#2563eb;font-size:.75rem}
    .course-content{padding:0 1.15rem 1.15rem;border-top:1px solid #edf1f6;background:#fbfcfe}.table-wrap{overflow-x:auto;padding-top:1rem}
    .obs-table { width: 100%; border-collapse: collapse; text-align: left; font-size: 0.9rem; }
    .obs-table th { background: #f0f4f9; padding: 0.8rem 1rem; color: #64748b; font-size:.7rem;font-weight: 800; border-bottom: 1px solid #dce4ee; }
    .obs-table td { padding: 0.75rem 1rem; border-bottom: 1px solid #edf1f5; color: #1e293b; vertical-align: middle;font-size:.8rem }
    .badge-no { background: #f1f5f9; padding: 0.25rem 0.5rem; border-radius: 6px; font-weight: 600; font-size: 0.82rem; color: #334155; }
    .input-grade { width: 80px; padding: 0.5rem; border: 1px solid #cbd5e1; border-radius: 8px; text-align: center; font-weight: 600; font-size: 0.9rem; outline: none; }
    .input-grade:focus { border-color: #2563eb; box-shadow: 0 0 0 3px rgba(37,99,235,0.1); }
    .status-ready{display:inline-flex;align-items:center;gap:.3rem;padding:.25rem .5rem;border-radius:99px;background:#eaf8f1;color:#168657;font-size:.68rem;font-weight:700}.footer-action { margin-top: 1rem; display: flex;align-items:center;justify-content:space-between;color:#7b8899;font-size:.72rem }
    .btn-save-all { background: #2563eb; color: white; border: none; padding: 0.85rem 2rem; border-radius: 12px; font-weight: 700; font-size: 0.95rem; cursor: pointer; display: flex; align-items: center; gap: 0.5rem; transition: background 0.2s; }
    .btn-save-all:hover { background: #1d4ed8; }
    .btn-save-all:disabled { background: #94a3b8; cursor: not-allowed; }.loading-state,.empty-students{display:flex;align-items:center;justify-content:center;gap:.6rem;min-height:110px;color:#718096;font-size:.82rem}.empty-students i{font-size:1.2rem;color:#2563eb}.page-empty{border:1px dashed #cfdae8;border-radius:14px}

    /* Profesyonel açık tema */
    :host{background:#edf2f7}
    .page-shell{position:relative;padding:32px 38px;background:radial-gradient(circle at 96% 0,#dceaff 0,transparent 27%),linear-gradient(180deg,#f5f8fc 0,#edf2f7 100%)}
    .page-shell::before{content:"";position:fixed;inset:0 auto 0 0;width:5px;background:linear-gradient(#173d79,#2f72db)}
    .container{position:relative;width:100%;max-width:none;margin:0;box-sizing:border-box;padding:0 2.25rem 2rem;background:#fff;border-color:#dce4ee;box-shadow:0 16px 38px rgba(35,55,80,.08);overflow:hidden}
    .container::before{content:"";position:absolute;inset:0 0 auto;height:5px;background:linear-gradient(90deg,#173d79,#3478dd 38%,#8cb8fa)}
    .header-row{margin:0 -2.25rem;padding:2rem 2.25rem 1.65rem;border-color:#e3e9f1;background:linear-gradient(105deg,#fff 0,#f7faff 72%,#eef5ff);text-align:left}
    .eyebrow{color:#2465c5}h2{color:#12233d}.subtitle{color:#718096}
    .btn-back{border:1px solid #d7e1ed;background:#fff;color:#34455d;box-shadow:0 2px 7px rgba(31,49,75,.04)}.btn-back:hover{border-color:#8cb4ef;background:#f5f9ff;color:#1d5ebc}
    .list-summary{margin-top:1.3rem;color:#2f3e53}.list-summary i{color:#2465c5}.list-summary strong{background:#edf4ff;color:#2465c5;border:1px solid #dbe9ff}
    .course-list{counter-reset:course;gap:.7rem}.course-item{counter-increment:course;background:#fff;border-color:#dce4ed;border-left:4px solid #b8c9dd}.course-item:hover{border-color:#a8c2e5;border-left-color:#3478d4;box-shadow:0 7px 18px rgba(43,83,135,.08);transform:translateX(2px)}.course-item.expanded{border-color:#8cb2e8;border-left-color:#1d5fba;box-shadow:0 11px 25px rgba(36,101,197,.1)}
    .course-trigger{padding:1.05rem 1.2rem;background:#fff;color:#17263c}.expanded .course-trigger{background:linear-gradient(90deg,#f1f6fd,#fff 78%)}
    .ders-icon{border:1px solid #d5e3f6;background:#edf4fd;color:#1e5eaf;font-size:0}.ders-icon i{display:none}.ders-icon::before{content:counter(course,decimal-leading-zero);font-size:.78rem;font-weight:800;letter-spacing:.4px}
    .ders-info span,.toggle-icon{color:#2465c5}.ders-info h3{font-size:.95rem}.ders-info small,.open-label{color:#7b899b}.open-label{padding:.38rem .7rem;border:1px solid #dce5f0;border-radius:8px;background:#f8fafc}
    .course-content{border-color:#dce5ef;background:#f8fafc}.obs-table{overflow:hidden;border:1px solid #dce4ee;border-radius:10px;background:#fff}.obs-table th{background:#edf2f7;color:#536176}.obs-table td{background:#fff}.loading-state,.empty-students{color:#718096}.empty-students i{color:#3478d4}.page-empty{border-color:#cad6e4;background:#f8fafc}.footer-action{color:#7b8999}
    @media(max-width:700px){.page-shell{padding:12px}.container{padding:1rem}.header-row{display:grid}.course-trigger{grid-template-columns:42px 1fr 16px}.open-label{display:none}.ders-icon{width:42px;height:42px}.footer-action{align-items:flex-end;gap:1rem}}`
  ]
})
export class NotGirisComponent implements OnInit {
  dersler: any[] = [];
  selectedDers: any = null;
  ogrenciler: any[] = [];
  saving: boolean = false;
  loadingStudents: boolean = false;

  constructor(
    private http: HttpClient,
    private router: Router,
    private notification: NotificationService
  ) { }

  ngOnInit() {
    const email = sessionStorage.getItem('userEmail') || '';
    this.http.get<any[]>(`${environment.apiBaseUrl}/Notlar/AkademisyenDersleri?email=${email}`).subscribe({
      next: (res) => this.dersler = res,
      error: (err) => console.error('Dersler yüklenemedi:', err)
    });
  }

  goBack() {
    this.router.navigate(['/anasayfa']);
  }

  dersSec(ders: any) {
    if (this.isSelected(ders)) {
      this.selectedDers = null;
      this.ogrenciler = [];
      return;
    }
    this.selectedDers = ders;
    this.ogrenciler = [];
    this.loadingStudents = true;

    // Backend'den büyük harfle (Id) veya küçük harfle (id) gelmesine karşı garantiye alıyoruz
    const secilenDersId = ders.id || ders.Id;

    // Az önce backend tarafında (GetAllStudentsQueryHandler) filtreleme yazdığımız endpoint'e yönlendiriyoruz
    // (Eğer backend tarafında bu yazdığımız query'i farklı bir URL'ye atadıysan 'api/Students' kısmını ona göre değiştirebilirsin)
    this.http.get<any[]>(`${environment.apiBaseUrl}/Notlar/DersOgrencileri?dersId=${secilenDersId}`).subscribe({
      next: (res) => {
        // Backend'den gelen StudentDto (adi, soyadi) modelini bizim HTML tablomuzun
        // ve toplu kaydetme payload'umuzun beklediği formata (ad, soyad, ogrenciId) çevirerek mapliyoruz.
        this.ogrenciler = res.map(student => ({
          ogrenciId: student.ogrenciId || student.OgrenciId,
          ad: student.ad || student.Ad || student.adi || student.Adi || '',
          soyad: student.soyad || student.Soyad || student.soyadi || student.Soyadi || '',
          ogrenciNumarasi: student.ogrenciNumarasi || student.OgrenciNumarasi,
          vize: student.vize ?? null,
          final: student.final ?? null
        }));
        this.loadingStudents = false;
      },
      error: (err) => { this.loadingStudents = false; console.error('Öğrenciler yüklenemedi:', err); }
    });
  }

  isSelected(ders: any): boolean {
    return !!this.selectedDers && (this.selectedDers.id || this.selectedDers.Id) === (ders.id || ders.Id);
  }

  topluKaydet() {
    this.saving = true;
    const secilenDersId = this.selectedDers.id || this.selectedDers.Id;

    const payload = {
      dersId: secilenDersId,
      notlar: this.ogrenciler.map(o => ({
        ogrenciId: o.ogrenciId,
        ogrenciNumarasi: o.ogrenciNumarasi,
        vize: o.vize !== null && o.vize !== undefined ? Number(o.vize) : null,
        final: o.final !== null && o.final !== undefined ? Number(o.final) : null
      }))
    };

    this.http.post(`${environment.apiBaseUrl}/Notlar/TopluKaydet`, payload).subscribe({
      next: () => {
        this.notification.success('Tüm sınıfın notları başarıyla kaydedildi.');
        this.saving = false;
      },
      error: (err) => {
        console.error('Toplu kayıt hatası:', err);
        this.notification.error('Notlar kaydedilirken bir hata oluştu.');
        this.saving = false;
      }
    });
  }
}
