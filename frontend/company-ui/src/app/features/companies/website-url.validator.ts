import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export function websiteUrlValidator(): ValidatorFn {
  return (control: AbstractControl<string>): ValidationErrors | null => {
    const value = control.value?.trim();
    if (!value) {
      return null;
    }

    try {
      const url = new URL(value);
      const isAllowedScheme = url.protocol === 'http:' || url.protocol === 'https:';
      return isAllowedScheme ? null : { websiteUrl: true };
    } catch {
      return { websiteUrl: true };
    }
  };
}
