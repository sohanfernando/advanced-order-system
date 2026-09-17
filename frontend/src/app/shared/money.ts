import { Pipe, PipeTransform } from '@angular/core';
import { CURRENCY_SYMBOL } from '../core/config';

const formatter = new Intl.NumberFormat('en-US', {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
});

// Rounds to 2 decimals the same way the backend does (.NET Math.Round uses banker's rounding)
export function roundMoney(value: number): number {
  const scaled = value * 100;
  const floor = Math.floor(scaled);

  if (Math.abs(scaled - floor - 0.5) < 1e-9) {
    return (floor % 2 === 0 ? floor : floor + 1) / 100;
  }

  return Math.round(scaled) / 100;
}

@Pipe({ name: 'money' })
export class MoneyPipe implements PipeTransform {
  transform(value: number | null | undefined): string {
    if (value === null || value === undefined) {
      return '';
    }

    return `${CURRENCY_SYMBOL} ${formatter.format(value)}`;
  }
}
