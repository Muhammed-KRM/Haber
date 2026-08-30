import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'imageUrl',
  standalone: true
})
export class ImageUrlPipe implements PipeTransform {
  private apiBase = 'https://localhost:7000';

  transform(value: string | undefined | null): string {
    if (!value) return '';
    if (value.startsWith('http://') || value.startsWith('https://')) {
      return value;
    }
    if (value.startsWith('/')) {
      return `${this.apiBase}${value}`;
    }
    return `${this.apiBase}/${value}`;
  }
}
