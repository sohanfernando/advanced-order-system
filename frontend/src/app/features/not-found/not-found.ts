import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-not-found',
  imports: [RouterLink],
  template: `
    <div class="card px-6 py-16 text-center">
      <p class="text-sm font-semibold text-indigo-600">404</p>
      <h1 class="page-title mt-2">Page not found</h1>
      <p class="mt-2 text-sm text-slate-500">The page you are looking for doesn't exist.</p>
      <a routerLink="/orders" class="btn btn-primary mt-6">Go to orders</a>
    </div>
  `,
})
export class NotFound {}
