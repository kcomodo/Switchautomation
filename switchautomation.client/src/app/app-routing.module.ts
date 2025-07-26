import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './components/home/home.component';
import { LoginComponent } from './components/login/login.component';
import { PortdetailsComponent } from './components/portdetails/portdetails.component';
import { ConfigurationInfoComponent } from './components/configuration-info/configuration-info.component';

const routes: Routes = [
  {
    path: 'login',
    component:LoginComponent
  },
  {
    path: 'home',
    component: HomeComponent
  },
  {
    path: 'portdetails',
    component: PortdetailsComponent
  },
  {
    path: 'configInfo',
    component: ConfigurationInfoComponent
  },
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full'
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
