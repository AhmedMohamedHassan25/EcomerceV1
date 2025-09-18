import { CurrencyFormatterPipe } from './currency-fromatter.pipe';

describe('CurrencyFormatterPipe', () => {
  let pipe: CurrencyFormatterPipe;

  beforeEach(() => {
    pipe = new CurrencyFormatterPipe();
  });

  it('create an instance', () => {
    expect(pipe).toBeTruthy();
  });

  it('should format USD currency by default', () => {
    const result = pipe.transform(100);
    expect(result).toBe('$100.00');
  });

  it('should format different currencies', () => {
    const result = pipe.transform(100, 'EUR', 'en-US');
    expect(result).toBe('€100.00');
  });

  it('should handle zero value', () => {
    const result = pipe.transform(0);
    expect(result).toBe('$0.00');
  });

  it('should handle null/undefined/NaN', () => {
    expect(pipe.transform(null as any)).toBe('');
    expect(pipe.transform(undefined as any)).toBe('');
    expect(pipe.transform(NaN)).toBe('');
  });

  it('should format decimal values correctly', () => {
    const result = pipe.transform(99.99);
    expect(result).toBe('$99.99');
  });
});
