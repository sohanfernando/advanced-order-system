import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/auth/auth.guards';
import { MainLayout } from './layout/main-layout';

export const routes: Routes = [
  {
    path: 'login',
    title: 'Sign in',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/login/login').then((m) => m.Login),
  },
  {
    path: 'register',
    title: 'Create account',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/register/register').then((m) => m.Register),
  },
  {
    // Everything below requires a signed-in admin
    path: '',
    component: MainLayout,
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'orders' },
      {
        path: 'orders',
        title: 'Orders',
        loadComponent: () =>
          import('./features/orders/order-list/order-list').then((m) => m.OrderList),
      },
      {
        path: 'orders/new',
        title: 'New order',
        loadComponent: () =>
          import('./features/orders/order-create/order-create').then((m) => m.OrderCreate),
      },
      {
        path: 'orders/:id',
        title: 'Order details',
        loadComponent: () =>
          import('./features/orders/order-detail/order-detail').then((m) => m.OrderDetail),
      },
      {
        path: 'products',
        title: 'Products',
        loadComponent: () =>
          import('./features/products/product-list/product-list').then((m) => m.ProductList),
      },
      {
        path: 'customers',
        title: 'Customers',
        loadComponent: () =>
          import('./features/customers/customer-list/customer-list').then(
            (m) => m.CustomerList,
          ),
      },
      {
        path: '**',
        title: 'Page not found',
        loadComponent: () => import('./features/not-found/not-found').then((m) => m.NotFound),
      },
    ],
  },
];
