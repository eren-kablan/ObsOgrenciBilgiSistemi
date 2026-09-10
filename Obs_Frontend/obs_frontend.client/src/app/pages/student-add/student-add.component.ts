import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-student-add',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="container" style="padding: 2rem; max-width: 600px; margin: 0 auto;">
      <h2>Yeni Öğrenci Ekle</h2>
      <form (ngSubmit)="saveStudent()">
        <div style="margin-bottom: 1rem;">
          <label>Ad:</label>
          <input type="text" [(ngModel)]="student.adi" name="adi" class="form-control" style="width: 100%; padding: 0.5rem;" required />
        </div>
        <div style="margin-bottom: 1rem;">
          <label>Soyad:</label>
          <input type="text" [(ngModel)]="student.soyadi" name="soyadi" class="form-control" style="width: 100%; padding: 0.5rem;" required />
        </div>
        <div style="margin-bottom: 1rem;">
          <label>Öğrenci Numarası:</label>
          <input type="text" [(ngModel)]="student.ogrenciNumarasi" name="ogrenciNumarasi" class="form-control" style="width: 100%; padding: 0.5rem;" required />
        </div>
        <div style="margin-bottom: 1rem;">
          <label>Sınıf:</label>
          <select [(ngModel)]="student.sinif" name="sinif" class="form-control" style="width: 100%; padding: 0.5rem;" required>
            <option [ngValue]="1">1. Sınıf</option>
            <option [ngValue]="2">2. Sınıf</option>
            <option [ngValue]="3">3. Sınıf</option>
            <option [ngValue]="4">4. Sınıf</option>
          </select>
        </div>
        <div style="margin-bottom: 1rem;">
          <label>Bölüm ID:</label>
          <input type="number" [(ngModel)]="student.bolumId" name="bolumId" class="form-control" style="width: 100%; padding: 0.5rem;" required />
        </div>
        <button type="submit" style="background: #2563eb; color: white; border: none; padding: 0.7rem 1.5rem; border-radius: 6px; cursor: pointer;">Öğrenciyi Kaydet ve Mail Gönder</button>
        <button type="button" (click)="goBack()" style="background: #64748b; color: white; border: none; padding: 0.7rem 1.5rem; border-radius: 6px; cursor: pointer; margin-left: 0.5rem;">İptal</button>
      </form>
    </div>
  `
})
export class StudentAddComponent {
  student = {
    adi: '',
    soyadi: '',
    ogrenciNumarasi: '',
    bolumId: 1,
    sinif: 1
  };

  constructor(private http: HttpClient, private router: Router) { }

  saveStudent() {
    this.http.post(`${environment.apiBaseUrl}/Students`, this.student).subscribe({
      next: () => {
        alert('Öğrenci başarıyla eklendi!');
        this.router.navigate(['/admin-dashboard']);
      },
      error: (err) => {
        console.error(err);
        alert('Öğrenci eklenirken hata oluştu.');
      }
    });
  }

  goBack() {
    this.router.navigate(['/admin-dashboard']);
  }
}
