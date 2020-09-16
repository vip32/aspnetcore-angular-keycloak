import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AppAuthGuard } from './app.authguard';

const routes: Routes = [
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
