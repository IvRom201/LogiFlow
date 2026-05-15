import { HttpClient, HttpParams } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { Observable, catchError, finalize, tap, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ApiProblemDetails,
  CreateTripRequest,
  TripResponse,
  VehicleAvailabilityResponse
} from '../models/logiflow.models';

@Injectable({
  providedIn: 'root'
})
export class LogiFlowApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;

  private readonly activeTripsState = signal<TripResponse[]>([]);
  private readonly loadingState = signal(false);
  private readonly errorState = signal<string | null>(null);
  private readonly searchState = signal('');
  private readonly actionLoadingIdState = signal<string | null>(null);

  readonly activeTrips = this.activeTripsState.asReadonly();
  readonly loading = this.loadingState.asReadonly();
  readonly error = this.errorState.asReadonly();
  readonly search = this.searchState.asReadonly();
  readonly actionLoadingId = this.actionLoadingIdState.asReadonly();

  readonly activeTripCount = computed(() => this.activeTripsState().length);

  setSearch(value: string): void {
    this.searchState.set(value.trim());
  }

  loadActiveTrips(search = this.searchState()): Observable<TripResponse[]> {
    this.loadingState.set(true);
    this.errorState.set(null);

    let params = new HttpParams();

    if (search.trim().length > 0) {
      params = params.set('search', search.trim());
    }

    return this.http
      .get<TripResponse[]>(`${this.baseUrl}/trips/active`, { params })
      .pipe(
        tap((trips) => this.activeTripsState.set(trips)),
        catchError((error: unknown) => this.handleError(error)),
        finalize(() => this.loadingState.set(false))
      );
  }

  createTrip(request: CreateTripRequest): Observable<TripResponse> {
    this.loadingState.set(true);
    this.errorState.set(null);

    return this.http
      .post<TripResponse>(`${this.baseUrl}/trips`, request)
      .pipe(
        tap((createdTrip) => {
          this.activeTripsState.update((current) => [createdTrip, ...current]);
        }),
        catchError((error: unknown) => this.handleError(error)),
        finalize(() => this.loadingState.set(false))
      );
  }

  completeTrip(tripId: string): Observable<TripResponse> {
    this.actionLoadingIdState.set(tripId);
    this.errorState.set(null);

    return this.http
      .put<TripResponse>(`${this.baseUrl}/trips/${encodeURIComponent(tripId)}/complete`, {})
      .pipe(
        tap((completedTrip) => {
          this.activeTripsState.update((current) =>
            current.filter((trip) => trip.id !== completedTrip.id)
          );
        }),
        catchError((error: unknown) => this.handleError(error)),
        finalize(() => this.actionLoadingIdState.set(null))
      );
  }

  cancelTrip(tripId: string): Observable<TripResponse> {
    this.actionLoadingIdState.set(tripId);
    this.errorState.set(null);

    return this.http
      .put<TripResponse>(`${this.baseUrl}/trips/${encodeURIComponent(tripId)}/cancel`, {})
      .pipe(
        tap((cancelledTrip) => {
          this.activeTripsState.update((current) =>
            current.filter((trip) => trip.id !== cancelledTrip.id)
          );
        }),
        catchError((error: unknown) => this.handleError(error)),
        finalize(() => this.actionLoadingIdState.set(null))
      );
  }

  checkVehicleAvailability(vehicleId: string): Observable<VehicleAvailabilityResponse> {
    return this.http
      .get<VehicleAvailabilityResponse>(
        `${this.baseUrl}/vehicles/${encodeURIComponent(vehicleId)}/availability`
      )
      .pipe(catchError((error: unknown) => this.handleError(error)));
  }

  clearError(): void {
    this.errorState.set(null);
  }

  private handleError(error: unknown): Observable<never> {
    const message = this.extractErrorMessage(error);
    this.errorState.set(message);

    return throwError(() => new Error(message));
  }

  private extractErrorMessage(error: unknown): string {
    if (typeof error !== 'object' || error === null) {
      return 'Unexpected client error.';
    }

    const candidate = error as {
      error?: ApiProblemDetails | string;
      message?: string;
      status?: number;
    };

    if (typeof candidate.error === 'string' && candidate.error.trim().length > 0) {
      return candidate.error;
    }

    const responseError = candidate.error;

    if (this.isApiProblemDetails(responseError)) {
      if (typeof responseError.detail === 'string' && responseError.detail.trim().length > 0) {
        return responseError.detail;
      }

      if (typeof responseError.title === 'string' && responseError.title.trim().length > 0) {
        return responseError.title;
      }
    }

    if (candidate.message && candidate.message.trim().length > 0) {
      return candidate.message;
    }

    return candidate.status
      ? `Request failed with status ${candidate.status}.`
      : 'Request failed.';
  }

  private isApiProblemDetails(value: unknown): value is ApiProblemDetails {
    return typeof value === 'object' && value !== null;
  }
}
