import { TruncatePipe } from './truncate.pipe';

describe('TruncatePipe', () => {
  let pipe: TruncatePipe;

  beforeEach(() => {
    pipe = new TruncatePipe();
  });

  it('create an instance', () => {
    expect(pipe).toBeTruthy();
  });

  it('should truncate text longer than limit', () => {
    const result = pipe.transform('This is a very long text', 10);
    expect(result).toBe('This is a ...');
  });

  it('should return original text if shorter than limit', () => {
    const result = pipe.transform('Short text', 20);
    expect(result).toBe('Short text');
  });

  it('should handle empty string', () => {
    const result = pipe.transform('', 10);
    expect(result).toBe('');
  });

  it('should handle null/undefined', () => {
    expect(pipe.transform(null as any)).toBe('');
    expect(pipe.transform(undefined as any)).toBe('');
  });

  it('should use custom trail', () => {
    const result = pipe.transform('Long text here', 5, '***');
    expect(result).toBe('Long ***');
  });
});
