import { HttpClient } from '@angular/common/http';
import { Component, ElementRef, ViewChild } from '@angular/core';
import { Card } from '../../models/card';
import Swal from 'sweetalert2';
import { LoaderService } from '../services/loader.service';

@Component({
  selector: 'app-card-list',
  templateUrl: './card-list.component.html',
  styleUrl: './card-list.component.css'
})
export class CardListComponent {
  public cards: Card[] = [];
  selectedFile: any;
  @ViewChild('fileInput') fileInput!: ElementRef;

  constructor(private http: HttpClient, private loaderService: LoaderService) { }

  ngOnInit() {
    this.getCards();
  }

  getCards() {
    this.loaderService.setLoading(true);
    this.http.get<Card[]>('https://localhost:7119/api/BusinessCard/GetBusinessCards').subscribe({
      next: (response) => {
        this.cards = response;
      },
      error: () => {
        this.loaderService.setLoading(false);
      },
      complete: () => {
        this.loaderService.setLoading(false);
      }
    });
  }

  onDelete(id: number) {
    Swal.fire({
      title: 'Are you sure?',
      text: 'You will not be able to recover this imaginary card!',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#3085d6',
      cancelButtonColor: '#d33',
      confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
      if (result.isConfirmed) {
        this.loaderService.setLoading(true);
        this.http.delete(`https://localhost:7119/api/BusinessCard/DeleteBusinessCard/${id}`, { observe: 'response' })
          .subscribe({
            next: (response) => {
              // Check for successful status codes
              if (response.status === 200 || response.status === 204) {
                Swal.fire('Deleted!', 'Your card has been deleted.', 'success')
                  .then(() => {
                    this.getCards(); // Refresh the list after deletion
                  });
              } else {
                Swal.fire('Error!', 'Failed to delete the card. Please try again.', 'error');
              }
            },
            error: () => {
              this.loaderService.setLoading(false);
              Swal.fire('Error!', 'There was an error deleting the card.', 'error');
            },
            complete: () => {
              this.loaderService.setLoading(false);
            }
          });
      }
    });
  }

  onFileSelected(event: Event) {
    let file = (event.target as HTMLInputElement).files?.[0];
    if (file) {
      const fileType = file.type;
      // Determine file type and call appropriate function
      if (fileType.match(/csv/i)) {
        this.importFile(file);
      } else if (fileType.match(/xml/i)) {
        this.importFile(file);
      } else if (fileType.match(/image\/(png|jpeg|jpg)/)) {
        this.importFile(file);
      } else {
        // Handle unsupported file types
        console.error('Unsupported file type:', fileType);
      }
    }
  }

  triggerFileInput(extension: string) {
    this.fileInput.nativeElement.accept = extension;
    this.fileInput.nativeElement.click();
  }

  importFile(selectedFile: any) {
    this.loaderService.setLoading(true);

    const formData = new FormData();
    formData.append('file', selectedFile);
    this.http.post('https://localhost:7119/api/BusinessCard/CreateBusinessCardFromFile', formData)
      .subscribe({
        next: (response) => {
          Swal.fire({
            title: 'Success!',
            text: 'Card successfully imported!',
            icon: 'success',
            confirmButtonText: 'OK'
          }).then((result) => {
            this.getCards();
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

  exportToXml() {
    this.loaderService.setLoading(true);

    this.http.get(`https://localhost:7119/api/BusinessCard/ExportToXml`, { responseType: 'blob' })
      .subscribe((response: Blob) => {
        this.loaderService.setLoading(false);

        const url = window.URL.createObjectURL(response);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'exported_data.xml';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
      }, error => {
        this.loaderService.setLoading(false);
      });
  }

  exportToCsv() {
    this.loaderService.setLoading(true);

    this.http.get(`https://localhost:7119/api/BusinessCard/ExportToCsv`, { responseType: 'blob' })
      .subscribe((response: Blob) => {
        this.loaderService.setLoading(false);
        const url = window.URL.createObjectURL(response);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'exported_data.csv';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
      }, error => {
        this.loaderService.setLoading(false);
      });
  }
}
