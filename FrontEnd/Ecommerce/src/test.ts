import 'zone.js/testing';
import { getTestBed } from '@angular/core/testing';

import { BrowserTestingModule ,platformBrowserTesting} from '@angular/platform-browser/testing';

declare const require: {
  context(path: string, deep?: boolean, filter?: RegExp): {
    keys(): string[];
    <T>(id: string): T;
  };
};

getTestBed().initTestEnvironment(
  BrowserTestingModule,
  platformBrowserTesting()
);

// Then we find all the tests.
const context = require.context('./', true, /\.spec\.ts$/);
// And load the modules.
context.keys().forEach(context);
