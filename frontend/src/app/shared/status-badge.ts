import { Component, computed, input } from '@angular/core';
import { OrderStatus } from '../core/models';

const STATUS_CLASSES: Record<OrderStatus, string> = {
  Confirmed: 'bg-emerald-50 text-emerald-700 ring-emerald-600/20',
  Cancelled: 'bg-rose-50 text-rose-700 ring-rose-600/20',
};

@Component({
  selector: 'app-status-badge',
  template: `
    <span
      class="inline-flex items-center gap-1.5 rounded-full px-2.5 py-0.5 text-xs font-medium ring-1 ring-inset"
      [class]="classes()"
    >
      <span class="size-1.5 rounded-full bg-current" aria-hidden="true"></span>
      {{ status() }}
    </span>
  `,
})
export class StatusBadge {
  readonly status = input.required<OrderStatus>();

  protected readonly classes = computed(
    () => STATUS_CLASSES[this.status()] ?? 'bg-slate-50 text-slate-700 ring-slate-600/20',
  );
}
