import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrls: ['./app.scss'],
  standalone: true,
  imports: [CommonModule, RouterOutlet],
})
export class AppComponent implements OnInit {
  title = 'BankerDeskOps Web';

  ngOnInit(): void {
    console.log('BankerDeskOps Web Application loaded');
  }
}
