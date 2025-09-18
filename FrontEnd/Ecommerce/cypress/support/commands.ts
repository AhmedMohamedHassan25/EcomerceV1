/// <reference types="cypress" />
// ***********************************************
// This example commands.ts shows you how to
// create various custom commands and overwrite
// existing commands.
//
// For more comprehensive examples of custom
// commands please read more here:
// https://on.cypress.io/custom-commands
// ***********************************************
//
//
// -- This is a parent command --
// Cypress.Commands.add('login', (email, password) => { ... })
//
//
// -- This is a child command --
// Cypress.Commands.add('drag', { prevSubject: 'element'}, (subject, options) => { ... })
//
//
// -- This is a dual command --
// Cypress.Commands.add('dismiss', { prevSubject: 'optional'}, (subject, options) => { ... })
//
//
// -- This will overwrite an existing command --
// Cypress.Commands.overwrite('visit', (originalFn, url, options) => { ... })
//
// declare global {
//   namespace Cypress {
//     interface Chainable {
//       login(email: string, password: string): Chainable<void>
//       drag(subject: string, options?: Partial<TypeOptions>): Chainable<Element>
//       dismiss(subject: string, options?: Partial<TypeOptions>): Chainable<Element>
//       visit(originalFn: CommandOriginalFn, url: string, options: Partial<VisitOptions>): Chainable<Element>
//     }
//   }
// }
export {};

declare global {
  namespace Cypress {
    interface Chainable {
      dataCy(selector: string): Chainable<JQuery<HTMLElement>>;
      loginByAPI(username: string, password: string): Chainable<any>;
      setAuthTokens(tokens: { accessToken: string; refreshToken: string }): Chainable<any>;
    }
  }
}

Cypress.Commands.add('dataCy', (selector: string) => {
  return cy.get(`[data-cy="${selector}"]`);
});

Cypress.Commands.add('loginByAPI', (username: string, password: string) => {
  return cy.request({
    method: 'POST',
    url: '/api/auth/login',
    body: { username, password }
  }).then((response) => {
    const res = response as { body: { token: string; refreshToken: string; user: any } };

    window.localStorage.setItem('access_token', res.body.token);
    window.localStorage.setItem('refresh_token', res.body.refreshToken);
    window.localStorage.setItem('current_user', JSON.stringify(res.body.user));
  });
});

Cypress.Commands.add('setAuthTokens', (tokens: { accessToken: string; refreshToken: string }) => {
  window.localStorage.setItem('access_token', tokens.accessToken);
  window.localStorage.setItem('refresh_token', tokens.refreshToken);
});
