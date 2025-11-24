import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CaseListComponent } from './components/case-list/case-list.component';
import { CaseFormComponent } from './components/case-form/case-form.component';
import { CaseDetailsComponent } from './components/case-details/case-details.component';

const routes: Routes = [
  {
    path: '',
    component: CaseListComponent
  },
  {
    path: 'create',
    component: CaseFormComponent
  },
  {
    path: 'edit/:id',
    component: CaseFormComponent
  },
  {
    path: 'details/:id',
    component: CaseDetailsComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CaseManagementRoutingModule { }