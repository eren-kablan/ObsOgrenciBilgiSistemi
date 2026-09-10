import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CalendarDay } from '../../shared/calendar/calendar.models';
import { createCalendarView } from '../../shared/calendar/calendar.utils';
import { ActiveView, AdminNotification, AdvisorDepartment, Announcement, CourseRequest, Department, Lecturer, Student } from './admin-dashboard.models';
import { environment } from '../../../environments/environment';

const API_URL = environment.apiBaseUrl;

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrls: [
    './admin-dashboard.layout.css',
    './admin-dashboard.content.css',
    './admin-dashboard.widgets.css',
    './admin-dashboard.course-groups.css'
  ]
})
export class AdminDashboardComponent implements OnInit {
  // Panel görünümü ve açılır menüler
  activeView: ActiveView = 'dashboard';
  isStudentMenuOpen = false;
  isLecturerMenuOpen = false;
  loadingStudent = false;
  loadingLecturer = false;
  loadingAnnouncement = false;
  loadingDepartment = false;
  loadingCurriculumImport = false;
  showAdminDropdown = false;
  showNotifications = false;
  loadingNotifications = false;
  notifications: AdminNotification[] = [];

  // Öğrenci ve akademisyen verileri
  targetStudentId?: number;
  targetLecturerId?: number;
  selectedStudentDepartmentFilter: number | null = null;
  selectedDepartmentFilter: number | null = null;
  selectedCourseDepartmentFilter: number | null = null;
  studentList: Student[] = [];
  lecturerList: Lecturer[] = [];
  tumDersler: any[] = [];
  tumAkademisyenler: Lecturer[] = [];
  secilenDersId: number | null = null;
  secilenAkademisyenEmail: string | null = null;
  secilenAtamaBolumId: number | null = null;
  filtrelenmisDersler: any[] = [];
  pendingCourseRequests: CourseRequest[] = [];
  advisorDepartments: AdvisorDepartment[] = [];
  readonly courseGradeGroups = [1, 2, 3, 4];
  newCourse = { adi: '', bolumId: 1, sinif: 1, donem: 1, akts: 5 };
  departments: Department[] = [];
  newDepartmentName = '';
  student: Student = { adi: '', soyadi: '', bolumId: 1, sinif: 1 };
  editStudent: Student | null = null;
  lecturer: Lecturer = { unvani: 'Prof. Dr.', adi: '', soyadi: '', email: '', bolumId: 1 };
  editLecturer: Lecturer | null = null;
  stats = { totalStudents: 0, totalDepartments: 12 };
  newAnnouncement = { title: '', description: '' };
  announcements: Announcement[] = [
    { title: '2026 Bütünleme Sınav Takvimi', description: 'Sınav salonları fakülte panolarına asılmıştır.', date: '07 Ağustos 2026' }
  ];
  displayDate = new Date();
  today = new Date();
  calendarTitle = '';
  calendarDays: CalendarDay[] = [];
  todayDateFormatted = '';

  // Sayfa başlangıcı
  constructor(private router: Router, private http: HttpClient) {}

  ngOnInit(): void {
    this.generateCalendar();
    this.fetchDepartments();
    this.fetchStats();
    this.fetchDerslerVeHocalar();
    this.fetchAnnouncements();
    this.fetchNotifications();
    this.todayDateFormatted = this.today.toLocaleDateString('tr-TR', { day: 'numeric', month: 'long', year: 'numeric' });
  }

  // Bildirim işlemleri
  get unreadNotificationCount(): number {
    return this.notifications.filter(item => !item.okundu).length;
  }

  toggleNotifications(): void {
    this.showNotifications = !this.showNotifications;
    this.showAdminDropdown = false;
    if (this.showNotifications) this.fetchNotifications();
  }

  fetchNotifications(): void {
    this.loadingNotifications = true;
    this.http.get<AdminNotification[]>(`${API_URL}/notifications`, { headers: this.getAuthHeaders() }).subscribe({
      next: notifications => {
        this.notifications = notifications || [];
        this.loadingNotifications = false;
      },
      error: () => {
        this.notifications = [];
        this.loadingNotifications = false;
      }
    });
  }

  markNotificationAsRead(notification: AdminNotification): void {
    if (notification.okundu) return;
    this.http.post(`${API_URL}/notifications/${notification.id}/read`, {}, { headers: this.getAuthHeaders() }).subscribe({
      next: () => notification.okundu = true
    });
  }

  private getAuthHeaders(): HttpHeaders {
    const token = sessionStorage.getItem('token');
    return new HttpHeaders({ 'Content-Type': 'application/json', Authorization: token ? `Bearer ${token}` : '' });
  }

  // Bölüm verileri
  fetchDepartments(): void {
    this.http.get<Array<{ id: number; adi?: string; ad?: string }>>(`${API_URL}/Departments`, { headers: this.getAuthHeaders() }).subscribe({
      next: departments => {
        this.departments = (departments || []).map(department => ({
          id: Number(department.id),
          ad: department.adi || department.ad || `Bölüm #${department.id}`
        }));
        this.stats.totalDepartments = this.departments.length;

        const firstDepartmentId = this.departments[0]?.id;
        if (firstDepartmentId) {
          if (!this.departments.some(item => item.id === this.student.bolumId)) this.student.bolumId = firstDepartmentId;
          if (!this.departments.some(item => item.id === this.lecturer.bolumId)) this.lecturer.bolumId = firstDepartmentId;
          if (!this.departments.some(item => item.id === this.newCourse.bolumId)) this.newCourse.bolumId = firstDepartmentId;
        }
      },
      error: () => {
        this.departments = [];
        this.stats.totalDepartments = 0;
      }
    });
  }

  createDepartment(): void {
    const departmentName = this.newDepartmentName.trim();
    if (!departmentName) return void alert('Lütfen bölüm adını giriniz.');

    this.loadingDepartment = true;
    this.http.post<{ id: number; message: string }>(`${API_URL}/Departments`, { adi: departmentName }, { headers: this.getAuthHeaders() }).subscribe({
      next: response => {
        this.loadingDepartment = false;
        this.newDepartmentName = '';
        alert(response?.message || 'Bölüm başarıyla eklendi.');
        this.fetchDepartments();
        this.fetchAdvisorDepartments();
      },
      error: error => {
        this.loadingDepartment = false;
        alert(error?.error?.message || 'Bölüm eklenirken bir hata oluştu.');
      }
    });
  }

  private slugify(text: string): string {
    return text ? text.toLowerCase().trim()
      .replace(/ğ/g, 'g').replace(/ü/g, 'u').replace(/ş/g, 's')
      .replace(/ı/g, 'i').replace(/ö/g, 'o').replace(/ç/g, 'c')
      .replace(/[^a-z0-9]/g, '') : '';
  }

  generateLecturerEmail(): void {
    const name = this.slugify(this.lecturer.adi);
    const surname = this.slugify(this.lecturer.soyadi);
    this.lecturer.email = name && surname ? `${name}.${surname}@duzce.edu.tr` : name ? `${name}@duzce.edu.tr` : '';
  }

  // Sol menü ve görünüm geçişleri
  toggleStudentMenu(event: Event): void {
    event.preventDefault();
    event.stopPropagation();
    this.isStudentMenuOpen = !this.isStudentMenuOpen;
  }

  toggleLecturerMenu(event: Event): void {
    event.preventDefault();
    event.stopPropagation();
    this.isLecturerMenuOpen = !this.isLecturerMenuOpen;
  }

  switchView(view: ActiveView, event?: Event): void {
    event?.preventDefault();
    event?.stopPropagation();
    this.activeView = view;
    if (view === 'student-list') this.fetchAllStudents();
    else if (view === 'lecturer-list') this.fetchLecturers();
    else if (view === 'department-management') this.fetchDepartments();
    else if (view === 'ders-atamalari') this.fetchPendingCourseRequests();
    else if (view === 'danisman-atama') this.fetchAdvisorDepartments();
    else if (view === 'ders-yonetimi') {
      this.fetchDerslerVeHocalar();
      this.fetchAllStudents();
    }
  }

  // Danışman ve ders talebi işlemleri
  fetchAdvisorDepartments(): void {
    this.http.get<AdvisorDepartment[]>(`${API_URL}/advisors`, { headers: this.getAuthHeaders() }).subscribe({
      next: departments => this.advisorDepartments = departments || [],
      error: error => alert(error?.error?.message || 'Danışmanlık bilgileri yüklenemedi.')
    });
  }

  assignAdvisor(department: AdvisorDepartment): void {
    if (!department.danismanAkademisyenId) return void alert('Lütfen danışman akademisyen seçiniz.');
    this.http.put(`${API_URL}/advisors/department/${department.id}`,
      { lecturerId: Number(department.danismanAkademisyenId) }, { headers: this.getAuthHeaders() }).subscribe({
      next: () => { alert(`${department.bolumAdi} danışmanı başarıyla atandı.`); this.fetchAdvisorDepartments(); },
      error: error => alert(error?.error?.message || 'Danışman atanamadı.')
    });
  }

  fetchPendingCourseRequests(): void {
    this.http.get<CourseRequest[]>(`${API_URL}/course-requests/pending`, { headers: this.getAuthHeaders() }).subscribe({
      next: requests => this.pendingCourseRequests = requests || [],
      error: () => alert('Ders talepleri yüklenemedi.')
    });
  }

  decideCourseRequest(request: CourseRequest, approve: boolean): void {
    const action = approve ? 'approve' : 'reject';
    const decision = approve ? 'onaylamak' : 'reddetmek';
    if (!confirm(`${request.akademisyenAdi} tarafından yapılan ${request.dersKodu} talebini ${decision} istiyor musunuz?`)) return;
    this.http.post(`${API_URL}/course-requests/${request.id}/${action}`, {}, { headers: this.getAuthHeaders() }).subscribe({
      next: () => { alert(approve ? 'Talep onaylandı ve ders akademisyene atandı.' : 'Talep reddedildi.'); this.fetchPendingCourseRequests(); this.fetchDerslerVeHocalar(); },
      error: error => alert(error?.error?.message || error?.error?.title || error?.message || 'Talep sonuçlandırılamadı.')
    });
  }

  // Ders ve müfredat işlemleri
  importCurricula(): void {
    if (this.loadingCurriculumImport || !confirm('Kayıtlı tüm bölümlerin dersleri Düzce Üniversitesi EBS üzerinden güncellenecek. Devam edilsin mi?')) return;

    this.loadingCurriculumImport = true;
    this.http.post<any>(`${API_URL}/Courses/mufredat-ice-aktar`, {}, { headers: this.getAuthHeaders() }).subscribe({
      next: result => {
        this.loadingCurriculumImport = false;
        this.fetchDerslerVeHocalar();
        const imported = result?.importedDepartments?.length || 0;
        const added = result?.addedCourseCount || 0;
        const updated = result?.updatedCourseCount || 0;
        const unmatched = result?.unmatchedDepartments?.length || 0;
        const errors = result?.errors?.length || 0;
        alert(`Müfredat aktarımı tamamlandı.\n\n${imported} bölüm işlendi.\n${added} ders eklendi.\n${updated} ders güncellendi.\n${unmatched} bölüm eşleşmedi.\n${errors} hata oluştu.`);
      },
      error: error => {
        this.loadingCurriculumImport = false;
        alert(error?.error?.message || 'Müfredat içe aktarılırken bir hata oluştu.');
      }
    });
  }

  fetchDerslerVeHocalar(): void {
    this.http.get<any[]>(`${API_URL}/Courses`, { headers: this.getAuthHeaders() }).subscribe({
      next: courses => {
        this.tumDersler = (courses || []).map(course => ({
          ...course, donem: course.donem ?? course.Donem ?? 1, sinif: course.sinif ?? course.Sinif ?? 1
        }));
        if (this.secilenAtamaBolumId) this.onAtamaBolumChange();
      },
      error: () => console.error('Dersler yüklenemedi.')
    });
    this.http.get<Lecturer[]>(`${API_URL}/Lecturers`, { headers: this.getAuthHeaders() }).subscribe({
      next: lecturers => this.tumAkademisyenler = lecturers || [],
      error: () => console.error('Akademisyenler yüklenemedi.')
    });
  }

  coursesByGrade(grade: number): any[] {
    return this.tumDersler.filter(course => {
      const courseGrade = Number(course.sinif ?? course.Sinif);
      const departmentId = Number(course.bolumId ?? course.BolumId ?? course.departmentId ?? 0);
      return courseGrade === grade &&
        (this.selectedCourseDepartmentFilter === null || departmentId === Number(this.selectedCourseDepartmentFilter));
    });
  }

  onAtamaBolumChange(): void {
    this.secilenDersId = null;
    if (!this.secilenAtamaBolumId) {
      this.filtrelenmisDersler = [];
      return;
    }
    const departmentId = Number(this.secilenAtamaBolumId);
    this.filtrelenmisDersler = this.tumDersler.filter(course => {
      const courseDepartmentId = Number(course.bolumId || course.BolumId || course.departmentId || 0);
      const grade = Number(course.sinif ?? course.Sinif ?? 0);
      const term = Number(course.donem ?? course.Donem ?? 0);
      return courseDepartmentId === departmentId && grade >= 1 && grade <= 4 && (term === 1 || term === 2);
    });
  }

  createCourse(): void {
    if (!this.newCourse.adi) return void alert('Lütfen ders adını giriniz.');
    this.http.post(`${API_URL}/Courses`, this.newCourse, { headers: this.getAuthHeaders() }).subscribe({
      next: () => {
        alert('Ders otomatik kodla ve dönem bilgisiyle başarıyla oluşturuldu!');
        this.newCourse = { adi: '', bolumId: this.departments[0]?.id ?? 1, sinif: 1, donem: 1, akts: 5 };
        this.fetchDerslerVeHocalar();
      },
      error: () => alert('Ders eklenirken bir hata oluştu.')
    });
  }

  dersAtamasiYap(): void {
    if (!this.secilenDersId || !this.secilenAkademisyenEmail) return void alert('Lütfen hem ders hem de akademisyen seçiniz.');
    const assignment = { dersId: this.secilenDersId, akademisyenEmail: this.secilenAkademisyenEmail };
    this.http.post(`${API_URL}/Courses/AkademisyenAta`, assignment, { headers: this.getAuthHeaders() }).subscribe({
      next: () => {
        alert('Akademisyen derse başarıyla atandı!');
        this.secilenDersId = null;
        this.secilenAkademisyenEmail = null;
        this.fetchDerslerVeHocalar();
      },
      error: () => alert('Atama sırasında hata oluştu.')
    });
  }

  dersSil(course: any): void {
    const id = course.id || course.Id;
    const name = course.adi || course.ad || course.Adi || 'bu ders';
    if (!id || !confirm(`"${name}" dersi ve bu derse bağlı öğrenci kayıtları ile notlar silinecek. Devam etmek istiyor musunuz?`)) return;
    this.http.delete(`${API_URL}/Courses/${id}`, { headers: this.getAuthHeaders() }).subscribe({
      next: () => { alert('Ders başarıyla silindi.'); this.fetchDerslerVeHocalar(); },
      error: error => alert(error?.error?.message || error?.error || 'Ders silinirken hata oluştu.')
    });
  }

  // Öğrenci işlemleri
  fetchStats(): void {
    this.http.get<Student[]>(`${API_URL}/Students`, { headers: this.getAuthHeaders() }).subscribe({
      next: students => { this.stats.totalStudents = students?.length || 0; this.studentList = students || []; },
      error: error => console.error('İstatistik çekilemedi:', error)
    });
  }

  fetchAllStudents(): void {
    const url = this.selectedStudentDepartmentFilter === null
      ? `${API_URL}/Students`
      : `${API_URL}/Students?bolumId=${this.selectedStudentDepartmentFilter}`;
    this.http.get<Student[]>(url, { headers: this.getAuthHeaders() }).subscribe({
      next: students => this.studentList = students || [],
      error: () => alert('Öğrenci listesi alınamadı.')
    });
  }

  onStudentDepartmentFilterChange(): void { this.fetchAllStudents(); }

  createStudent(): void {
    this.loadingStudent = true;
    this.http.post(`${API_URL}/Students`, this.student, { headers: this.getAuthHeaders() }).subscribe({
      next: () => {
        alert('Öğrenci başarıyla eklendi!');
        this.loadingStudent = false;
        this.student = { adi: '', soyadi: '', bolumId: 1, sinif: 1 };
        this.fetchStats();
        this.switchView('student-list');
      },
      error: () => { alert('Öğrenci eklenirken hata oluştu.'); this.loadingStudent = false; }
    });
  }

  deleteStudent(): void {
    if (!this.targetStudentId || !confirm(`${this.targetStudentId} ID'li öğrenciyi silmek istediğinize emin misiniz?`)) return;
    this.http.delete(`${API_URL}/Students/${this.targetStudentId}`, { headers: this.getAuthHeaders() }).subscribe({
      next: () => { alert('Öğrenci başarıyla silindi!'); this.targetStudentId = undefined; this.fetchStats(); this.switchView('student-list'); },
      error: () => alert('Silme işlemi başarısız.')
    });
  }

  fetchStudentForUpdate(): void {
    if (!this.targetStudentId) return;
    this.http.get<Student>(`${API_URL}/Students/${this.targetStudentId}`, { headers: this.getAuthHeaders() }).subscribe({
      next: student => this.editStudent = student,
      error: () => alert('Öğrenci bulunamadı!')
    });
  }

  updateStudent(): void {
    if (!this.editStudent?.id) return;
    this.http.put(`${API_URL}/Students/${this.editStudent.id}`, this.editStudent, { headers: this.getAuthHeaders() }).subscribe({
      next: () => { alert('Öğrenci bilgileri güncellendi!'); this.editStudent = null; this.fetchStats(); this.switchView('student-list'); },
      error: () => alert('Güncelleme başarısız.')
    });
  }

  // Akademisyen işlemleri
  fetchLecturers(): void {
    const url = this.selectedDepartmentFilter === null
      ? `${API_URL}/Lecturers`
      : `${API_URL}/Lecturers?bolumId=${this.selectedDepartmentFilter}`;
    this.http.get<Lecturer[]>(url, { headers: this.getAuthHeaders() }).subscribe({
      next: lecturers => this.lecturerList = lecturers || [],
      error: () => console.error('Akademisyen listesi alınamadı.')
    });
  }

  onDepartmentFilterChange(): void { this.fetchLecturers(); }

  createLecturer(): void {
    if (!this.lecturer.adi || !this.lecturer.soyadi) return void alert('Lütfen akademisyen adını ve soyadını giriniz.');
    this.loadingLecturer = true;
    this.http.post<any>(`${API_URL}/Lecturers`, this.lecturer, { headers: this.getAuthHeaders() }).subscribe({
      next: response => {
        const password = response?.tempPassword || response?.TempPassword || response?.temp_password;
        const email = response?.email || response?.Email || this.lecturer.email;
        alert(password ? `Akademisyen Başarıyla Eklendi!\n\nE-Posta: ${email}\nGeçici Şifre: ${password}\n\nLütfen bu şifreyi akademisyene iletiniz.` : `Akademisyen Eklendi!\nE-Posta: ${email}`);
        this.loadingLecturer = false;
        this.lecturer = { unvani: 'Prof. Dr.', adi: '', soyadi: '', email: '', bolumId: 1 };
        this.switchView('lecturer-list');
      },
      error: () => { alert('Akademisyen eklenirken hata oluştu.'); this.loadingLecturer = false; }
    });
  }

  deleteLecturer(): void {
    if (!this.targetLecturerId || !confirm(`${this.targetLecturerId} ID'li akademisyeni silmek istediğinize emin misiniz?`)) return;
    this.http.delete(`${API_URL}/Lecturers/${this.targetLecturerId}`, { headers: this.getAuthHeaders() }).subscribe({
      next: () => { alert('Akademisyen başarıyla silindi!'); this.targetLecturerId = undefined; this.switchView('lecturer-list'); },
      error: () => alert('Silme işlemi başarısız.')
    });
  }

  fetchLecturerForUpdate(): void {
    if (!this.targetLecturerId) return;
    this.http.get<Lecturer>(`${API_URL}/Lecturers/${this.targetLecturerId}`, { headers: this.getAuthHeaders() }).subscribe({
      next: lecturer => this.editLecturer = lecturer,
      error: () => alert('Akademisyen bulunamadı!')
    });
  }

  updateLecturer(): void {
    if (!this.editLecturer?.id) return;
    this.http.put(`${API_URL}/Lecturers/${this.editLecturer.id}`, this.editLecturer, { headers: this.getAuthHeaders() }).subscribe({
      next: () => { alert('Akademisyen bilgileri güncellendi!'); this.editLecturer = null; this.switchView('lecturer-list'); },
      error: () => alert('Güncelleme başarısız.')
    });
  }

  quickEditLecturer(lecturer: Lecturer): void { this.targetLecturerId = lecturer.id; this.editLecturer = { ...lecturer }; this.switchView('lecturer-update'); }
  quickDeleteLecturer(id: number): void { this.targetLecturerId = id; this.switchView('lecturer-delete'); }
  toggleAdminDropdown(): void {
    this.showAdminDropdown = !this.showAdminDropdown;
    this.showNotifications = false;
  }

  // Duyuru işlemleri
  publishAnnouncement(): void {
    if (!this.newAnnouncement.title || !this.newAnnouncement.description) return;
    this.loadingAnnouncement = true;
    const request = {
      baslik: this.newAnnouncement.title,
      icerik: this.newAnnouncement.description,
      hedefRol: 'All',
      yayinda: true
    };
    this.http.post<any>(`${API_URL}/Duyurular`, request, { headers: this.getAuthHeaders() }).subscribe({
      next: announcement => {
        this.announcements.unshift(this.mapAnnouncement(announcement));
        this.newAnnouncement = { title: '', description: '' };
        this.loadingAnnouncement = false;
        alert('Duyuru başarıyla yayınlandı!');
      },
      error: () => { this.loadingAnnouncement = false; alert('Duyuru yayınlanamadı.'); }
    });
  }

  fetchAnnouncements(): void {
    this.http.get<any[]>(`${API_URL}/Duyurular`, { headers: this.getAuthHeaders() }).subscribe({
      next: announcements => this.announcements = (announcements || []).map(item => this.mapAnnouncement(item)),
      error: error => console.error('Duyurular yüklenemedi:', error)
    });
  }

  private mapAnnouncement(item: any): Announcement {
    const publishedAt = item.yayinTarihi || item.olusturulmaTarihi || new Date();
    return {
      id: item.id,
      title: item.baslik || item.title || '',
      description: item.icerik || item.description || '',
      date: new Date(publishedAt).toLocaleDateString('tr-TR', { day: '2-digit', month: 'long', year: 'numeric' })
    };
  }

  // Takvim ve oturum işlemleri
  generateCalendar(): void {
    const calendar = createCalendarView(this.displayDate, this.today);
    this.calendarTitle = calendar.title;
    this.calendarDays = calendar.days;
  }

  prevMonth(): void { this.displayDate = new Date(this.displayDate.getFullYear(), this.displayDate.getMonth() - 1, 1); this.generateCalendar(); }
  nextMonth(): void { this.displayDate = new Date(this.displayDate.getFullYear(), this.displayDate.getMonth() + 1, 1); this.generateCalendar(); }
  goToday(): void { this.displayDate = new Date(); this.generateCalendar(); }
  logout(): void { sessionStorage.clear(); this.router.navigate(['/login']); }
}
