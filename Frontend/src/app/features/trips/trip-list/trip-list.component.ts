import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { debounceTime, distinctUntilChanged, startWith, switchMap } from 'rxjs';
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
    MatTableModule,
    DatePipe
  ],
  templateUrl: './trip-list.component.html',
  styleUrl: './trip-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TripListComponent {
  readonly api = inject(LogiFlowApiService);
  readonly searchControl = new FormControl('', { nonNullable: true });
  readonly displayedColumns: readonly string[] = ['route', 'cargo', 'vehicle', 'driver', 'window', 'status'];

  constructor() {
    this.searchControl.valueChanges
      .pipe(
        startWith(this.searchControl.value),
        debounceTime(300),
        distinctUntilChanged(),
        switchMap((value) => {
          this.api.setSearch(value);
          return this.api.loadActiveTrips(value);
        }),
        takeUntilDestroyed()
      )
      .subscribe();
  }

  refresh(): void {
    this.api.loadActiveTrips().subscribe();
  }
}
