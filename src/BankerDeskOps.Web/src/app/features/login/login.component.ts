import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  standalone: true,
  imports: [CommonModule, FormsModule],
})
export class LoginComponent {
  username = '';
  password = '';
  isAnonymous = false;

  loading$ = this.authService.loading$;
  error$   = this.authService.error$;

  get loginButtonText(): string {
    return this.isAnonymous ? 'Enter as Guest' : 'Log In';
  }

  constructor(private authService: AuthService, private router: Router) {
    // If already logged in, redirect to home
    if (authService.isLoggedIn) {
      router.navigate(['/']);
    }
  }

  onAnonymousChange(): void {
    if (this.isAnonymous) {
      this.username = 'anonymous';
      this.password = '';
    } else {
      this.username = '';
    }
    this.authService.clearError();
  }

  login(): void {
    const request = {
      username: this.isAnonymous ? 'anonymous' : this.username,
      password: this.isAnonymous ? '' : this.password,
    };

    this.authService.login(request).subscribe({
      next: (response) => {
        if (response.success) {
          this.router.navigate(['/']);
        }
      },
      error: () => { /* Error handled in AuthService */ },
    });
  }
}
