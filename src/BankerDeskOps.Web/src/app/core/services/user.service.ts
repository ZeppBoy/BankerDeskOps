import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { UserDto, CreateUserRequest, UpdateUserRequest } from '../models';
import { ApiService } from './api.service';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private usersSubject = new BehaviorSubject<UserDto[]>([]);
  public users$ = this.usersSubject.asObservable();

  private loadingSubject = new BehaviorSubject<boolean>(false);
  public loading$ = this.loadingSubject.asObservable();

  private errorSubject = new BehaviorSubject<string | null>(null);
  public error$ = this.errorSubject.asObservable();

  constructor(private apiService: ApiService) {}

  loadUsers(): void {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    this.apiService.getUsers().subscribe({
      next: (users) => {
        this.usersSubject.next(users);
        this.loadingSubject.next(false);
      },
      error: (error) => {
        this.errorSubject.next('Failed to load users');
        this.loadingSubject.next(false);
        console.error('Error loading users:', error);
      },
    });
  }

  createUser(request: CreateUserRequest): Observable<UserDto> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    return this.apiService.createUser(request).pipe(
      tap((newUser) => {
        this.usersSubject.next([...this.usersSubject.value, newUser]);
        this.loadingSubject.next(false);
      }),
      catchError((error) => {
        this.errorSubject.next('Failed to create user');
        this.loadingSubject.next(false);
        console.error('Error creating user:', error);
        throw error;
      })
    );
  }

  updateUser(id: string, request: UpdateUserRequest): Observable<UserDto> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    return this.apiService.updateUser(id, request).pipe(
      tap((updated) => {
        const current = this.usersSubject.value;
        const index = current.findIndex((u) => u.id === id);
        if (index !== -1) {
          current[index] = updated;
          this.usersSubject.next([...current]);
        }
        this.loadingSubject.next(false);
      }),
      catchError((error) => {
        this.errorSubject.next('Failed to update user');
        this.loadingSubject.next(false);
        console.error('Error updating user:', error);
        throw error;
      })
    );
  }

  activateUser(id: string): Observable<UserDto> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    return this.apiService.activateUser(id).pipe(
      tap((updated) => {
        this.replaceInList(id, updated);
        this.loadingSubject.next(false);
      }),
      catchError((error) => {
        this.errorSubject.next('Failed to activate user');
        this.loadingSubject.next(false);
        throw error;
      })
    );
  }

  deactivateUser(id: string): Observable<UserDto> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    return this.apiService.deactivateUser(id).pipe(
      tap((updated) => {
        this.replaceInList(id, updated);
        this.loadingSubject.next(false);
      }),
      catchError((error) => {
        this.errorSubject.next('Failed to deactivate user');
        this.loadingSubject.next(false);
        throw error;
      })
    );
  }

  deleteUser(id: string): Observable<void> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    return this.apiService.deleteUser(id).pipe(
      tap(() => {
        this.usersSubject.next(this.usersSubject.value.filter((u) => u.id !== id));
        this.loadingSubject.next(false);
      }),
      catchError((error) => {
        this.errorSubject.next('Failed to delete user');
        this.loadingSubject.next(false);
        throw error;
      })
    );
  }

  private replaceInList(id: string, updated: UserDto): void {
    const current = this.usersSubject.value;
    const index = current.findIndex((u) => u.id === id);
    if (index !== -1) {
      current[index] = updated;
      this.usersSubject.next([...current]);
    }
  }
}
