import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ContractService } from '../../services/contract.service';
import { Contract } from '../../models/contract.model';

@Component({
  selector: 'app-contract-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './contract-list.component.html',
  styleUrls: ['./contract-list.component.css']
})
export class ContractListComponent implements OnInit {
  contracts: Contract[] = [];
  loading = false;
  error: string | null = null;
  selectedStatus: string = 'all';
  searchTerm: string = '';

  constructor(
    private contractService: ContractService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadContracts();
  }

  loadContracts(): void {
    this.loading = true;
    this.error = null;

    this.contractService.getContracts().subscribe({
      next: (data) => {
        this.contracts = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load contracts';
        this.loading = false;
        console.error('Contract loading error:', err);
      }
    });
  }

  getFilteredContracts(): Contract[] {
    let filtered = this.contracts;

    // Filter by status
    if (this.selectedStatus !== 'all') {
      filtered = filtered.filter(c => c.status === this.selectedStatus);
    }

    // Filter by search term
    if (this.searchTerm) {
      const term = this.searchTerm.toLowerCase();
      filtered = filtered.filter(c => 
        c.title.toLowerCase().includes(term) ||
        c.contractNumber.toLowerCase().includes(term) ||
        (c.counterparty && c.counterparty.toLowerCase().includes(term))
      );
    }

    return filtered;
  }

  getStatusClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'active':
        return 'bg-green-100 text-green-800';
      case 'pending':
      case 'under review':
        return 'bg-yellow-100 text-yellow-800';
      case 'expired':
        return 'bg-red-100 text-red-800';
      case 'executed':
        return 'bg-blue-100 text-blue-800';
      case 'draft':
        return 'bg-gray-100 text-gray-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  }

  getRiskClass(risk: string): string {
    switch (risk.toLowerCase()) {
      case 'low':
        return 'bg-green-100 text-green-800';
      case 'medium':
        return 'bg-yellow-100 text-yellow-800';
      case 'high':
        return 'bg-orange-100 text-orange-800';
      case 'critical':
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  }

  isContractExpiring(contract: Contract): boolean {
    if (!contract.endDate) return false;
    const warningPeriod = new Date();
    warningPeriod.setDate(warningPeriod.getDate() + 90); // 90 days warning
    return new Date(contract.endDate) <= warningPeriod && new Date(contract.endDate) > new Date();
  }

  viewContract(contract: Contract): void {
    this.router.navigate(['/contracts', contract.contractId]);
  }

  createContract(): void {
    this.router.navigate(['/contracts/create']);
  }

  formatCurrency(amount: number, currency: string): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: currency || 'USD'
    }).format(amount);
  }

  getContractsDueForRenewal(): void {
    this.contractService.getContractsDueForRenewal().subscribe({
      next: (data) => {
        this.contracts = data;
        this.selectedStatus = 'active';
      },
      error: (err) => {
        this.error = 'Failed to load contracts due for renewal';
        console.error('Renewal contracts loading error:', err);
      }
    });
  }
}
