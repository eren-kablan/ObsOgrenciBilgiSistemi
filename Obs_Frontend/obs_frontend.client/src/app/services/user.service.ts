import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private apiUrl = 'https://localhost:7066/api/Students/profile'; // Kendi API portunu kontrol et

  constructor(private http: HttpClient) { }

  getProfile(email: string): Observable<any> {
    return this.http.get(`${this.apiUrl}?email=${email}`);
  }
}
