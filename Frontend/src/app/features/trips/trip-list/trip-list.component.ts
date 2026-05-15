import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import {
  EMPTY,
  catchError,
  debounceTime,
  distinctUntilChanged,
  startWith,
  switchMap
} from 'rxjs';
import { TripResponse } from '../../../core/models/logiflow.models';
import { LogiFlowApiService } from '../../../core/services/logiflow-api.service';

@Component({
  selector: 'app-trip-list',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressBarModule,
    MatSnackBarModule,
    MatTableModule,
    DatePipe
  ],
  templateUrl: './trip-list.component.html',
  styleUrl: './trip-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TripListComponent {
  readonly api = inject(LogiFlowApiService);

  private readonly snackBar = inject(MatSnackBar);

  readonly searchControl = new FormControl('', { nonNullable: true });

  readonly displayedColumns: readonly string[] = [
    'route',
    'cargo',
    'vehicle',
    'driver',
    'window',
    'status',
    'actions'
  ];

  constructor() {
    this.searchControl.valueChanges
      .pipe(
        startWith(this.searchControl.value),
        debounceTime(300),
        distinctUntilChanged(),
        switchMap((value) => {
          this.api.setSearch(value);

          return this.api.loadActiveTrips(value).pipe(
            catchError(() => EMPTY)
          );
        }),
        takeUntilDestroyed()
      )
      .subscribe();
  }

  refresh(): void {
    this.api.loadActiveTrips().subscribe({
      error: (error: Error) => this.showError(error.message)
    });
  }

  completeTrip(trip: TripResponse): void {
    this.api.completeTrip(trip.id).subscribe({
      next: () => {
        this.snackBar.open('Trip completed successfully.', 'Close', {
          duration: 3000
        });
      },
      error: (error: Error) => this.showError(error.message)
    });
  }

  cancelTrip(trip: TripResponse): void {
    const confirmed = window.confirm(
      `Cancel trip from ${trip.origin} to ${trip.destination}?`
    );

    if (!confirmed) {
      return;
    }

    this.api.cancelTrip(trip.id).subscribe({
      next: () => {
        this.snackBar.open('Trip cancelled successfully.', 'Close', {
          duration: 3000
        });
      },
      error: (error: Error) => this.showError(error.message)
    });
  }

  isActionLoading(trip: TripResponse): boolean {
    return this.api.actionLoadingId() === trip.id;
  }

  private showError(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 5000
    });
  }
}
