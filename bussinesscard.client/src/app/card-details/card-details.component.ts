import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Card } from '../../models/card';
import { HttpClient } from '@angular/common/http';
import Swal from 'sweetalert2';
import { LoaderService } from '../services/loader.service';

@Component({
  selector: 'app-card-details',
  templateUrl: './card-details.component.html',
  styleUrl: './card-details.component.css'
})
export class CardDetailsComponent implements OnInit {
  id: number = 0;
  card: Card = new Card();
  isLoading: boolean = true;

  constructor(private route: ActivatedRoute, private http: HttpClient, private router: Router, private loaderService: LoaderService) { }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.id = Number(params.get('id') ?? 0);
      this.loadData();
    });
  }

  loadData() {
    if (this.id > 0)//details
    {
      this.isLoading = true;
      this.loaderService.setLoading(true);
      this.http.get<Card>(`https://localhost:7119/api/BusinessCard/${this.id}`).subscribe({
        next: (response) => {
          this.card = response;
        },
        error: () => {
          this.loaderService.setLoading(false);
          this.isLoading = false;
        },
        complete: () => {
          this.loaderService.setLoading(false);
          this.isLoading = false;
        }
      });
    }
  }

  onSubmit() {
    this.loaderService.setLoading(true);

    if (this.card.cardId === 0) {

      this.card.gender = parseInt(this.card.gender as any);
      this.http.post('https://localhost:7119/api/BusinessCard/CreateBusinessCard', this.card)
        .subscribe({
          next: (response) => {
            Swal.fire({
              title: 'Success!',
              text: 'Card successfully created!',
              icon: 'success',
              confirmButtonText: 'OK'
            }).then(() => {
              this.router.navigate(['/cards']); // Adjust the route as needed
            });
          },
          complete: () => {
            this.loaderService.setLoading(false);
          },
          error: (error) => {
            this.loaderService.setLoading(false);
          }
        });
    } else {
      // Update existing card (you would need to implement this)
      this.http.put(`https://localhost:7119/api/BusinessCard/UpdateBusinessCard/${this.id}`, this.card)
        .subscribe({
          next: (response) => {
            Swal.fire({
              title: 'Success!',
              text: 'Card successfully updated!',
              icon: 'success',
              confirmButtonText: 'OK'
            }).then(() => {
              // Navigate to the list route
              this.router.navigate(['/cards']); // Adjust the route as needed
            });
          },
          complete: () => {
            this.loaderService.setLoading(false);
          },
          error: (error) => {
            this.loaderService.setLoading(false);
          }
        });
    }
  }

}
