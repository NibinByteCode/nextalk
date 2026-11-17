import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthResponse } from './models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly storageKey = 'nextalk.auth';
  private readonly currentUserSubject = new BehaviorSubject<AuthResponse | null>(this.readStoredUser());
  readonly currentUser$ = this.currentUserSubject.asObservable();

  constructor(private readonly http: HttpClient) {}

  get token(): string | null { return this.currentUserSubject.value?.accessToken ?? null; }
  get user(): AuthResponse | null { return this.currentUserSubject.value; }

  login(email: string, password: string) {
    return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/login`, { email, password }).pipe(tap(user => this.save(user)));
  }
  register(userName: string, email: string, password: string) {
    return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/register`, { userName, email, password }).pipe(tap(user => this.save(user)));
  }
  logout() {
    localStorage.removeItem(this.storageKey);
    this.currentUserSubject.next(null);
  }
  private save(user: AuthResponse) {
    localStorage.setItem(this.storageKey, JSON.stringify(user));
    this.currentUserSubject.next(user);
  }
  private readStoredUser(): AuthResponse | null {
    try {
      const value = localStorage.getItem(this.storageKey);
      return value ? JSON.parse(value) as AuthResponse : null;
    } catch {
      return null;
    }
  }
}
