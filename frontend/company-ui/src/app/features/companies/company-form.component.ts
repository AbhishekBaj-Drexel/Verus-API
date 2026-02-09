import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, EventEmitter, Output, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { CompanyApiService } from '../../api/company-api.service';
import { ValidationErrorResponse } from '../../api/models';
import { websiteUrlValidator } from './website-url.validator';

@Component({
  selector: 'app-company-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './company-form.component.html',
})
export class CompanyFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly companyApi = inject(CompanyApiService);

  @Output() created = new EventEmitter<void>();

  protected readonly submitting = signal(false);
  protected readonly successMessage = signal<string | null>(null);
  protected readonly generalErrorMessage = signal<string | null>(null);
  protected readonly backendErrors = signal<Record<string, string[]>>({});

  protected readonly form = this.fb.nonNullable.group({
    companyName: ['', [Validators.required, Validators.minLength(3)]],
    websiteUrl: ['', [Validators.required, websiteUrlValidator()]],
  });

  protected submit(): void {
    if (this.form.invalid || this.submitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.successMessage.set(null);
    this.generalErrorMessage.set(null);
    this.backendErrors.set({});

    this.companyApi
      .createCompany(this.form.getRawValue())
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        next: () => {
          this.successMessage.set('Company created successfully.');
          this.form.reset({ companyName: '', websiteUrl: '' });
          this.created.emit();
        },
        error: (error: HttpErrorResponse) => {
          const { message, errors } = this.toApiErrors(error);
          this.generalErrorMessage.set(message);
          this.backendErrors.set(errors);
        },
      });
  }

  protected getFieldErrors(field: 'companyName' | 'websiteUrl'): string[] {
    const backendErrors = this.backendErrors();
    const targetKey = field.toLowerCase();

    return Object.entries(backendErrors)
      .filter(([key]) => key.toLowerCase() === targetKey)
      .flatMap(([, errors]) => errors);
  }

  private toApiErrors(error: HttpErrorResponse): {
    message: string;
    errors: Record<string, string[]>;
  } {
    if (error.status === 0) {
      return {
        message: 'The API is not reachable. Make sure the backend is running.',
        errors: {},
      };
    }

    const payload = error.error as ValidationErrorResponse | undefined;
    if (payload?.message && payload.errors) {
      return {
        message: payload.message,
        errors: payload.errors,
      };
    }

    return {
      message: 'Failed to create company.',
      errors: {},
    };
  }
}
