
import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div style="padding: 20px; font-family: Arial, sans-serif;">
      <header style="background: #1E8750; color: white; padding: 15px; margin-bottom: 20px; border-radius: 8px;">
        <div style="display: flex; justify-content: space-between; align-items: center;">
          <div>
            <h1 style="margin: 0; font-size: 24px;">LPMS - Legal Practice Management System</h1>
          </div>
          <nav style="display: flex; gap: 20px;">
            <a routerLink="/dashboard" routerLinkActive="active" style="color: white; text-decoration: none; padding: 8px 16px; border-radius: 4px; transition: background 0.3s;">
              Dashboard
            </a>
            <a routerLink="/case-management" routerLinkActive="active" style="color: white; text-decoration: none; padding: 8px 16px; border-radius: 4px; transition: background 0.3s;">
              Cases
            </a>
            <a routerLink="/document-management" routerLinkActive="active" style="color: white; text-decoration: none; padding: 8px 16px; border-radius: 4px; transition: background 0.3s;">
              Documents
            </a>
            <a routerLink="/meetings" routerLinkActive="active" style="color: white; text-decoration: none; padding: 8px 16px; border-radius: 4px; transition: background 0.3s;">
              Meetings
            </a>
          </nav>
        </div>
      </header>
      
      <main style="min-height: 400px; background: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);">
        <router-outlet></router-outlet>
      </main>
      
      <footer style="text-align: center; padding: 20px; color: #666; margin-top: 20px;">
        <p>&copy; 2026 LPMS. All rights reserved.</p>
      </footer>
    </div>
  `
})
export class AppComponent {
  title = 'LPMS - Legal Practice Management System';
}
