import { Directive, ElementRef, EventEmitter, OnInit, OnDestroy, Output, Input } from '@angular/core';
import { fromEvent, Subject } from 'rxjs';
import { takeUntil, throttleTime } from 'rxjs/operators';

@Directive({
  selector: '[appInfiniteScroll]'
})
export class InfiniteScrollDirective implements OnInit, OnDestroy {
  @Output() scrolled = new EventEmitter<void>();
  @Input() threshold = 100;
  @Input() disabled = false;

  private destroy$ = new Subject<void>();

  constructor(private element: ElementRef) {}

  ngOnInit(): void {
    fromEvent(window, 'scroll')
      .pipe(
        throttleTime(300),
        takeUntil(this.destroy$)
      )
      .subscribe(() => this.onScroll()); // call public method
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /** Public method to allow manual triggering of scroll check */
  public onScroll(): void {
    if (!this.disabled && this.isNearBottom()) {
      this.scrolled.emit();
    }
  }

  private isNearBottom(): boolean {
    const windowHeight = window.innerHeight;
    const documentHeight = document.documentElement.scrollHeight;
    const scrollTop = window.scrollY;

    return (windowHeight + scrollTop) >= (documentHeight - this.threshold);
  }
}
