import { AbstractControl, ValidationErrors } from '@angular/forms';

export function notBlank(control: AbstractControl): ValidationErrors | null {
  const value = control.value;

  return typeof value === 'string' && value.length > 0 && value.trim().length === 0
    ? { blank: true }
    : null;
}

export function wholeNumber(control: AbstractControl): ValidationErrors | null {
  const value = control.value;

  if (value === null || value === undefined || value === '') {
    return null;
  }

  return Number.isInteger(Number(value)) ? null : { wholeNumber: true };
}

// True when a control's error should be shown: after it was touched or the form was submitted
export function showError(control: AbstractControl, submitted: boolean): boolean {
  return control.invalid && (control.touched || submitted);
}

export const PASSWORD_MIN_LENGTH = 8;
export const PASSWORD_MAX_LENGTH = 128;

// Same rules as the backend's AuthService
export const PASSWORD_RULES: { label: string; test: (value: string) => boolean }[] = [
  {
    label: `At least ${PASSWORD_MIN_LENGTH} characters`,
    test: (value) => value.length >= PASSWORD_MIN_LENGTH,
  },
  { label: 'An upper-case letter', test: (value) => /\p{Lu}/u.test(value) },
  { label: 'A lower-case letter', test: (value) => /\p{Ll}/u.test(value) },
  { label: 'A number', test: (value) => /\p{Nd}/u.test(value) },
];

export function strongPassword(control: AbstractControl): ValidationErrors | null {
  const value = typeof control.value === 'string' ? control.value : '';

  // Empty values are handled by Validators.required
  if (!value) {
    return null;
  }

  const valid =
    value.length <= PASSWORD_MAX_LENGTH && PASSWORD_RULES.every((rule) => rule.test(value));

  return valid ? null : { strongPassword: true };
}

// Group validator: "password" and "confirmPassword" must match
export function passwordsMatch(group: AbstractControl): ValidationErrors | null {
  const password = group.get('password')?.value;
  const confirmPassword = group.get('confirmPassword')?.value;

  return password && confirmPassword && password !== confirmPassword
    ? { passwordsMismatch: true }
    : null;
}
