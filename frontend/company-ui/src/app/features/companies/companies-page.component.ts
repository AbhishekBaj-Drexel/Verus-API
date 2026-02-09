import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { CompanyApiService } from '../../api/company-api.service';
import { CompanyDto, CompanyQueryParams } from '../../api/models';
import { CompanyFormComponent } from './company-form.component';
import { CompanyListComponent } from './company-list.component';

@Component({
  selector: 'app-companies-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, CompanyFormComponent, CompanyListComponent],
  templateUrl: './companies-page.component.html',
})
export class CompaniesPageComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly companyApi = inject(CompanyApiService);

  protected readonly companies = signal<CompanyDto[]>([]);
  protected readonly loading = signal(false);
  protected readonly loadError = signal<string | null>(null);
  protected readonly searchForm = this.fb.nonNullable.group({
    name: [''],
    domain: [''],
    q: [''],
  });

  ngOnInit(): void {
    this.search();
  }

  protected refreshCompanies(): void {
    this.search();
  }

  protected search(): void {
    this.loadCompanies(this.searchForm.getRawValue());
  }

  protected clearSearch(): void {
    this.searchForm.reset({
      name: '',
      domain: '',
      q: '',
    });

    this.loadCompanies();
  }

  private loadCompanies(query: CompanyQueryParams = {}): void {
    this.loading.set(true);
    this.loadError.set(null);

    this.companyApi.getCompanies(query).subscribe({
      next: (companies) => {
        this.companies.set(companies);
      },
      error: (error: HttpErrorResponse) => {
        this.loadError.set(this.toLoadErrorMessage(error));
      },
      complete: () => {
        this.loading.set(false);
      },
    });
  }

  private toLoadErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 0) {
      return 'Could not connect to the API.';
    }

    return 'Failed to load companies.';
  }
}
