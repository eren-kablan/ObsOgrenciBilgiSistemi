import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CalendarDay } from '../../shared/calendar/calendar.models';
import { createCalendarView } from '../../shared/calendar/calendar.utils';
import { Announcement, CourseRequest, ExamResult } from './dashboard.models';
import { environment } from '../../../environments/environment';
import { NotificationService } from '../../services/notification.service';

const API_URL = environment.apiBaseUrl;

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dashboard.component.html',
  styleUrls: [
    './dashboard.layout.css',
    './dashboard.content.css',
    './dashboard.widgets.css',
    './dashboard.course-groups.css',
    './dashboard.exam-results.css',
    './dashboard.attendance-summary-v2.css',
    './dashboard.announcements.css'
  ]
})
export class DashboardComponent implements OnInit {
  userEmail: string = '';
  userName: string = 'Yükleniyor...';
  userDepartment: string = 'Yükleniyor...';
  userFaculty: string = 'Mühendislik Fakültesi';
  userUnvan: string = '';
  userGrade: string = '-';
  userInitials: string = '';
  gano: number = 0;
  isLecturer: boolean = false;

  academicYear: string = '';
  academicTerm: string = '';
  showNotifications: boolean = false;

  announcements: Announcement[] = [
    { date: '05 Ağustos 2026', title: 'Bütünleme Sınav Takvimi', description: 'Sınav salonları ve saat detayları fakülte panosunda ilan edilmiştir.', icon: 'pi pi-megaphone', iconClass: 'icon-blue' },
    { date: '28 Temmuz 2026', title: 'Staj Değerlendirme Sonuçları', description: 'Staj defteri kabul edilen öğrencilerin listesi açıklandı.', icon: 'pi pi-file-edit', iconClass: 'icon-green' },
    { date: '20 Temmuz 2026', title: 'Yaz Okulu Ders Kayıtları', description: 'Yaz okulu ders kayıtları 25 Temmuz\'a kadar uzatılmıştır.', icon: 'pi pi-bookmark', iconClass: 'icon-purple' }
  ];

  displayDate: Date = new Date();
  today: Date = new Date();
  calendarTitle: string = '';
  calendarDays: CalendarDay[] = [];

  // ================= YENİ EKLENEN DEĞİŞKENLER =================
  aktifSekme: string = 'menu'; // Ekranda hangi gridin görüneceğini tutar
  aktifDonem: number = 1; // 1. Dönem veya 2. Dönem bilgisini tutar
  toplamKredi: number = 0;
  tumDersler: any[] = [];
  donemDersleri: any[] = [];
  get courseGradeGroups(): number[] {
    const grade = Number(this.userGrade);
    return grade >= 1 && grade <= 4 ? [grade] : [];
  }
  readonly maxAkts = 40;
  registeredAkts = 0;
  examResults: ExamResult[] = [];
  secilenDersler: any[] = [];
  availableRequestCourses: any[] = [];
  myCourseRequests: CourseRequest[] = [];
  registrationRequests: any[] = [];
  registrationStatus: any = null;
  attendanceCourses: any[] = [];
  attendanceStudents: any[] = [];
  attendanceCourseId: number | null = null;
  attendanceWeek = 1;
  savingAttendance = false;
  readonly attendanceWeeks = Array.from({ length: 12 }, (_, index) => index + 1);
  studentAttendanceCourses: any[] = [];
  selectedStudentAttendanceCourse: any = null;
  studentAttendanceWeeks: any[] = [];
  loadingStudentAttendance = false;
  publishingLecturerAnnouncement = false;
  lecturerAnnouncement = { title: '', description: '' };
  // ============================================================

  constructor(
    private router: Router,
    private http: HttpClient,
    private notification: NotificationService
  ) { }

  ngOnInit() {
    this.userEmail = sessionStorage.getItem('userEmail') || '';
    const role = (sessionStorage.getItem('role') || '').toLowerCase();

    // Rol akademisyen mi kontrolü
    this.isLecturer = role === 'lecturer' || role === 'akademisyen';

    if (this.isLecturer) {
      this.userUnvan = sessionStorage.getItem('unvan') || '';
      const lecturerName = sessionStorage.getItem('userName') || this.formatFullName(this.userEmail.split('@')[0]);
      this.userName = this.stripAcademicTitle(lecturerName, this.userUnvan);
      this.userDepartment = sessionStorage.getItem('userDepartment') || 'Bilgisayar Mühendisliği';
      this.userFaculty = sessionStorage.getItem('userFaculty') || 'Mühendislik Fakültesi';
      this.userInitials = this.getInitials(this.userName);
    } else if (this.userEmail) {
      this.http.get<any>(`${API_URL}/Students/profile?email=${this.userEmail}`).subscribe({
        next: (profile) => {
          const name = profile.ad || profile.adi || '';
          const surname = profile.soyad || profile.soyadi || '';

          this.userName = `${name} ${surname}`.trim();
          const dept = profile.bolum?.adi || profile.bolum?.ad || profile.bolum?.name || profile.bolumAdi;
          this.userDepartment = dept || (profile.bolumId ? `Bölüm ID: ${profile.bolumId}` : 'Bilgisayar Mühendisliği');

          this.userGrade = profile.sinif ? `${profile.sinif}` : '1';
          this.userInitials = (name.charAt(0) + surname.charAt(0)).toUpperCase() || 'ÖĞ';
        },
        error: (err) => {
          console.error("Profil bilgileri yüklenemedi, varsayılanlar yükleniyor.", err);
          let rawName = sessionStorage.getItem('userName') || this.userEmail.split('@')[0];
          this.userName = this.formatFullName(rawName);
          this.userDepartment = sessionStorage.getItem('userDepartment') || 'Bilgisayar Mühendisliği';
          this.userGrade = sessionStorage.getItem('userGrade') || '1';
          this.userInitials = this.userName.slice(0, 2).toUpperCase();
        }
      });
    }

    this.calculateAcademicPeriod();
    this.generateCalendar();

    // YENİ: Gerçek API'den dersleri çekiyoruz
    if (!this.isLecturer) this.gercekDersleriGetir();
    this.duyurulariGetir();
    if (this.isLecturer) this.loadCourseRequestData();
    if (!this.isLecturer && this.userEmail) { this.ganoGetir(); this.loadRegistrationStatus(); }
  }

  // ================= YENİ EKLENEN METOTLAR =================

  duyurulariGetir() {
    this.http.get<any[]>(`${API_URL}/Duyurular`).subscribe({
      next: data => {
        const dismissedIds = this.getDismissedAnnouncementIds();
        const systemAnnouncements = (data || []).filter(item => !dismissedIds.has(Number(item.id || item.Id))).map(item => ({
          id: Number(item.id || item.Id),
          date: new Date(item.yayinTarihi || item.YayinTarihi || item.olusturulmaTarihi || item.OlusturulmaTarihi).toLocaleDateString('tr-TR'),
          title: item.baslik || item.Baslik,
          description: item.icerik || item.Icerik,
          icon: 'pi pi-megaphone', iconClass: 'icon-blue'
        }));
        this.announcements = [...this.announcements.filter(item => item.isPersonal), ...systemAnnouncements];
      },
      error: () => this.announcements = this.announcements.filter(item => item.isPersonal)
    });
    if (this.isLecturer) {
      this.http.get<any[]>(`${API_URL}/notifications`).subscribe({
        next: notifications => {
          const personal = (notifications || []).map(item => ({
            id: item.id,
            date: new Date(item.olusturulmaTarihi).toLocaleDateString('tr-TR'),
            title: item.baslik,
            description: item.mesaj,
            icon: item.okundu ? 'pi pi-check-circle' : 'pi pi-bell',
            iconClass: item.okundu ? 'icon-green' : 'icon-blue',
            isPersonal: true,
            unread: !item.okundu
          }));
          this.announcements = [...personal, ...this.announcements.filter(item => !item.isPersonal)];
        }
      });
    }
  }

  get notificationCount(): number {
    return this.announcements.filter(item => !item.isPersonal || item.unread).length;
  }

  dismissAnnouncement(item: Announcement, event: Event): void {
    event.stopPropagation();
    const removeFromList = () => this.announcements = this.announcements.filter(announcement => announcement !== item);
    if (item.isPersonal && item.id) {
      this.http.delete(`${API_URL}/notifications/${item.id}`).subscribe({
        next: removeFromList,
        error: error => this.notification.error(error?.error?.message || 'Bildirim silinemedi.')
      });
      return;
    }

    if (item.id) {
      const dismissedIds = this.getDismissedAnnouncementIds();
      dismissedIds.add(Number(item.id));
      sessionStorage.setItem(this.dismissedAnnouncementStorageKey, JSON.stringify([...dismissedIds]));
    }
    removeFromList();
  }

  private get dismissedAnnouncementStorageKey(): string { return `dismissedAnnouncements:${this.userEmail}`; }

  private getDismissedAnnouncementIds(): Set<number> {
    try {
      const ids = JSON.parse(sessionStorage.getItem(this.dismissedAnnouncementStorageKey) || '[]');
      return new Set<number>((Array.isArray(ids) ? ids : []).map(Number));
    } catch {
      return new Set<number>();
    }
  }

  openAnnouncementPublisher(): void { this.aktifSekme = 'announcementPublish'; }

  publishLecturerAnnouncement(): void {
    const title = this.lecturerAnnouncement.title.trim();
    const description = this.lecturerAnnouncement.description.trim();
    if (!title || !description || this.publishingLecturerAnnouncement) return;
    this.publishingLecturerAnnouncement = true;
    this.http.post(`${API_URL}/Duyurular`, { baslik: title, icerik: description, hedefRol: 'Student', yayinda: true }).subscribe({
      next: () => {
        this.publishingLecturerAnnouncement = false;
        this.lecturerAnnouncement = { title: '', description: '' };
        this.notification.success('Duyuru öğrencilere başarıyla yayınlandı.');
        this.anaMenuyeDon();
      },
      error: error => {
        this.publishingLecturerAnnouncement = false;
        this.notification.error(error?.error?.message || 'Duyuru yayınlanamadı.');
      }
    });
  }

  openCourseRequests(): void {
    this.aktifSekme = 'courseRequests';
    this.loadCourseRequestData();
  }

  loadCourseRequestData(): void {
    this.http.get<any[]>(`${API_URL}/course-requests/available`).subscribe({
      next: courses => this.availableRequestCourses = courses || [],
      error: error => console.error('Talep edilebilir dersler yüklenemedi:', error)
    });
    this.http.get<CourseRequest[]>(`${API_URL}/course-requests/mine`).subscribe({
      next: requests => this.myCourseRequests = requests || [],
      error: error => console.error('Ders talepleri yüklenemedi:', error)
    });
  }

  requestCourse(course: any): void {
    this.http.post(`${API_URL}/course-requests`, { courseId: course.id }).subscribe({
      next: () => { this.notification.success('Ders talebiniz yönetici onayına gönderildi.'); this.loadCourseRequestData(); },
      error: error => this.notification.error(error?.error?.message || 'Ders talebi oluşturulamadı.')
    });
  }

  gercekDersleriGetir() {
    this.http.get<any[]>(`${API_URL}/Courses/registration-options?term=${this.aktifDonem}`).subscribe({
      next: (res) => {
        // Backend'den gelen isimlendirmeler ne olursa olsun bizim tabloya uyduruyoruz
        this.tumDersler = res.map(ders => ({
          id: ders.id || ders.Id,
          kod: ders.dersKodu || ders.kod || ders.Kod,
          ad: ders.dersAdi || ders.adi || ders.ad || ders.Adi || ders.Ad,
          kredi: ders.kredi ?? ders.Kredi ?? 0,
          akts: ders.akts ?? ders.Akts ?? 0,
          akademisyenAdi: ders.akademisyenAdi || ders.AkademisyenAdi || null,
          donem: ders.donem ?? ders.Donem ?? null,
          sinif: ders.sinif ?? ders.Sinif ?? 0,
          bolumId: ders.bolumId ?? ders.BolumId ?? null
        }));
      },
      error: (err) => console.error('Gerçek dersler veritabanından çekilemedi:', err)
    });
  }

  ganoGetir() {
    this.http.get<any>(`${API_URL}/Transkript/Ozet?email=${this.userEmail}`).subscribe({
      next: result => this.gano = Number(result.gano) || 0,
      error: err => console.error('GANO özeti yüklenemedi:', err)
    });
  }

  dersKaydiEkraniniAc() {
    if (this.registrationStatus?.durum === 'Bekliyor') return void this.notification.info(`Ders kaydınız danışman onayında bekliyor. Danışman: ${this.registrationStatus.danisman}`);
    if (this.registrationStatus?.durum === 'Onaylandi') return void this.notification.info('Bu dönem ders kaydınız onaylandı. Yeniden ders kaydı yapamazsınız.');
    this.aktifSekme = 'dersKaydi';
    this.loadExamResults();

    const studentGrade = Number(this.userGrade);
    this.donemDersleri = this.tumDersler.filter(d =>
      d.donem === this.aktifDonem && d.sinif === studentGrade
    );
  }

  loadRegistrationStatus(): void {
    this.http.get<any>(`${API_URL}/course-registration-requests/status?academicYear=${encodeURIComponent(this.academicYear)}&term=${this.aktifDonem}`).subscribe({
      next: status => this.registrationStatus = status,
      error: error => console.error('Ders kayıt durumu alınamadı:', error)
    });
  }

  openRegistrationApprovals(): void {
    this.aktifSekme = 'registrationApprovals';
    this.loadRegistrationApprovals();
  }

  loadRegistrationApprovals(): void {
    this.http.get<any[]>(`${API_URL}/course-registration-requests/pending`).subscribe({
      next: requests => this.registrationRequests = requests || [],
      error: error => this.notification.error(error?.error?.message || 'Bekleyen ders kayıtları yüklenemedi.')
    });
  }

  async decideRegistration(request: any, approve: boolean): Promise<void> {
    let body: any = {};
    if (!approve) {
      const reason = await this.notification.prompt(
        `${request.ogrenciAdi} için ret gerekçesini yazın.`,
        'Ders kaydını reddet',
        'Ret gerekçesi'
      );
      if (!reason) return;
      body = { reason };
    } else {
      const confirmed = await this.notification.confirm(
        `${request.ogrenciAdi} öğrencisinin seçtiği ${request.dersler.length} dersi onaylıyor musunuz?`,
        'Ders kaydını onayla'
      );
      if (!confirmed) return;
    }
    this.http.post(`${API_URL}/course-registration-requests/${request.id}/${approve ? 'approve' : 'reject'}`, body).subscribe({
      next: () => { this.notification.success(approve ? 'Öğrencinin ders kaydı onaylandı.' : 'Ders kaydı gerekçesiyle reddedildi.'); this.loadRegistrationApprovals(); },
      error: error => this.notification.error(error?.error?.message || 'Ders kayıt kararı kaydedilemedi.')
    });
  }

  openAttendanceEntry(): void {
    this.aktifSekme = 'attendanceEntry';
    this.attendanceCourseId = null;
    this.attendanceStudents = [];
    this.http.get<any[]>(`${API_URL}/Devamsizliklar/akademisyen-dersleri`).subscribe({
      next: courses => this.attendanceCourses = courses || [],
      error: error => this.notification.error(error?.error?.message || 'Dersleriniz yüklenemedi.')
    });
  }

  openStudentAttendance(): void {
    this.aktifSekme = 'attendanceSummary';
    this.selectedStudentAttendanceCourse = null;
    this.studentAttendanceWeeks = [];
    this.loadingStudentAttendance = true;
    this.http.get<any[]>(`${API_URL}/Devamsizliklar/ogrenci-dersleri`).subscribe({
      next: courses => {
        this.studentAttendanceCourses = courses || [];
        this.loadingStudentAttendance = false;
        if (this.studentAttendanceCourses.length > 0) this.selectStudentAttendanceCourse(this.studentAttendanceCourses[0]);
      },
      error: error => {
        this.loadingStudentAttendance = false;
        this.studentAttendanceCourses = [];
        this.notification.error(error?.error?.message || 'Devamsızlık bilgileri yüklenemedi.');
      }
    });
  }

  selectStudentAttendanceCourse(course: any): void {
    this.selectedStudentAttendanceCourse = course;
    this.studentAttendanceWeeks = [];
    this.loadingStudentAttendance = true;
    this.http.get<any[]>(`${API_URL}/Devamsizliklar/ogrenci-ders/${course.id}`).subscribe({
      next: weeks => {
        this.studentAttendanceWeeks = weeks || [];
        this.loadingStudentAttendance = false;
      },
      error: error => {
        this.loadingStudentAttendance = false;
        this.notification.error(error?.error?.message || 'Haftalık devamsızlık bilgileri yüklenemedi.');
      }
    });
  }

  get studentProcessedWeekCount(): number { return this.studentAttendanceWeeks.filter(week => week.durum !== 'Islenmedi').length; }
  get studentPresentWeekCount(): number { return this.studentAttendanceWeeks.filter(week => week.durum === 'Katildi').length; }
  get studentAbsentWeekCount(): number { return this.studentAttendanceWeeks.filter(week => week.durum === 'Gelmedi').length; }

  selectAttendanceCourse(courseId: number): void {
    this.attendanceCourseId = courseId;
    this.attendanceWeek = 1;
    this.loadAttendanceWeek();
  }

  selectAttendanceWeek(week: number): void {
    this.attendanceWeek = week;
    if (this.attendanceCourseId) this.loadAttendanceWeek();
  }

  loadAttendanceWeek(): void {
    if (!this.attendanceCourseId) return;
    this.http.get<any[]>(`${API_URL}/Devamsizliklar/ders/${this.attendanceCourseId}/hafta/${this.attendanceWeek}`).subscribe({
      next: students => this.attendanceStudents = (students || []).map(student => ({ ...student, katildi: student.katildi !== false })),
      error: error => this.notification.error(error?.error?.message || 'Öğrenci yoklaması yüklenemedi.')
    });
  }

  toggleAttendance(student: any): void { student.katildi = !student.katildi; }

  get presentAttendanceCount(): number { return this.attendanceStudents.filter(student => student.katildi).length; }
  get absentAttendanceCount(): number { return this.attendanceStudents.length - this.presentAttendanceCount; }

  saveAttendanceWeek(): void {
    if (!this.attendanceCourseId || this.savingAttendance) return;
    this.savingAttendance = true;
    const absentStudentIds = this.attendanceStudents.filter(student => !student.katildi).map(student => student.id);
    this.http.put<any>(`${API_URL}/Devamsizliklar/hafta`, {
      courseId: this.attendanceCourseId, week: this.attendanceWeek, absentStudentIds
    }).subscribe({
      next: response => {
        this.savingAttendance = false;
        this.notification.success(response.message || 'Devamsızlık kaydedildi.');
      },
      error: error => {
        this.savingAttendance = false;
        this.notification.error(error?.error?.message || 'Devamsızlık kaydedilemedi.');
      }
    });
  }

  anaMenuyeDon() {
    this.aktifSekme = 'menu';
  }

  dersEkle(ders: any) {
    if (this.registeredAkts + this.selectedAkts + ders.akts > this.maxAkts) {
      this.notification.warning(`Bu dersle birlikte 40 AKTS sınırını aşıyorsunuz. Kalan hakkınız: ${this.maxAkts - this.registeredAkts - this.selectedAkts} AKTS.`);
      return;
    }
    if (!this.secilenDersler.find(d => d.id === ders.id)) {
      this.secilenDersler.push(ders);
      this.krediHesapla();
    }
  }

  get selectedAkts(): number {
    return this.secilenDersler.reduce((total, course) => total + Number(course.akts || 0), 0);
  }

  get totalPlannedAkts(): number {
    return this.registeredAkts + this.selectedAkts;
  }

  canSelectCourse(course: any): boolean {
    return !this.isCourseSelected(course.id)
      && this.totalPlannedAkts + Number(course.akts || 0) <= this.maxAkts;
  }

  openExamResults(): void {
    this.aktifSekme = 'examResults';
    this.loadExamResults();
  }

  openMyCourses(): void {
    this.aktifSekme = 'myCourses';
    this.loadExamResults();
  }

  loadExamResults(): void {
    if (!this.userEmail || this.isLecturer) return;
    this.http.get<any[]>(`${API_URL}/Notlar/OgrenciNotlari?email=${encodeURIComponent(this.userEmail)}`).subscribe({
      next: results => {
        this.examResults = (results || []).map(item => ({
          dersKodu: item.dersKodu || item.DersKodu,
          dersAdi: item.dersAdi || item.DersAdi,
          akts: Number(item.akts ?? item.Akts ?? 0),
          vize: item.vize ?? item.Vize ?? null,
          final: item.final ?? item.Final ?? null,
          ortalama: item.ortalama ?? item.Ortalama ?? null,
          harfNotu: item.harfNotu || item.HarfNotu || null
          ,sinif: Number(item.sinif ?? item.Sinif ?? 0)
          ,donem: this.formatCourseTerm(item.donem ?? item.Donem)
        }));
        this.registeredAkts = this.examResults.reduce((total, result) => total + result.akts, 0);
      },
      error: error => console.error('Sınav sonuçları yüklenemedi:', error)
    });
  }

  private formatCourseTerm(term: unknown): string {
    if (term === 0 || term === '0' || term === 'Guz') return 'Güz';
    if (term === 1 || term === '1' || term === 'Bahar') return 'Bahar';
    return String(term || '-');
  }

  coursesByGrade(grade: number): any[] {
    return this.donemDersleri.filter(course => course.sinif === grade);
  }

  requestCoursesByGrade(grade: number): any[] {
    return this.availableRequestCourses.filter(course => course.sinif === grade);
  }

  isCourseSelected(courseId: number): boolean {
    return this.secilenDersler.some(course => course.id === courseId);
  }

  dersCikar(ders: any) {
    this.secilenDersler = this.secilenDersler.filter(d => d.id !== ders.id);
    this.krediHesapla();
  }

  krediHesapla() {
    this.toplamKredi = this.secilenDersler.reduce((acc, curr) => acc + curr.kredi, 0);
  }

  kaydiOnayla() {
    if (this.secilenDersler.length === 0) {
      this.notification.warning('Lütfen en az bir ders seçin.');
      return;
    }
    if (this.totalPlannedAkts > this.maxAkts) {
      this.notification.warning('Toplam ders kaydınız 40 AKTS sınırını aşamaz.');
      return;
    }

    this.http.post<any>(`${API_URL}/course-registration-requests`, {
      courseIds: this.secilenDersler.map(course => course.id), academicYear: this.academicYear, term: this.aktifDonem
    }).subscribe({
      next: response => {
        this.notification.success(response.message || 'Ders seçiminiz danışman onayına gönderildi.');
        this.secilenDersler = []; this.toplamKredi = 0; this.loadRegistrationStatus(); this.anaMenuyeDon();
      },
      error: error => this.notification.error(error?.error?.message || 'Ders seçiminiz danışman onayına gönderilemedi.')
    });
  }
  // ==========================================================

  navigateTo(path: string) {
    this.router.navigate([path]);
  }

  getInitials(fullName: string): string {
    if (!fullName) return 'AK';
    const parts = fullName.trim().split(' ');
    if (parts.length >= 2) {
      return (parts[parts.length - 2].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase();
    }
    return fullName.slice(0, 2).toUpperCase();
  }

  formatFullName(input: string): string {
    if (!input) return '';
    return input
      .replace(/[._-]/g, ' ')
      .split(' ')
      .filter(part => part.length > 0)
      .map(part => part.charAt(0).toUpperCase() + part.slice(1).toLowerCase())
      .join(' ');
  }

  private stripAcademicTitle(fullName: string, title: string): string {
    let cleanName = (fullName || '').trim();
    if (title) {
      const escapedTitle = title.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
      cleanName = cleanName.replace(new RegExp(`^${escapedTitle}\\s*`, 'i'), '');
    }

    return cleanName
      .replace(/^(Prof\.?\s*Dr\.?|Doç\.?\s*Dr\.?|Dr\.?\s*Öğr\.?\s*Üyesi|Öğr\.?\s*Gör\.?|Arş\.?\s*Gör\.?)\s*/i, '')
      .trim();
  }

  toggleNotifications() {
    this.showNotifications = !this.showNotifications;
    if (this.showNotifications) {
      for (const item of this.announcements.filter(item => item.isPersonal && item.unread && item.id)) {
        this.http.post(`${API_URL}/notifications/${item.id}/read`, {}).subscribe();
        item.unread = false;
      }
    }
  }

  calculateAcademicPeriod() {
    const now = new Date();
    const year = now.getFullYear();
    const month = now.getMonth() + 1;

    // Eylül (9. ay) ve sonrasını Güz (1. Dönem) kabul ediyoruz
    if (month >= 9) {
      this.academicYear = `${year} - ${year + 1}`;
      this.academicTerm = 'Güz Dönemi';
      this.aktifDonem = 1;
    } else {
      this.academicYear = `${year - 1} - ${year}`;
      this.academicTerm = 'Bahar Dönemi';
      this.aktifDonem = 2;
    }
  }

  generateCalendar() {
    const calendar = createCalendarView(this.displayDate, this.today);
    this.calendarTitle = calendar.title;
    this.calendarDays = calendar.days;
  }

  prevMonth() {
    this.displayDate = new Date(this.displayDate.getFullYear(), this.displayDate.getMonth() - 1, 1);
    this.generateCalendar();
  }

  nextMonth() {
    this.displayDate = new Date(this.displayDate.getFullYear(), this.displayDate.getMonth() + 1, 1);
    this.generateCalendar();
  }

  logout() {
    sessionStorage.clear();
    this.router.navigate(['/login']);
  }
}
