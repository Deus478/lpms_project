import { Routes } from '@angular/router';

import { DashboardOverviewComponent } from './dashboard/components/dashboard-overview/dashboard-overview.component';
import { CaseListComponent } from './case-management/components/case-list/case-list.component';
import { CaseFormComponent } from './case-management/components/case-form/case-form.component';
import { CaseDetailsComponent } from './case-management/components/case-details/case-details.component';
import { LoginComponent } from './auth/login.component';

// 📂 Import document management components
import { DocumentListComponent } from './document-management/components/document-list/document-list.component';
import { DocumentUploadComponent } from './document-management/components/document-upload/document-upload.component';
import { DocumentViewComponent } from './document-management/components/document-view/document-view.component';
import { DocumentArchiveComponent } from './document-management/components/document-archive/document-archive.component';
import { MeetingListComponent } from './meeting-management/components/meeting-list/meeting-list.component';
import { MeetingFormComponent } from './meeting-management/components/meeting-form/meeting-form.component';
import { MeetingDetailComponent } from './meeting-management/components/meeting-detail/meeting-detail.component';
import { ResolutionSummaryComponent } from './meeting-management/components/resolution-summary/resolution-summary.component';

export const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'dashboard', component: DashboardOverviewComponent },

  // 🧾 Case Management
  { path: 'case-management', component: CaseListComponent },
  { path: 'case-management/create', component: CaseFormComponent },
  { path: 'case-management/edit/:id', component: CaseFormComponent },
  { path: 'case-management/details/:id', component: CaseDetailsComponent },

  // 📑 Document Management
  { path: 'document-management', component: DocumentListComponent },
  { path: 'document-management/upload', component: DocumentUploadComponent },
  { path: 'document-management/view/:id', component: DocumentViewComponent },
  { path: 'document-management/archive', component: DocumentArchiveComponent },

  // 📆 Meeting Management
  { path: 'meetings', component: MeetingListComponent },
  { path: 'meetings/create', component: MeetingFormComponent },
  { path: 'meetings/edit/:id', component: MeetingFormComponent },
  { path: 'meetings/resolution-summary', component: ResolutionSummaryComponent },
  { path: 'meetings/:id', component: MeetingDetailComponent },
];

