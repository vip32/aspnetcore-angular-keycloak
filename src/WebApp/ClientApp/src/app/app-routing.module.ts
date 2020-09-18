import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AppAuthGuard } from './app.authguard';
import { CounterComponent } from './counter/counter.component';
import { FetchDataComponent } from './fetch-data/fetch-data.component';

const routes: Routes = [
    { path: 'counter', component: CounterComponent },
    { path: 'fetch-data', component: FetchDataComponent },
  //{
  //  path: '',
  //  redirectTo: '/home',
  //  pathMatch: 'full'
  //},
  //{
  //  path: 'home',
  //  component: HomeComponent,
  //  canActivate: [AppAuthGuard]
  //},
  //{
  //  path: 'heroes',
  //  component: HeroesComponent,
  //  canActivate: [AppAuthGuard]
  //}
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
  providers: [AppAuthGuard]
})
export class AppRoutingModule { }
