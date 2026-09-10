export interface Announcement { id?: number; title: string; description: string; date: string; }
export interface Student { id?: number; adi: string; soyadi: string; ogrenciNumarasi?: string; bolumId: number; bolumAdi?: string; sinif: number; }
export interface Lecturer { id?: number; unvani: string; adi: string; soyadi: string; email: string; bolumId: number; bolumu?: string; }
export interface Department { id: number; ad: string; }
export interface AdminNotification {
  id: number;
  baslik: string;
  mesaj: string;
  okundu: boolean;
  olusturulmaTarihi: string;
}
export interface AdvisorDepartment {
  id: number; bolumAdi: string; danismanAkademisyenId: number | null; danismanAdi?: string;
  akademisyenler: Array<{ id: number; adi: string; soyadi: string; unvani: string; email: string }>;
}
export interface CourseRequest {
  id: number;
  dersId: number;
  dersKodu: string;
  dersAdi: string;
  bolumAdi: string;
  akademisyenId: number;
  akademisyenEmail: string;
  akademisyenAdi: string;
  talepTarihi: string;
}

export type ActiveView =
  | 'dashboard'
  | 'student-add' | 'student-delete' | 'student-update' | 'student-list'
  | 'lecturer-add' | 'lecturer-delete' | 'lecturer-update' | 'lecturer-list'
  | 'department-management' | 'ders-atamalari' | 'danisman-atama' | 'ders-yonetimi';
