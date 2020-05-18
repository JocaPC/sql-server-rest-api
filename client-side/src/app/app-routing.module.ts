import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { PeopleComponent } from './people/people.component';


const routes: Routes = [
  { path: 'load', component: PeopleComponent },
  { path: '**', redirectTo: '/' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
