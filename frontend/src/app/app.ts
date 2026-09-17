import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { APP_NAME } from './core/config';

interface NavLink {
  label: string;
  path: string;
}

@Component({
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  selector: 'app-root',
  templateUrl: './app.html',
})
export class App {
  protected readonly appName = APP_NAME;

  protected readonly navLinks: NavLink[] = [
    { label: 'Orders', path: '/orders' },
    { label: 'Products', path: '/products' },
    { label: 'Customers', path: '/customers' },
  ];
}
