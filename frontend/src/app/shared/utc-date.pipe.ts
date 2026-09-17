import { Pipe, PipeTransform } from '@angular/core';

const TIME_ZONE_SUFFIX = /(Z|[+-]\d{2}:?\d{2})$/i;

// The API stores UTC times, but dates read back from the database have no "Z" suffix,
// which browsers would otherwise parse as local time
@Pipe({ name: 'utcDate' })
export class UtcDatePipe implements PipeTransform {
  transform(value: string | null | undefined): Date | null {
    if (!value) {
      return null;
    }

    return new Date(TIME_ZONE_SUFFIX.test(value) ? value : `${value}Z`);
  }
}
