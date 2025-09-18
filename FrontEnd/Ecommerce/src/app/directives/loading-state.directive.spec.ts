import { TemplateRef, ViewContainerRef } from '@angular/core';
import { LoadingStateDirective } from './loading-state.directive';

describe('LoadingStateDirective', () => {
  let directive: LoadingStateDirective;
  let mockTemplateRef: jest.Mocked<TemplateRef<any>>;
  let mockViewContainer: jest.Mocked<ViewContainerRef>;

  beforeEach(() => {
    mockTemplateRef = { createEmbeddedView: jest.fn() } as any;
    mockViewContainer = { createEmbeddedView: jest.fn(), clear: jest.fn() } as any;

    directive = new LoadingStateDirective(mockTemplateRef, mockViewContainer);
  });

  it('should create an instance', () => {
    expect(directive).toBeTruthy();
  });

  it('should create view when condition is false', () => {
    directive.appLoadingState = false;

    expect(mockViewContainer.createEmbeddedView).toHaveBeenCalledWith(mockTemplateRef);
  });

  it('should clear view when condition is true', () => {
    // First create a view
    directive.appLoadingState = false;

    // Then clear it
    directive.appLoadingState = true;

    expect(mockViewContainer.clear).toHaveBeenCalled();
  });
});
