import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class LoaderService {

  loadingSubject = new BehaviorSubject(false);
  constructor() {
  }

  setLoading(state: boolean) {
    this.loadingSubject.next(state); // Emit new loading state
  }

  getLoadingState() {
    return this.loadingSubject.asObservable();
  }
}
