import { Component, OnInit } from '@angular/core';
import { KeycloakProfile } from 'keycloak-js';
import { KeycloakService, KeycloakEventType } from 'keycloak-angular';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})

export class AppComponent implements OnInit {
  title = 'WebApp';
  userDetails: KeycloakProfile;

  constructor(private keycloakService: KeycloakService) { }

  async ngOnInit() {
    if (await this.keycloakService.isLoggedIn()) {
      this.userDetails = await this.keycloakService.loadUserProfile();
      console.log("auth: account=", this.userDetails);
      console.log("auth: roles=", this.keycloakService.getUserRoles(true));
      console.log("auth: token=", this.keycloakService.getKeycloakInstance().token);
      console.log("auth: keycloak=", this.keycloakService.getKeycloakInstance());
    }
  }

  async doLogout() {
    await this.keycloakService.logout();
  }
}
