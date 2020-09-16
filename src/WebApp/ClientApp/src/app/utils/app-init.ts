import { KeycloakEventType, KeycloakService } from 'keycloak-angular';
import { from } from 'rxjs';
import { filter } from 'rxjs/operators';
import { environment } from '../../environments/environment';

export function initializer(keycloak: KeycloakService): () => Promise<any> {
  return (): Promise<any> => {
    return new Promise(async (resolve, reject) => {
      const { keycloakConfig } = environment;
      try {
        await keycloak.init({
          config: keycloakConfig,
          initOptions: {
            onLoad: 'login-required',
            checkLoginIframe: false
          },
          bearerExcludedUrls: []
        });

        from(keycloak.keycloakEvents$)
          .pipe(filter(event => event.type === KeycloakEventType.OnAuthSuccess))
          .subscribe(() => {
            console.log('auth: success');
          });

        from(keycloak.keycloakEvents$)
          .pipe(filter(event => event.type === KeycloakEventType.OnAuthRefreshSuccess))
          .subscribe(() => {
            console.log('auth: refresh success');
          });

        from(keycloak.keycloakEvents$)
          .pipe(filter(event => event.type === KeycloakEventType.OnTokenExpired))
          .subscribe(() => {
            console.log('auth: token has expired');
            if (keycloak.getKeycloakInstance().refreshToken) {
              console.log("auth: update token");
              keycloak.updateToken(0)
                .then(_ => {
                  console.log("auth: token=", keycloak.getKeycloakInstance().token);
                })
                .catch(e => { console.error(e) })
            } else {
              console.log("auth: force login");
              keycloak.login()
                .then(_ => {
                  console.log("auth: token=", keycloak.getKeycloakInstance().token);
                })
                .catch(e => { console.error(e) })
            }
          })

        resolve();
      } catch (error) {
        reject(error);
      }
    });
  };
}
