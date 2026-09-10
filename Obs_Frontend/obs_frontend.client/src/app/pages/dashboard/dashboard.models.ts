export interface Announcement {
  id?: number;
  date: string;
  title: string;
  description: string;
  icon: string;
  iconClass: string;
  isPersonal?: boolean;
  unread?: boolean;
}

export interface CourseRequest {
  id: number;
  dersId: number;
  dersKodu: string;
  dersAdi: string;
  durum: 'Bekliyor' | 'Onaylandı' | 'Reddedildi';
  talepTarihi: string;
  kararTarihi?: string;
}

export interface ExamResult {
  dersKodu: string;
  dersAdi: string;
  akts: number;
  vize: number | null;
  final: number | null;
  ortalama: number | null;
  harfNotu: string | null;
  sinif?: number;
  donem?: string;
}
