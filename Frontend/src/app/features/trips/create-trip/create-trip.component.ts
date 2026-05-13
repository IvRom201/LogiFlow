import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { AbstractControl, AsyncValidatorFn, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Observable, catchError, map, of, switchMap, timer } from 'rxjs';
import { CreateTripRequest } from '../../../core/models/logiflow.models';
import { LogiFlowApiService } from '../../../core/services/logiflow-api.service';

@Component({
  selector: 'app-create-trip',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './create-trip.component.html',
  styleUrl: './create-trip.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CreateTripComponent {
  private readonly fb = inject(FormBuilder);
  private readonly snackBar = inject(MatSnackBar);
  readonly api = inject(LogiFlowApiService);

  readonly form = this.fb.nonNullable.group({
    cargoId: ['', [Validators.required]],
    vehicleId: ['', [Validators.required], [this.vehicleAvailableValidator()]],
    driverId: ['', [Validators.required]],
    origin: ['', [Validators.required, Validators.maxLength(200)]],
    destination: ['', [Validators.required, Validators.maxLength(200)]],
    scheduledStart: ['', [Validators.required]],
    scheduledEnd: ['', [Validators.required]]
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const request: CreateTripRequest = {
      cargoId: value.cargoId,
      vehicleId: value.vehicleId,
      driverId: value.driverId,
      origin: value.origin,
      destination: value.destination,
      scheduledStart: new Date(value.scheduledStart).toISOString(),
      scheduledEnd: new Date(value.scheduledEnd).toISOString()
    };

    this.api.createTrip(request).subscribe({
      next: () => {
        this.form.reset();
        this.snackBar.open('Trip created successfully.', 'Close', { duration: 3000 });
      },
      error: (error: Error) => {
        this.snackBar.open(error.message, 'Close', { duration: 5000 });
      }
    });
  }

  private vehicleAvailableValidator(): AsyncValidatorFn {
    return (control: AbstractControl<string>): Observable<{ vehicleUnavailable: string } | null> => {
      const vehicleId = control.value?.trim();

      if (!vehicleId) {
        return of(null);
      }

      return timer(350).pipe(
        switchMap(() => this.api.checkVehicleAvailability(vehicleId)),
        map((response) => (response.available ? null : { vehicleUnavailable: response.reason ?? 'Vehicle is unavailable.' })),
        catchError(() => of({ vehicleUnavailable: 'Vehicle availability check failed.' }))
      );
    };
  }
}
