import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserService } from '../../core/services/user.service';
import {
  UserDto,
  UserRole,
  UserStatus,
  CreateUserRequest,
  UpdateUserRequest,
} from '../../core/models';

@Component({
  selector: 'app-users',
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.scss'],
  standalone: true,
  imports: [CommonModule, FormsModule],
})
export class UsersComponent implements OnInit {
  users$   = this.userService.users$;
  loading$ = this.userService.loading$;
  error$   = this.userService.error$;

  UserRole   = UserRole;
  UserStatus = UserStatus;
  roleValues = Object.values(UserRole).filter((v) => typeof v === 'number') as UserRole[];

  editingId: string | null = null;
  showModal = false;

  formData: CreateUserRequest = this.emptyForm();

  constructor(private userService: UserService) {}

  ngOnInit(): void {
    this.userService.loadUsers();
  }

  openModal(): void {
    this.editingId = null;
    this.formData  = this.emptyForm();
    this.showModal = true;
    document.body.style.overflow = 'hidden';
  }

  editUser(user: UserDto): void {
    this.editingId = user.id;
    this.formData  = {
      username:  user.username,
      email:     user.email,
      firstName: user.firstName,
      lastName:  user.lastName,
      password:  '',
      role:      user.role,
    };
    this.showModal = true;
    document.body.style.overflow = 'hidden';
  }

  hideModal(): void {
    this.showModal = false;
    document.body.style.overflow = 'auto';
  }

  saveUser(): void {
    if (!this.formData.username || !this.formData.firstName
        || !this.formData.lastName || !this.formData.email) {
      alert('Username, first name, last name and email are required');
      return;
    }

    if (this.editingId) {
      const updateReq: UpdateUserRequest = {
        email:     this.formData.email,
        firstName: this.formData.firstName,
        lastName:  this.formData.lastName,
        role:      this.formData.role,
      };
      this.userService.updateUser(this.editingId, updateReq).subscribe({
        next: () => this.hideModal(),
        error: (err: unknown) => console.error('Error updating user:', err),
      });
    } else {
      if (!this.formData.password || this.formData.password.length < 8) {
        alert('Password must be at least 8 characters');
        return;
      }
      this.userService.createUser(this.formData).subscribe({
        next: () => this.hideModal(),
        error: (err: unknown) => console.error('Error creating user:', err),
      });
    }
  }

  activateUser(id: string): void {
    if (confirm('Activate this user?')) {
      this.userService.activateUser(id).subscribe({
        error: (err: unknown) => console.error('Error activating user:', err),
      });
    }
  }

  deactivateUser(id: string): void {
    if (confirm('Deactivate this user?')) {
      this.userService.deactivateUser(id).subscribe({
        error: (err: unknown) => console.error('Error deactivating user:', err),
      });
    }
  }

  deleteUser(id: string): void {
    if (confirm('Are you sure you want to delete this user?')) {
      this.userService.deleteUser(id).subscribe({
        error: (err: unknown) => console.error('Error deleting user:', err),
      });
    }
  }

  getRoleLabel(role: UserRole): string {
    return UserRole[role] ?? 'Unknown';
  }

  getStatusLabel(status: UserStatus): string {
    return UserStatus[status] ?? 'Unknown';
  }

  getStatusClass(status: UserStatus): string {
    switch (status) {
      case UserStatus.Active:   return 'bg-success';
      case UserStatus.Inactive: return 'bg-secondary';
      case UserStatus.Locked:   return 'bg-danger';
      default:                  return 'bg-secondary';
    }
  }

  private emptyForm(): CreateUserRequest {
    return {
      username:  '',
      email:     '',
      firstName: '',
      lastName:  '',
      password:  '',
      role:      UserRole.Operator,
    };
  }
}
