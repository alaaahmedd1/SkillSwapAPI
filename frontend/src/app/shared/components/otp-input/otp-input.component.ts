import {
  Component,
  ElementRef,
  output,
  signal,
  viewChildren,
} from '@angular/core';

@Component({
  selector: 'app-otp-input',
  imports: [],
  template: `
    <div class="otp-row" (paste)="onPaste($event)">
      @for (digit of digits(); track $index) {
        <input
          #box
          class="otp-box"
          [class.filled]="digit !== ''"
          type="text"
          inputmode="numeric"
          maxlength="1"
          autocomplete="one-time-code"
          [value]="digit"
          [attr.aria-label]="'Digit ' + ($index + 1)"
          (input)="onInput($index, $event)"
          (keydown)="onKeydown($index, $event)"
          (focus)="onFocus($index)"
        />
      }
    </div>
  `,
  styles: `
    .otp-row {
      display: flex;
      gap: 12px;
      justify-content: center;
    }

    .otp-box {
      width: 48px;
      height: 58px;
      border: 1.5px solid var(--border);
      border-radius: var(--radius-sm);
      background: #fff;
      text-align: center;
      font-size: 22px;
      font-weight: 700;
      color: var(--text);
      outline: none;
      transition: border-color 0.15s ease, box-shadow 0.15s ease, background 0.15s ease;

      &:focus {
        border-color: var(--primary);
        box-shadow: 0 0 0 4px rgba(124, 92, 252, 0.14);
      }

      &.filled {
        border-color: var(--primary);
        background: var(--primary-soft);
      }
    }

    @media (max-width: 380px) {
      .otp-box {
        width: 42px;
        height: 52px;
      }
    }
  `,
})
export class OtpInputComponent {
  readonly length = 6;
  readonly digits = signal<string[]>(Array(this.length).fill(''));
  readonly valueChange = output<string>();

  private readonly boxes = viewChildren<ElementRef<HTMLInputElement>>('box');

  get value(): string {
    return this.digits().join('');
  }

  clear(): void {
    this.digits.set(Array(this.length).fill(''));
    this.valueChange.emit('');
    this.boxes()[0]?.nativeElement.focus();
  }

  onInput(index: number, event: Event): void {
    const input = event.target as HTMLInputElement;
    const clean = input.value.replace(/\D/g, '');
    const next = [...this.digits()];

    if (clean.length > 1) {
      // Multi-char (autofill): spread across boxes starting here.
      clean.slice(0, this.length - index).split('').forEach((ch, i) => {
        next[index + i] = ch;
      });
      const lastIndex = Math.min(index + clean.length, this.length) - 1;
      this.boxes()[lastIndex]?.nativeElement.focus();
    } else {
      next[index] = clean;
      if (clean && index < this.length - 1) {
        this.boxes()[index + 1]?.nativeElement.focus();
      }
    }

    this.digits.set(next);
    this.valueChange.emit(next.join(''));
  }

  onKeydown(index: number, event: KeyboardEvent): void {
    if (event.key === 'Backspace' && !this.digits()[index] && index > 0) {
      const next = [...this.digits()];
      next[index - 1] = '';
      this.digits.set(next);
      this.valueChange.emit(next.join(''));
      this.boxes()[index - 1]?.nativeElement.focus();
    }
    if (event.key === 'ArrowLeft' && index > 0) {
      this.boxes()[index - 1]?.nativeElement.focus();
    }
    if (event.key === 'ArrowRight' && index < this.length - 1) {
      this.boxes()[index + 1]?.nativeElement.focus();
    }
  }

  onFocus(index: number): void {
    this.boxes()[index]?.nativeElement.select();
  }

  onPaste(event: ClipboardEvent): void {
    event.preventDefault();
    const text = (event.clipboardData?.getData('text') ?? '').replace(/\D/g, '').slice(0, this.length);
    if (!text) return;
    const next = Array(this.length).fill('');
    text.split('').forEach((ch, i) => (next[i] = ch));
    this.digits.set(next);
    this.valueChange.emit(next.join(''));
    this.boxes()[Math.min(text.length, this.length - 1)]?.nativeElement.focus();
  }
}
