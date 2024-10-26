import { NgModule } from '@angular/core';
import { CommonModule, } from '@angular/common';
import { BrowserModule } from '@angular/platform-browser';
import { Routes, RouterModule } from '@angular/router';
import { CardListComponent } from './card-list/card-list.component';
import { CardDetailsComponent } from './card-details/card-details.component';

const routes: Routes = [
  { path: '', redirectTo: 'card', pathMatch: 'full' }, //default route

  {
    path: 'card',
    children: [
      { path: '', redirectTo: 'list', pathMatch: 'full' },
      { path: 'list', component: CardListComponent },
      { path: 'details', component: CardDetailsComponent },
      { path: 'details/:id', component: CardDetailsComponent }
    ]
  },
  { path: '**', redirectTo: 'card' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
