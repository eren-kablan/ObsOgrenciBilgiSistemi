import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { forkJoin } from 'rxjs';

interface TranscriptCourse {
  dersKodu: string; dersAdi: string; akts: number; sinif: number; donem: string; harfNotu: string | null;
}
interface TranscriptGradeGroup { sinif: number; courses: TranscriptCourse[]; totalAkts: number; }
interface StudentProfile {
  adi?: string; ad?: string; soyadi?: string; soyad?: string; ogrenciNumarasi?: string;
  sinif?: number; bolumAdi?: string; bolum?: { adi?: string; ad?: string };
}

@Component({
  selector: 'app-ogrenci-notlar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './ogrenci-notlar.component.html',
  styleUrls: ['./ogrenci-notlar.component.css']
})
export class OgrenciNotlarComponent implements OnInit {
  // Transkript verileri
  courses: TranscriptCourse[] = [];
  profile: StudentProfile | null = null;
  gano = 0;
  calculatedAkts = 0;
  loading = true;
  documentDate = new Date();
  academicYear = '';

  constructor(private http: HttpClient, private router: Router) {}

  // Transkript ekranı başlangıcı
  ngOnInit(): void {
    const email = sessionStorage.getItem('userEmail') || '';
    if (!email) { this.loading = false; return; }
    this.academicYear = this.calculateAcademicYear();
    const encodedEmail = encodeURIComponent(email);
    forkJoin({
      profile: this.http.get<StudentProfile>(`https://localhost:7066/api/Students/profile?email=${encodedEmail}`),
      courses: this.http.get<any[]>(`https://localhost:7066/api/Notlar/OgrenciNotlari?email=${encodedEmail}`),
      summary: this.http.get<any>(`https://localhost:7066/api/Transkript/Ozet?email=${encodedEmail}`)
    }).subscribe({
      next: result => {
        this.profile = result.profile;
        this.courses = (result.courses || []).map(course => ({
          dersKodu: course.dersKodu || course.DersKodu,
          dersAdi: course.dersAdi || course.DersAdi,
          akts: Number(course.akts ?? course.Akts ?? 0),
          sinif: Number(course.sinif ?? course.Sinif ?? 0),
          donem: this.formatTerm(course.donem ?? course.Donem),
          harfNotu: course.harfNotu || course.HarfNotu || null
        }));
        this.gano = Number(result.summary?.gano) || 0;
        this.calculatedAkts = Number(result.summary?.hesaplananAkts) || 0;
        this.loading = false;
      },
      error: error => { console.error('Transkript bilgileri yüklenemedi:', error); this.loading = false; }
    });
  }

  // Öğrenci ve not özetleri
  get fullName(): string {
    return `${this.profile?.adi || this.profile?.ad || ''} ${this.profile?.soyadi || this.profile?.soyad || ''}`.trim();
  }
  get departmentName(): string {
    return this.profile?.bolumAdi || this.profile?.bolum?.adi || this.profile?.bolum?.ad || '-';
  }
  get totalAkts(): number { return this.courses.reduce((total, course) => total + course.akts, 0); }
  get gradeGroups(): TranscriptGradeGroup[] {
    const groups = new Map<number, TranscriptCourse[]>();
    for (const course of this.courses) {
      const grade = course.sinif > 0 ? course.sinif : 1;
      groups.set(grade, [...(groups.get(grade) || []), course]);
    }
    return [...groups.entries()].sort(([a], [b]) => a - b).map(([sinif, courses]) => ({
      sinif,
      courses: courses.sort((a, b) => a.donem.localeCompare(b.donem, 'tr') || a.dersKodu.localeCompare(b.dersKodu, 'tr')),
      totalAkts: courses.reduce((sum, course) => sum + course.akts, 0)
    }));
  }
  goBack(): void { this.router.navigate(['/anasayfa']); }
  printTranscript(): void { window.print(); }

  // Akademik dönem yardımcıları
  private calculateAcademicYear(): string {
    const now = new Date();
    const start = now.getMonth() >= 8 ? now.getFullYear() : now.getFullYear() - 1;
    return `${start} - ${start + 1}`;
  }

  private formatTerm(term: unknown): string {
    if (term === 0 || term === '0' || term === 'Guz') return 'Güz';
    if (term === 1 || term === '1' || term === 'Bahar') return 'Bahar';
    return String(term || '-');
  }
}
