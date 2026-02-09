export interface CreateCompanyRequestModel {
  companyName: string;
  websiteUrl: string;
}

export interface CompanyDto {
  id: string;
  companyName: string;
  websiteUrl: string;
  websiteDomain: string;
  createdAt: string;
}

export interface CompanySearchResultDto {
  company: CompanyDto;
  relevanceScore: number;
  scoreReasons: string[];
}

export interface CompanyQueryParams {
  name?: string;
  domain?: string;
  q?: string;
}

export interface ValidationErrorResponse {
  message: string;
  errors: Record<string, string[]>;
}
