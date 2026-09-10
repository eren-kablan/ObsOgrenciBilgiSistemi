import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

interface AttendanceCourse {
  id: number;
  dersKodu: string;
  dersAdi: string;
  akts: number;
  islenenHafta: number;
  devamsizHafta: number;
}

interface AttendanceWeek {
  hafta: number;
  durum: 'Katildi' | 'Gelmedi' | 'Islenmedi';
  tarih: string | null;
}

@Component({
  selector: 'app-devamsizlik',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './devamsizlik.component.html',
  styleUrls: ['./devamsizlik.component.css']
})
export class DevamsizlikComponent implements OnInit {
  courses: AttendanceCourse[] = [];
  selectedCourse: AttendanceCourse | null = null;
  weeks: AttendanceWeek[] = [];

  constructor(private http: HttpClient, private router: Router) {}

  ngOnInit(): void {
    this.http.get<AttendanceCourse[]>('https://localhost:7066/api/Devamsizliklar/ogrenci-dersleri')
      .subscribe({
        next: data => this.courses = data || [],
        error: error => console.error('Dersler yüklenemedi', error)
      });
  }

  selectCourse(course: AttendanceCourse): void {
    this.selectedCourse = course;
    this.weeks = [];
    this.http.get<AttendanceWeek[]>(`https://localhost:7066/api/Devamsizliklar/ogrenci-ders/${course.id}`)
      .subscribe({
        next: data => this.weeks = data || [],
        error: error => console.error('Haftalar yüklenemedi', error)
      });
  }

  get processedWeekCount(): number { return this.weeks.filter(x => x.durum !== 'Islenmedi').length; }
  get presentWeekCount(): number { return this.weeks.filter(x => x.durum === 'Katildi').length; }
  get absentWeekCount(): number { return this.weeks.filter(x => x.durum === 'Gelmedi').length; }

  goBack(): void { this.router.navigate(['/anasayfa']); }
}
