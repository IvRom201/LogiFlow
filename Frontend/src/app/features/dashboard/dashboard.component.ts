import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { TripListComponent } from '../trips/trip-list/trip-list.component';
import { CreateTripComponent } from '../trips/create-trip/create-trip.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [MatCardModule, TripListComponent, CreateTripComponent],
  template: `
    <main class="page">
      <section class="grid">
        <mat-card>
          <mat-card-header>
            <mat-card-title>Active Trips</mat-card-title>
            <mat-card-subtitle>Search by route, plate, cargo, or driver</mat-card-subtitle>
          </mat-card-header>
          <mat-card-content>
            <app-trip-list />
          </mat-card-content>
        </mat-card>

        <mat-card>
          <mat-card-header>
            <mat-card-title>Create Trip</mat-card-title>
            <mat-card-subtitle>Assign available vehicle and driver</mat-card-subtitle>
          </mat-card-header>
          <mat-card-content>
            <app-create-trip />
          </mat-card-content>
        </mat-card>
      </section>
    </main>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DashboardComponent {}
