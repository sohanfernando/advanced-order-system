import { Component, DestroyRef, computed, inject, input, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import {
  FormControl,
  FormGroup,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize, forkJoin, map } from 'rxjs';
import { getErrorMessage } from '../../../core/http-error';
import { CreateOrderRequest, Customer, Product } from '../../../core/models';
import { CustomerService } from '../../../core/services/customer.service';
import { OrderService } from '../../../core/services/order.service';
import { ProductService } from '../../../core/services/product.service';
import { Alert } from '../../../shared/alert';
import { showError, wholeNumber } from '../../../shared/form-validators';
import { MoneyPipe, roundMoney } from '../../../shared/money';

type ItemForm = FormGroup<{
  productId: FormControl<number | null>;
  quantity: FormControl<number | null>;
}>;

interface OrderLine {
  product: Product | null;
  quantity: number;
  lineTotal: number;
  exceedsStock: boolean;
}

@Component({
  selector: 'app-order-create',
  imports: [ReactiveFormsModule, RouterLink, Alert, MoneyPipe],
  templateUrl: './order-create.html',
})
export class OrderCreate {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly orderService = inject(OrderService);
  private readonly customerService = inject(CustomerService);
  private readonly productService = inject(ProductService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  // Optional ?customerId= query param to preselect a customer
  readonly customerId = input<string>();

  protected readonly showError = showError;

  protected readonly customers = signal<Customer[]>([]);
  protected readonly products = signal<Product[]>([]);
  protected readonly loading = signal(true);
  protected readonly loadError = signal<string | null>(null);
  protected readonly submitting = signal(false);
  protected readonly submitError = signal<string | null>(null);
  protected readonly submitted = signal(false);

  protected readonly form = this.fb.group({
    customerId: this.fb.control<number | null>(null, Validators.required),
    discountPercent: this.fb.control<number | null>(0, [
      Validators.required,
      Validators.min(0),
      Validators.max(100),
    ]),
    items: this.fb.array<ItemForm>([]),
  });

  protected get items() {
    return this.form.controls.items;
  }

  private readonly formValue = toSignal(
    this.form.valueChanges.pipe(map(() => this.form.getRawValue())),
    { initialValue: this.form.getRawValue() },
  );

  private readonly productsById = computed(
    () => new Map(this.products().map((product) => [product.id, product])),
  );

  protected readonly lines = computed<OrderLine[]>(() =>
    this.formValue().items.map((item) => {
      const product =
        item.productId !== null ? (this.productsById().get(item.productId) ?? null) : null;
      const quantity = Number(item.quantity) || 0;

      return {
        product,
        quantity,
        lineTotal: product ? roundMoney(product.unitPrice * quantity) : 0,
        exceedsStock: product !== null && quantity > product.stock,
      };
    }),
  );

  protected readonly subtotal = computed(() =>
    roundMoney(this.lines().reduce((sum, line) => sum + line.lineTotal, 0)),
  );

  protected readonly discountPercent = computed(() => {
    const value = Number(this.formValue().discountPercent);

    return Number.isFinite(value) ? Math.min(Math.max(value, 0), 100) : 0;
  });

  protected readonly discountAmount = computed(() =>
    roundMoney((this.subtotal() * this.discountPercent()) / 100),
  );

  protected readonly total = computed(() => roundMoney(this.subtotal() - this.discountAmount()));

  protected readonly itemCount = computed(() =>
    this.lines().reduce((sum, line) => sum + (line.product ? line.quantity : 0), 0),
  );

  protected readonly hasStockProblem = computed(() => this.lines().some((line) => line.exceedsStock));

  protected readonly canAddItem = computed(
    () => this.formValue().items.length < this.products().length,
  );

  constructor() {
    this.addItem();
    this.loadData();
  }

  protected loadData(): void {
    this.loading.set(true);
    this.loadError.set(null);

    forkJoin({
      customers: this.customerService.getCustomers(),
      products: this.productService.getProducts(),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ customers, products }) => {
          this.customers.set(customers);
          this.products.set(products);
          this.preselectCustomer(customers);
          this.loading.set(false);
        },
        error: (error) => {
          this.loadError.set(getErrorMessage(error));
          this.loading.set(false);
        },
      });
  }

  protected addItem(): void {
    this.items.push(
      this.fb.group({
        productId: this.fb.control<number | null>(null, Validators.required),
        quantity: this.fb.control<number | null>(1, [
          Validators.required,
          Validators.min(1),
          wholeNumber,
        ]),
      }),
    );
  }

  protected removeItem(index: number): void {
    if (this.items.length > 1) {
      this.items.removeAt(index);
    }
  }

  // A product can only appear once per order, so hide it from the other rows
  protected isProductTaken(productId: number, rowIndex: number): boolean {
    return this.formValue().items.some(
      (item, index) => index !== rowIndex && item.productId === productId,
    );
  }

  protected submit(): void {
    this.submitted.set(true);
    this.submitError.set(null);

    if (this.form.invalid || this.hasStockProblem() || this.submitting()) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();

    const request: CreateOrderRequest = {
      customerId: value.customerId!,
      discountPercent: Number(value.discountPercent),
      items: value.items.map((item) => ({
        productId: item.productId!,
        quantity: Number(item.quantity),
      })),
    };

    this.submitting.set(true);

    this.orderService
      .createOrder(request)
      .pipe(
        finalize(() => this.submitting.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (order) => {
          this.router.navigate(['/orders', order.id], { state: { created: true } });
        },
        error: (error) => {
          this.submitError.set(getErrorMessage(error));
          // Stock may have changed since the page loaded
          this.refreshProducts();
        },
      });
  }

  private refreshProducts(): void {
    this.productService
      .getProducts()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({ next: (products) => this.products.set(products) });
  }

  private preselectCustomer(customers: Customer[]): void {
    const id = Number(this.customerId());
    const control = this.form.controls.customerId;

    if (control.value === null && customers.some((customer) => customer.id === id)) {
      control.setValue(id);
    }
  }
}
