import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from './auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div style="max-width: 420px; margin: 0 auto; padding: 24px;">
      <h2 style="margin: 0 0 16px 0;">Sign in</h2>

      <form (ngSubmit)="onSubmit()" #form="ngForm" style="display: grid; gap: 12px;">
        <label style="display: grid; gap: 6px;">
          <span>Email</span>
          <input
            name="email"
            [(ngModel)]="email"
            required
            type="email"
            placeholder="admin@law.com"
            style="padding: 10px; border: 1px solid #ccc; border-radius: 6px;"
          />
        </label>

        <button
          type="submit"
          [disabled]="form.invalid || isSubmitting"
          style="background: #1E8750; color: white; padding: 10px 16px; border: none; border-radius: 6px; cursor: pointer;"
        >
          {{ isSubmitting ? 'Signing in...' : 'Sign in' }}
        </button>

        <div *ngIf="error" style="color: #b00020;">
          {{ error }}
        </div>

        <div style="font-size: 13px; color: #666;">
          Use an email that exists in the LPMSDB Users table.
        </div>
      </form>
    </div>
  `
})
export class LoginComponent {
  email = '';
  isSubmitting = false;
  error: string | null = null;

  constructor(
    private auth: AuthService,
    private router: Router
  ) {}

  onSubmit(): void {
    this.error = null;
    this.isSubmitting = true;

    this.auth.login(this.email).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.router.navigate(['/dashboard']);
      },
      error: (err: any) => {
        this.isSubmitting = false;
        if (err?.status === 401) {
          this.error = 'Invalid email or inactive user.';
          return;
        }
        this.error = 'Login failed. Please try again.';
      }
    });
  }
}
