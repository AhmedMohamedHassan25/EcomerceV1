import { TruncatePipe } from './truncate.pipe';

describe('TruncatePipe (Cypress)', () => {
  it('should truncate text longer than limit', () => {
    const pipe = new TruncatePipe();
    const result = pipe.transform('This is a very long text', 10);
    expect(result).to.equal('This is a ...');
  });

  it('should return original text if shorter than limit', () => {
    const pipe = new TruncatePipe();
    expect(pipe.transform('Short text', 20)).to.equal('Short text');
  });
});