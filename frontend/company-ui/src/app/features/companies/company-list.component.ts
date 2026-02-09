import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { CompanyDto } from '../../api/models';

@Component({
  selector: 'app-company-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './company-list.component.html',
})
export class CompanyListComponent {
  @Input({ required: true }) companies: CompanyDto[] = [];
}
