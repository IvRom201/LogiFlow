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

@Injectable({ providedIn: 'root' })
export class LogiFlowApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;

  private readonly activeTripsState = signal<TripResponse[]>([]);
  private readonly loadingState = signal(false);
  private readonly errorState = signal<string | null>(null);
  private readonly searchState = signal('');

  readonly activeTrips = this.activeTripsState.asReadonly();
  readonly loading = this.loadingState.asReadonly();
  readonly error = this.errorState.asReadonly();
  readonly search = this.searchState.asReadonly();
  readonly activeTripCount = computed(() => this.activeTripsState().length);

  setSearch(value: string): void {
    this.searchState.set(value.trim());
  }

  loadActiveTrips(search = this.searchState()): Observable<TripResponse[]> {
    this.loadingState.set(true);
    this.errorState.set(null);

    const params = search.length > 0 ? new HttpParams().set('search', search) : undefined;

    return this.http.get<TripResponse[]>(`${this.baseUrl}/trips/active`, { params }).pipe(
      tap((trips) => this.activeTripsState.set(trips)),
      catchError((error: unknown) => this.handleError<TripResponse[]>(error)),
      finalize(() => this.loadingState.set(false))
    );
  }

  createTrip(request: CreateTripRequest): Observable<TripResponse> {
    this.loadingState.set(true);
    this.errorState.set(null);

    return this.http.post<TripResponse>(`${this.baseUrl}/trips`, request).pipe(
      tap((createdTrip) => {
        this.activeTripsState.update((current) => [createdTrip, ...current]);
      }),
      catchError((error: unknown) => this.handleError<TripResponse>(error)),
      finalize(() => this.loadingState.set(false))
    );
  }

  checkVehicleAvailability(vehicleId: string): Observable<VehicleAvailabilityResponse> {
    return this.http
      .get<VehicleAvailabilityResponse>(`${this.baseUrl}/vehicles/${encodeURIComponent(vehicleId)}/availability`)
      .pipe(catchError((error: unknown) => this.handleError<VehicleAvailabilityResponse>(error)));
  }

  clearError(): void {
    this.errorState.set(null);
  }

  private handleError<T>(error: unknown): Observable<T> {
    const message = this.extractErrorMessage(error);
    this.errorState.set(message);
    return throwError(() => new Error(message));
  }

  private extractErrorMessage(error: unknown): string {
    if (typeof error !== 'object' || error === null) {
      return 'Unexpected client error.';
    }

    const candidate = error as { error?: ApiProblemDetails | string; message?: string; status?: number };

    if (typeof candidate.error === 'string' && candidate.error.trim().length > 0) {
      return candidate.error;
    }

    const responseError = candidate.error as unknown;

    if (typeof responseError === 'string' && responseError.trim().length > 0) {
      return responseError;
    }

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

    return candidate.status ? `Request failed with status ${candidate.status}.` : 'Request failed.';
  }

  private isApiProblemDetails(value: unknown): value is ApiProblemDetails {
    return typeof value === 'object' && value !== null;
  }
}
