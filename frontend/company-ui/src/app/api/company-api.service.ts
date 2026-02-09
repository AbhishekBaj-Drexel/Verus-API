import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { CompanyDto, CompanyQueryParams, CompanySearchResultDto, CreateCompanyRequestModel } from './models';

@Injectable({ providedIn: 'root' })
export class CompanyApiService {
  private readonly http = inject(HttpClient);
  private readonly companiesUrl = '/api/companies';

  createCompany(request: CreateCompanyRequestModel): Observable<CompanyDto> {
    return this.http.post<CompanyDto>(this.companiesUrl, request);
  }

  getCompanies(query: CompanyQueryParams = {}): Observable<CompanyDto[]> {
    let params = new HttpParams();
    if (query.name?.trim()) {
      params = params.set('name', query.name.trim());
    }

    if (query.domain?.trim()) {
      params = params.set('domain', query.domain.trim());
    }

    const freeText = query.q?.trim();
    if (freeText) {
      params = params.set('q', freeText);
    }

    return this.http
      .get<CompanySearchResultDto[]>(this.companiesUrl, { params })
      .pipe(map((results) => results.map((result) => result.company)));
  }

  getCompanyById(id: string): Observable<CompanyDto> {
    return this.http.get<CompanyDto>(`${this.companiesUrl}/${id}`);
  }
}
