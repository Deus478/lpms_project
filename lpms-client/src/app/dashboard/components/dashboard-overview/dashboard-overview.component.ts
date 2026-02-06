import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { CaseService } from '../../../case-management/services/case.service';
import { ContractService } from '../../../document-management/services/contract.service';
import { WorkflowService } from '../../../case-management/services/workflow.service';
import { MeetingService } from '../../../meeting-management/services/meeting.service';
import { NotificationsService } from '../../../shared/services/notifications.service';

interface DashboardStats {
  totalCases: number;
  activeCases: number;
  pendingApprovals: number;
  contractsExpiring: number;
  upcomingMeetings: number;
  unreadNotifications: number;
}

interface RecentActivity {
  id: number;
  type: 'case' | 'contract' | 'meeting' | 'document';
  title: string;
  description: string;
  timestamp: Date;
  status: string;
}

@Component({
  selector: 'app-dashboard-overview',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard-overview.component.html',
  styleUrls: ['./dashboard-overview.component.css']
})
export class DashboardOverviewComponent implements OnInit {
  stats: DashboardStats = {
    totalCases: 0,
    activeCases: 0,
    pendingApprovals: 0,
    contractsExpiring: 0,
    upcomingMeetings: 0,
    unreadNotifications: 0
  };

  recentActivities: RecentActivity[] = [];
  loading = true;
  error: string | null = null;
  currentUserId = 1; // This should come from authentication service
  currentUserName = 'John Doe'; // This should come from authentication service

  constructor(
    private caseService: CaseService,
    private contractService: ContractService,
    private workflowService: WorkflowService,
    private meetingService: MeetingService,
    private notificationsService: NotificationsService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.loading = true;
    this.error = null;

    // Load all dashboard data in parallel
    Promise.all([
      this.loadCaseStats(),
      this.loadContractStats(),
      this.loadPendingTasks(),
      this.loadUpcomingMeetings(),
      this.loadNotifications(),
      this.loadRecentActivity()
    ]).then(() => {
      this.loading = false;
    }).catch((err) => {
      this.error = 'Failed to load dashboard data';
      this.loading = false;
      console.error('Dashboard loading error:', err);
    });
  }

  private async loadCaseStats(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.caseService.getCases().subscribe({
        next: (cases: any[]) => {
          this.stats.totalCases = cases.length;
          this.stats.activeCases = cases.filter((c: any) => 
            c.status === 'In Progress' || c.status === 'Pending'
          ).length;
          resolve();
        },
        error: reject
      });
    });
  }

  private async loadContractStats(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.contractService.getContractsDueForRenewal().subscribe({
        next: (contracts: any[]) => {
          this.stats.contractsExpiring = contracts.length;
          resolve();
        },
        error: reject
      });
    });
  }

  private async loadPendingTasks(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.workflowService.getPendingTasks(this.currentUserId).subscribe({
        next: (tasks: any[]) => {
          this.stats.pendingApprovals = tasks.length;
          resolve();
        },
        error: reject
      });
    });
  }

  private async loadUpcomingMeetings(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.meetingService.getUpcomingMeetings().subscribe({
        next: (meetings: any[]) => {
          this.stats.upcomingMeetings = meetings.length;
          resolve();
        },
        error: reject
      });
    });
  }

  private async loadNotifications(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.notificationsService.getUnreadCount(this.currentUserId).subscribe({
        next: (count: number) => {
          this.stats.unreadNotifications = count;
          resolve();
        },
        error: reject
      });
    });
  }

  private async loadRecentActivity(): Promise<void> {
    // This would typically come from a dedicated activity endpoint
    // For now, we'll create some mock data
    this.recentActivities = [
      {
        id: 1,
        type: 'case',
        title: 'Case #2024-00123',
        description: 'New litigation case filed',
        timestamp: new Date(Date.now() - 2 * 60 * 60 * 1000), // 2 hours ago
        status: 'Pending'
      },
      {
        id: 2,
        type: 'contract',
        title: 'Service Agreement - ABC Corp',
        description: 'Contract awaiting legal review',
        timestamp: new Date(Date.now() - 4 * 60 * 60 * 1000), // 4 hours ago
        status: 'Under Review'
      },
      {
        id: 3,
        type: 'meeting',
        title: 'Board Meeting',
        description: 'Quarterly board meeting scheduled',
        timestamp: new Date(Date.now() - 6 * 60 * 60 * 1000), // 6 hours ago
        status: 'Scheduled'
      }
    ];
    return Promise.resolve();
  }

  navigateToSection(section: string): void {
    switch (section) {
      case 'cases':
        this.router.navigate(['/case-management']);
        break;
      case 'contracts':
        this.router.navigate(['/document-management']);
        break;
      case 'meetings':
        this.router.navigate(['/meetings']);
        break;
      case 'documents':
        this.router.navigate(['/document-management']);
        break;
      case 'notifications':
        this.router.navigate(['/notifications']);
        break;
    }
  }

  getActivityIcon(type: string): string {
    switch (type) {
      case 'case':
        return '⚖️';
      case 'contract':
        return '📄';
      case 'meeting':
        return '👥';
      case 'document':
        return '📁';
      default:
        return '📋';
    }
  }

  getActivityColor(type: string): string {
    switch (type) {
      case 'case':
        return 'text-blue-600 bg-blue-100';
      case 'contract':
        return 'text-green-600 bg-green-100';
      case 'meeting':
        return 'text-purple-600 bg-purple-100';
      case 'document':
        return 'text-orange-600 bg-orange-100';
      default:
        return 'text-gray-600 bg-gray-100';
    }
  }

  formatTimeAgo(date: Date): string {
    const now = new Date();
    const diff = now.getTime() - date.getTime();
    const hours = Math.floor(diff / (1000 * 60 * 60));
    const days = Math.floor(hours / 24);
    
    if (days > 0) {
      return `${days} day${days > 1 ? 's' : ''} ago`;
    } else if (hours > 0) {
      return `${hours} hour${hours > 1 ? 's' : ''} ago`;
    } else {
      return 'Just now';
    }
  }

  // Navigation methods
  navigateToCases(): void {
    this.router.navigate(['/case-management']);
  }

  navigateToContracts(): void {
    this.router.navigate(['/document-management']);
  }

  navigateToMeetings(): void {
    this.router.navigate(['/meetings']);
  }

  navigateToDocuments(): void {
    this.router.navigate(['/document-management']);
  }
}
