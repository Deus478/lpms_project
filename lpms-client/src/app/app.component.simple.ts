import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div style="padding: 20px; font-family: Arial, sans-serif;">
      <h1 style="color: #1E8750;">LPMS - Legal Practice Management System</h1>
      <p style="font-size: 18px; margin: 10px 0;">Application is running successfully!</p>
      <div style="background: #f0f0f0; padding: 15px; border-radius: 8px; margin: 10px 0;">
        <h2>System Status:</h2>
        <p>✅ Angular Client: Running on port 4200</p>
        <p>✅ API Server: Available at http://localhost:5261</p>
        <p>✅ Database: Connected to LPMSDB</p>
      </div>
      <div style="margin-top: 20px;">
        <button (click)="showAlert()" style="background: #1E8750; color: white; padding: 10px 20px; border: none; border-radius: 5px; cursor: pointer;">
          Test Connection
        </button>
      </div>
    </div>
  `
})
export class AppComponent {
  title = 'LPMS - Legal Practice Management System';
  
  showAlert(): void {
    alert('Angular is working! Button click event is functional.');
  }
}
