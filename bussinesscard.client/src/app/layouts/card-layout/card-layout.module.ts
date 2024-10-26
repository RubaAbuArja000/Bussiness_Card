import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule, Routes } from '@angular/router';
import { CommonModule } from '@angular/common';
import { CardListComponent } from '../../card-list/card-list.component';
import { CardDetailsComponent } from '../../card-details/card-details.component';

export const CardRoutes: Routes = [
  { path: 'list', component: CardListComponent },
  { path: 'details', component: CardDetailsComponent }
];

@NgModule({
  imports: [
    CommonModule,
    RouterModule.forChild(CardRoutes),
    FormsModule,
    ReactiveFormsModule,
  ],
  declarations: [
    CardDetailsComponent,
    CardListComponent
  ]
})
export class CardLayoutModule { }
