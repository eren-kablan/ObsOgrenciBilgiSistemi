import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CalendarDay } from '../../shared/calendar/calendar.models';
import { createCalendarView } from '../../shared/calendar/calendar.utils';
import { ActiveView, AdminNotification, AdvisorDepartment, Announcement, CourseRequest, Department, Lecturer, Student } from './admin-dashboard.models';
import { environment } from '../../../environments/environment';
import { NotificationService } from '../../services/notification.service';

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
  studentFirstNameSearch = '';
  studentLastNameSearch = '';
  deletingStudentId?: number;
  updatingStudent = false;
  lecturerFirstNameSearch = '';
  lecturerLastNameSearch = '';
  deletingLecturerId?: number;
  updatingLecturer = false;
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
  constructor(
    private router: Router,
    private http: HttpClient,
    private notification: NotificationService
  ) {}

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
    if (!departmentName) return void this.notification.warning('Lütfen bölüm adını girin.');

    this.loadingDepartment = true;
    this.http.post<{ id: number; message: string }>(`${API_URL}/Departments`, { adi: departmentName }, { headers: this.getAuthHeaders() }).subscribe({
      next: response => {
        this.loadingDepartment = false;
        this.newDepartmentName = '';
        this.notification.success(response?.message || 'Bölüm başarıyla eklendi.');
        this.fetchDepartments();
        this.fetchAdvisorDepartments();
      },
      error: error => {
        this.loadingDepartment = false;
        this.notification.error(error?.error?.message || 'Bölüm eklenirken bir hata oluştu.');
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
    if (view === 'student-list' || view === 'student-delete' || view === 'student-update') {
      if (view === 'student-delete' || view === 'student-update') {
        this.studentFirstNameSearch = '';
        this.studentLastNameSearch = '';
      }
      if (view === 'student-update') this.editStudent = null;
      this.fetchAllStudents();
    }
    else if (view === 'lecturer-list' || view === 'lecturer-delete' || view === 'lecturer-update') {
      if (view === 'lecturer-delete' || view === 'lecturer-update') {
        this.lecturerFirstNameSearch = '';
        this.lecturerLastNameSearch = '';
      }
      if (view === 'lecturer-update') this.editLecturer = null;
      this.fetchLecturers();
    }
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
      error: error => this.notification.error(error?.error?.message || 'Danışmanlık bilgileri yüklenemedi.')
    });
  }

  assignAdvisor(department: AdvisorDepartment): void {
    if (!department.danismanAkademisyenId) return void this.notification.warning('Lütfen danışman akademisyen seçin.');
    this.http.put(`${API_URL}/advisors/department/${department.id}`,
      { lecturerId: Number(department.danismanAkademisyenId) }, { headers: this.getAuthHeaders() }).subscribe({
      next: () => { this.notification.success(`${department.bolumAdi} danışmanı başarıyla atandı.`); this.fetchAdvisorDepartments(); },
      error: error => this.notification.error(error?.error?.message || 'Danışman atanamadı.')
    });
  }

  fetchPendingCourseRequests(): void {
    this.http.get<CourseRequest[]>(`${API_URL}/course-requests/pending`, { headers: this.getAuthHeaders() }).subscribe({
      next: requests => this.pendingCourseRequests = requests || [],
      error: () => this.notification.error('Ders talepleri yüklenemedi.')
    });
  }

  async decideCourseRequest(request: CourseRequest, approve: boolean): Promise<void> {
    const action = approve ? 'approve' : 'reject';
    const decision = approve ? 'onaylamak' : 'reddetmek';
    const confirmed = await this.notification.confirm(
      `${request.akademisyenAdi} tarafından yapılan ${request.dersKodu} talebini ${decision} istiyor musunuz?`,
      approve ? 'Ders talebini onayla' : 'Ders talebini reddet',
      approve ? 'Onayla' : 'Reddet'
    );
    if (!confirmed) return;
    this.http.post(`${API_URL}/course-requests/${request.id}/${action}`, {}, { headers: this.getAuthHeaders() }).subscribe({
      next: () => { this.notification.success(approve ? 'Talep onaylandı ve ders akademisyene atandı.' : 'Talep reddedildi.'); this.fetchPendingCourseRequests(); this.fetchDerslerVeHocalar(); },
      error: error => this.notification.error(error?.error?.message || error?.error?.title || error?.message || 'Talep sonuçlandırılamadı.')
    });
  }

  // Ders ve müfredat işlemleri
  async importCurricula(): Promise<void> {
    if (this.loadingCurriculumImport) return;
    const confirmed = await this.notification.confirm(
      'Kayıtlı tüm bölümlerin dersleri Düzce Üniversitesi EBS üzerinden güncellenecek. Devam edilsin mi?',
      'Müfredatı güncelle',
      'Güncellemeyi başlat'
    );
    if (!confirmed) return;

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
        this.notification.success(`Müfredat aktarımı tamamlandı.\n\n${imported} bölüm işlendi.\n${added} ders eklendi.\n${updated} ders güncellendi.\n${unmatched} bölüm eşleşmedi.\n${errors} hata oluştu.`);
      },
      error: error => {
        this.loadingCurriculumImport = false;
        this.notification.error(error?.error?.message || 'Müfredat içe aktarılırken bir hata oluştu.');
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
    if (!this.newCourse.adi) return void this.notification.warning('Lütfen ders adını girin.');
    this.http.post(`${API_URL}/Courses`, this.newCourse, { headers: this.getAuthHeaders() }).subscribe({
      next: () => {
        this.notification.success('Ders otomatik kodla ve dönem bilgisiyle başarıyla oluşturuldu.');
        this.newCourse = { adi: '', bolumId: this.departments[0]?.id ?? 1, sinif: 1, donem: 1, akts: 5 };
        this.fetchDerslerVeHocalar();
      },
      error: () => this.notification.error('Ders eklenirken bir hata oluştu.')
    });
  }

  dersAtamasiYap(): void {
    if (!this.secilenDersId || !this.secilenAkademisyenEmail) return void this.notification.warning('Lütfen hem ders hem de akademisyen seçin.');
    const assignment = { dersId: this.secilenDersId, akademisyenEmail: this.secilenAkademisyenEmail };
    this.http.post(`${API_URL}/Courses/AkademisyenAta`, assignment, { headers: this.getAuthHeaders() }).subscribe({
      next: () => {
        this.notification.success('Akademisyen derse başarıyla atandı.');
        this.secilenDersId = null;
        this.secilenAkademisyenEmail = null;
        this.fetchDerslerVeHocalar();
      },
      error: () => this.notification.error('Atama sırasında hata oluştu.')
    });
  }

  async dersSil(course: any): Promise<void> {
    const id = course.id || course.Id;
    const name = course.adi || course.ad || course.Adi || 'bu ders';
    if (!id) return;
    const confirmed = await this.notification.confirm(
      `"${name}" dersi ve bu derse bağlı öğrenci kayıtları ile notlar silinecek. Devam etmek istiyor musunuz?`,
      'Dersi sil',
      'Dersi sil'
    );
    if (!confirmed) return;
    this.http.delete(`${API_URL}/Courses/${id}`, { headers: this.getAuthHeaders() }).subscribe({
      next: () => { this.notification.success('Ders başarıyla silindi.'); this.fetchDerslerVeHocalar(); },
      error: error => this.notification.error(error?.error?.message || error?.error || 'Ders silinirken hata oluştu.')
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
      error: () => this.notification.error('Öğrenci listesi alınamadı.')
    });
  }

  onStudentDepartmentFilterChange(): void { this.fetchAllStudents(); }

  get filteredStudents(): Student[] {
    const firstName = this.studentFirstNameSearch.trim().toLocaleLowerCase('tr-TR');
    const lastName = this.studentLastNameSearch.trim().toLocaleLowerCase('tr-TR');
    if (!firstName && !lastName) return this.studentList;

    return this.studentList.filter(student =>
      (!firstName || student.adi.toLocaleLowerCase('tr-TR').includes(firstName)) &&
      (!lastName || student.soyadi.toLocaleLowerCase('tr-TR').includes(lastName))
    );
  }

  createStudent(): void {
    this.loadingStudent = true;
    this.http.post(`${API_URL}/Students`, this.student, { headers: this.getAuthHeaders() }).subscribe({
      next: () => {
        this.notification.success('Öğrenci başarıyla eklendi.');
        this.loadingStudent = false;
        this.student = { adi: '', soyadi: '', bolumId: 1, sinif: 1 };
        this.fetchStats();
        this.switchView('student-list');
      },
      error: () => { this.notification.error('Öğrenci eklenirken hata oluştu.'); this.loadingStudent = false; }
    });
  }

  async deleteStudent(student: Student): Promise<void> {
    const studentId = student.id;
    if (!studentId || this.deletingStudentId) return;

    const studentName = `${student.adi} ${student.soyadi}`.trim();
    const confirmed = await this.notification.confirm(
      `${studentName} (${student.ogrenciNumarasi || 'öğrenci numarası yok'}) sistemden silinecek. Ders kayıtları, notları ve devamsızlık bilgileri de kaldırılacak. Bu işlemi onaylıyor musunuz?`,
      'Öğrenciyi sil',
      'Öğrenciyi sil'
    );
    if (!confirmed) return;

    this.deletingStudentId = studentId;
    this.http.delete(`${API_URL}/Students/${studentId}`, { headers: this.getAuthHeaders() }).subscribe({
      next: () => {
        this.deletingStudentId = undefined;
        this.studentList = this.studentList.filter(item => item.id !== studentId);
        this.stats.totalStudents = Math.max(0, this.stats.totalStudents - 1);
        this.notification.success(`${studentName} sistemden silindi.`);
      },
      error: error => {
        this.deletingStudentId = undefined;
        this.notification.error(error?.error?.message || 'Öğrenci silinemedi.');
      }
    });
  }

  quickDeleteStudent(student: Student): void { void this.deleteStudent(student); }

  selectStudentForUpdate(student: Student): void {
    this.editStudent = { ...student };
  }

  updateStudent(): void {
    if (!this.editStudent?.id) return;
    this.updatingStudent = true;
    this.http.put(`${API_URL}/Students/${this.editStudent.id}`, this.editStudent, { headers: this.getAuthHeaders() }).subscribe({
      next: () => {
        this.updatingStudent = false;
        this.notification.success('Öğrenci bilgileri güncellendi.');
        this.editStudent = null;
        this.fetchStats();
        this.switchView('student-list');
      },
      error: error => {
        this.updatingStudent = false;
        this.notification.error(error?.error?.message || 'Güncelleme başarısız.');
      }
    });
  }

  quickEditStudent(student: Student): void {
    this.activeView = 'student-update';
    this.studentFirstNameSearch = '';
    this.studentLastNameSearch = '';
    this.selectStudentForUpdate(student);
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

  get filteredLecturers(): Lecturer[] {
    const firstName = this.lecturerFirstNameSearch.trim().toLocaleLowerCase('tr-TR');
    const lastName = this.lecturerLastNameSearch.trim().toLocaleLowerCase('tr-TR');
    if (!firstName && !lastName) return this.lecturerList;

    return this.lecturerList.filter(lecturer =>
      (!firstName || lecturer.adi.toLocaleLowerCase('tr-TR').includes(firstName)) &&
      (!lastName || lecturer.soyadi.toLocaleLowerCase('tr-TR').includes(lastName))
    );
  }

  getDepartmentName(departmentId: number): string {
    return this.departments.find(department => department.id === departmentId)?.ad || `Bölüm #${departmentId}`;
  }

  createLecturer(): void {
    if (!this.lecturer.adi || !this.lecturer.soyadi) return void this.notification.warning('Lütfen akademisyen adını ve soyadını girin.');
    this.loadingLecturer = true;
    this.http.post<any>(`${API_URL}/Lecturers`, this.lecturer, { headers: this.getAuthHeaders() }).subscribe({
      next: response => {
        const password = response?.tempPassword || response?.TempPassword || response?.temp_password;
        const email = response?.email || response?.Email || this.lecturer.email;
        this.notification.success(
          password
            ? `Akademisyen başarıyla eklendi.\n\nE-posta: ${email}\nGeçici şifre: ${password}\n\nLütfen bu şifreyi akademisyene iletin.`
            : `Akademisyen eklendi.\nE-posta: ${email}`
        );
        this.loadingLecturer = false;
        this.lecturer = { unvani: 'Prof. Dr.', adi: '', soyadi: '', email: '', bolumId: 1 };
        this.switchView('lecturer-list');
      },
      error: () => { this.notification.error('Akademisyen eklenirken hata oluştu.'); this.loadingLecturer = false; }
    });
  }

  async deleteLecturer(lecturer: Lecturer): Promise<void> {
    const lecturerId = lecturer.id;
    if (!lecturerId || this.deletingLecturerId) return;

    const lecturerName = `${lecturer.unvani} ${lecturer.adi} ${lecturer.soyadi}`.trim();
    const confirmed = await this.notification.confirm(
      `${lecturerName} (${lecturer.email}) sistemden silinecek. Ders atamaları ve danışmanlık bağlantıları kaldırılacak. Bu işlemi onaylıyor musunuz?`,
      'Akademisyeni sil',
      'Akademisyeni sil'
    );
    if (!confirmed) return;

    this.deletingLecturerId = lecturerId;
    this.http.delete(`${API_URL}/Lecturers/${lecturerId}`, { headers: this.getAuthHeaders() }).subscribe({
      next: () => {
        this.deletingLecturerId = undefined;
        this.lecturerList = this.lecturerList.filter(item => item.id !== lecturerId);
        this.notification.success(`${lecturerName} sistemden silindi.`);
      },
      error: error => {
        this.deletingLecturerId = undefined;
        this.notification.error(error?.error?.message || 'Akademisyen silinemedi.');
      }
    });
  }

  selectLecturerForUpdate(lecturer: Lecturer): void {
    this.editLecturer = { ...lecturer };
  }

  updateLecturer(): void {
    if (!this.editLecturer?.id) return;
    this.updatingLecturer = true;
    this.http.put(`${API_URL}/Lecturers/${this.editLecturer.id}`, this.editLecturer, { headers: this.getAuthHeaders() }).subscribe({
      next: () => {
        this.updatingLecturer = false;
        this.notification.success('Akademisyen bilgileri güncellendi.');
        this.editLecturer = null;
        this.switchView('lecturer-list');
      },
      error: error => {
        this.updatingLecturer = false;
        this.notification.error(error?.error?.message || 'Güncelleme başarısız.');
      }
    });
  }

  quickEditLecturer(lecturer: Lecturer): void {
    this.activeView = 'lecturer-update';
    this.lecturerFirstNameSearch = '';
    this.lecturerLastNameSearch = '';
    this.selectLecturerForUpdate(lecturer);
  }
  quickDeleteLecturer(lecturer: Lecturer): void { void this.deleteLecturer(lecturer); }
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
        this.notification.success('Duyuru başarıyla yayınlandı.');
      },
      error: () => { this.loadingAnnouncement = false; this.notification.error('Duyuru yayınlanamadı.'); }
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
