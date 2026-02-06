import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { 
  Contract, 
  CreateContractDto, 
  UpdateContractStatusDto, 
  AddContractDocumentDto,
  SubmitForApprovalDto
} from '../models/contract.model';

@Injectable({
  providedIn: 'root'
})
export class ContractService {
  private readonly apiUrl = `${environment.apiBaseUrl}/api/contracts`;

  constructor(private http: HttpClient) {}

  // Contract CRUD
  getContracts(): Observable<Contract[]> {
    return this.http.get<Contract[]>(this.apiUrl);
  }

  getContract(id: number): Observable<Contract> {
    return this.http.get<Contract>(`${this.apiUrl}/${id}`);
  }

  createContract(contract: CreateContractDto): Observable<Contract> {
    return this.http.post<Contract>(this.apiUrl, contract);
  }

  updateContractStatus(id: number, updateDto: UpdateContractStatusDto): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/status`, updateDto);
  }

  // Contract Documents
  addDocument(id: number, documentDto: AddContractDocumentDto): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/documents`, documentDto);
  }

  // Contract Approvals
  submitForApproval(id: number, approvalDto: SubmitForApprovalDto): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/approvals`, approvalDto);
  }

  // Contract Renewals
  getContractsDueForRenewal(): Observable<Contract[]> {
    return this.http.get<Contract[]>(`${this.apiUrl}/renewals/due`);
  }
}
