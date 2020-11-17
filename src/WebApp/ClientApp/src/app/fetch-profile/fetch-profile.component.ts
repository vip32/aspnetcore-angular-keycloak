import { Component, Inject } from '@angular/core';
import { KeycloakService } from 'keycloak-angular';
import { UserProfileClient } from 'src/app/shared/api.generated.client';

@Component({
  selector: 'app-fetch-profile',
  templateUrl: './fetch-profile.component.html',
})
export class FetchProfileComponent {
  public content: object;
  public accessToken: string;
  public roles: string[];

  constructor(
    private userProfileClient: UserProfileClient,
    private keycloakService: KeycloakService) {
    console.log('get userprofile');
    this.userProfileClient.get().subscribe(
      (result) => {
        console.log('profile: data=', result);
        this.content = result;
        this.accessToken = this.keycloakService.getKeycloakInstance().token;
        this.roles = this.keycloakService.getUserRoles(true);
      },
      (error) => console.error(error)
    );
  }
}
