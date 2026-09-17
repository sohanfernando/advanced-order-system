import { Location } from '@angular/common';
import { Component, DestroyRef, inject, input, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { safeReturnUrl } from '../../../core/auth/auth.guards';
import { AuthService } from '../../../core/auth/auth.service';
import { getErrorMessage } from '../../../core/http-error';
import { Alert } from '../../../shared/alert';
import { showError } from '../../../shared/form-validators';
import { AuthCard } from '../auth-card';

interface LoginPageState {
  registered?: boolean;
  email?: string;
}

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink, Alert, AuthCard],
  templateUrl: './login.html',
})
export class Login {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  // Bound from ?returnUrl=
  readonly returnUrl = input<string>();

  protected readonly showError = showError;

  // Set when arriving from the register page
  private readonly pageState = (inject(Location).getState() ?? {}) as LoginPageState;

  protected readonly successMessage = signal(
    this.pageState.registered ? 'Account created. You can sign in now.' : null,
  );
  protected readonly error = signal<string | null>(null);
  protected readonly submitting = signal(false);
  protected readonly submitted = signal(false);
  protected readonly showPassword = signal(false);

  protected readonly form = this.fb.group({
    email: [this.pageState.email ?? '', [Validators.required, Validators.email]],
    password: ['', Validators.required],
    rememberMe: [false],
  });

  protected togglePassword(): void {
    this.showPassword.update((visible) => !visible);
  }

  protected submit(): void {
    this.submitted.set(true);
    this.error.set(null);

    if (this.form.invalid || this.submitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.successMessage.set(null);

    this.auth
      .login(this.form.getRawValue())
      .pipe(
        finalize(() => this.submitting.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: () => this.router.navigateByUrl(safeReturnUrl(this.returnUrl())),
        error: (error) => {
          this.error.set(getErrorMessage(error));
          this.form.controls.password.reset();
        },
      });
  }
}
