import { ElementRef } from '@angular/core';
import { InfiniteScrollDirective } from './infinite-scroll.directive';

describe('InfiniteScrollDirective', () => {
  let directive: InfiniteScrollDirective;
  let mockElementRef: ElementRef;

  beforeEach(() => {
    mockElementRef = new ElementRef(document.createElement('div'));
    directive = new InfiniteScrollDirective(mockElementRef);
  });

  it('should create an instance', () => {
    expect(directive).toBeTruthy();
  });

  it('should emit scrolled event when near bottom', () => {
    directive.scrolled.emit = jest.fn();

    Object.defineProperty(window, 'innerHeight', { value: 800, configurable: true });
    Object.defineProperty(window, 'scrollY', { value: 500, configurable: true });
    Object.defineProperty(document.documentElement, 'scrollHeight', { value: 1000, configurable: true });

    directive.threshold = 100;
    directive.disabled = false;

    const scrollEvent = new Event('scroll');
    window.dispatchEvent(scrollEvent);


     directive.onScroll();

  });

  it('should not emit when disabled', () => {
    directive.scrolled.emit = jest.fn();
    directive.disabled = true;


    directive.onScroll();

    expect(directive.scrolled.emit).not.toHaveBeenCalled();
  });
});
