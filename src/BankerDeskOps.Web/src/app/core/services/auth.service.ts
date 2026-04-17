import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { Router } from '@angular/router';
import { UserDto, LoginRequest, LoginResponse } from '../models';
import { ApiService } from './api.service';

const SESSION_KEY = 'banker_session';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private currentUserSubject = new BehaviorSubject<UserDto | null>(this.loadFromSession());
  public currentUser$ = this.currentUserSubject.asObservable();

  private isAnonymousSubject = new BehaviorSubject<boolean>(this.loadIsAnonymous());
  public isAnonymous$ = this.isAnonymousSubject.asObservable();

  private loadingSubject = new BehaviorSubject<boolean>(false);
  public loading$ = this.loadingSubject.asObservable();

  private errorSubject = new BehaviorSubject<string | null>(null);
  public error$ = this.errorSubject.asObservable();

  get isLoggedIn(): boolean {
    return this.currentUserSubject.value !== null || this.isAnonymousSubject.value;
  }

  get currentUser(): UserDto | null {
    return this.currentUserSubject.value;
  }

  constructor(private apiService: ApiService, private router: Router) {}

  login(request: LoginRequest): Observable<LoginResponse> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    return this.apiService.login(request).pipe(
      tap((response) => {
        this.loadingSubject.next(false);
        if (response.success) {
          this.currentUserSubject.next(response.user ?? null);
          this.isAnonymousSubject.next(response.isAnonymous);
          sessionStorage.setItem(SESSION_KEY, JSON.stringify({
            user: response.user ?? null,
            isAnonymous: response.isAnonymous,
          }));
        } else {
          this.errorSubject.next(response.errorMessage ?? 'Login failed');
        }
      }),
      catchError((error) => {
        this.loadingSubject.next(false);
        this.errorSubject.next('Connection error. Is the API running?');
        console.error('Login error:', error);
        throw error;
      })
    );
  }

  logout(): void {
    this.currentUserSubject.next(null);
    this.isAnonymousSubject.next(false);
    sessionStorage.removeItem(SESSION_KEY);
    this.router.navigate(['/login']);
  }

  clearError(): void {
    this.errorSubject.next(null);
  }

  private loadFromSession(): UserDto | null {
    try {
      const raw = sessionStorage.getItem(SESSION_KEY);
      if (!raw) return null;
      return JSON.parse(raw)?.user ?? null;
    } catch {
      return null;
    }
  }

  private loadIsAnonymous(): boolean {
    try {
      const raw = sessionStorage.getItem(SESSION_KEY);
      if (!raw) return false;
      return JSON.parse(raw)?.isAnonymous ?? false;
    } catch {
      return false;
    }
  }
}
