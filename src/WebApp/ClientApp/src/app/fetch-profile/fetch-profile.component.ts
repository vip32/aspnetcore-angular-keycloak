import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-fetch-profile',
  templateUrl: './fetch-profile.component.html'
})
export class FetchProfileComponent {
  public content: object;

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    console.log('profile: get=', baseUrl + 'api/v1/userprofile');
    http.get<object>(baseUrl + 'api/v1/userprofile').subscribe(result => { // TODO: use generated API client
      console.log('profile: data=', result);
      this.content = result;
    }, error => console.error(error));
  }
}
