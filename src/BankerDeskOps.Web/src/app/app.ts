import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { map } from 'rxjs/operators';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrls: ['./app.scss'],
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
})
export class AppComponent implements OnInit {
  title = 'BankerDeskOps Web';

  isLoggedIn$ = this.authService.currentUser$.pipe(
    map(() => this.authService.isLoggedIn)
  );

  currentUserDisplay$ = this.authService.currentUser$.pipe(
    map((user) => {
      if (this.authService.isLoggedIn) {
        return user ? `${user.firstName} ${user.lastName} (${user.username})` : 'Guest';
      }
      return '';
    })
  );

  constructor(private authService: AuthService) {}

  ngOnInit(): void {
    console.log('BankerDeskOps Web Application loaded');
  }

  logout(): void {
    this.authService.logout();
  }
}
