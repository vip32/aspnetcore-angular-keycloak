import { KeycloakConfig } from 'keycloak-js';

let keycloakConfig: KeycloakConfig = {
  url: 'http://localhost:8080/auth/',
  realm: 'master',
  clientId: 'demo2'
};

export const environment = {
  production: true,
  keycloak: keycloakConfig
};
