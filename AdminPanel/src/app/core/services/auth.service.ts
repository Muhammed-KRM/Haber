import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, catchError, of } from 'rxjs';
import { AuthResultDto, UserDto } from '../models/auth.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:5000/api/auth';
  private tokenKey = 'kursutv_admin_token';
  private userKey = 'kursutv_admin_user';

  currentUser = signal<UserDto | null>(this.getStoredUser());
  token = signal<string | null>(localStorage.getItem(this.tokenKey));

  isAuthenticated = computed(() => !!this.token());
  isSuperAdmin = computed(() => this.currentUser()?.role === 'SuperAdmin' || this.currentUser()?.role === 'Admin');

  constructor(private http: HttpClient, private router: Router) {}

  login(credentials: { email: string; password: string }): Observable<AuthResultDto> {
    return this.http.post<AuthResultDto>(`${this.apiUrl}/login`, credentials).pipe(
      tap(res => {
        if (res.success && res.token) {
          this.setSession(res.token, res.user || null);
        }
      })
    );
  }

  logout() {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
    this.token.set(null);
    this.currentUser.set(null);
    this.router.navigate(['/login']);
  }

  private setSession(token: string, user: UserDto | null) {
    localStorage.setItem(this.tokenKey, token);
    this.token.set(token);
    if (user) {
      localStorage.setItem(this.userKey, JSON.stringify(user));
      this.currentUser.set(user);
    }
  }

  private getStoredUser(): UserDto | null {
    const raw = localStorage.getItem(this.userKey);
    if (!raw) return null;
    try {
      return JSON.parse(raw);
    } catch {
      return null;
    }
  }
}
