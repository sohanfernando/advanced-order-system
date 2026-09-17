export const ORDER_STATUSES = ['Confirmed', 'Cancelled'] as const;

export type OrderStatus = (typeof ORDER_STATUSES)[number];

export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

export interface Product {
  id: number;
  name: string;
  sku: string;
  unitPrice: number;
  stock: number;
  isActive: boolean;
  createdAt: string;
}

export interface CreateProductRequest {
  name: string;
  sku: string;
  unitPrice: number;
  stock: number;
}

export interface Customer {
  id: number;
  name: string;
  email: string;
  createdAt: string;
}

export interface OrderItem {
  id: number;
  productId: number;
  productName: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface Order {
  id: number;
  customerId: number;
  customerName: string;
  orderDate: string;
  subtotal: number;
  discountAmount: number;
  total: number;
  status: OrderStatus;
  items: OrderItem[];
}

export interface OrderSummary {
  id: number;
  customerName: string;
  orderDate: string;
  total: number;
  status: OrderStatus;
}

export interface OrderQuery {
  status: OrderStatus | null;
  customerId: number | null;
  page: number;
  pageSize: number;
}

export interface CreateOrderRequest {
  customerId: number;
  discountPercent: number;
  items: { productId: number; quantity: number }[];
}
