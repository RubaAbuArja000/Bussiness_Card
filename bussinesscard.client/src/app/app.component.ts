import { Component } from '@angular/core';
import { LoaderService } from './services/loader.service';


@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {

  isLoading: boolean = true;
  constructor(loaderService: LoaderService) {
    loaderService.getLoadingState().subscribe({
      next: (response) => {
        this.isLoading = response;
      }
    })
  }
}
