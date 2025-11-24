
import { Routes } from '@angular/router';
import { DocumentListComponent } from './components/document-list/document-list.component';
import { DocumentUploadComponent } from './components/document-upload/document-upload.component';
import { DocumentViewComponent } from './components/document-view/document-view.component';
import { DocumentArchiveComponent } from './components/document-archive/document-archive.component';

export const DOCUMENT_ROUTES: Routes = [
  {
    path: '',
    component: DocumentListComponent
  },
  {
    path: 'upload',
    component: DocumentUploadComponent
  },
  {
    path: 'view/:id',
    component: DocumentViewComponent
  },
  {
    path: 'archive',
    component: DocumentArchiveComponent
  }
];