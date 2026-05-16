export interface CreateTripRequest {
  cargoId: string;
  vehicleId: string;
  driverId: string;
  origin: string;
  destination: string;
  scheduledStart: string;
  scheduledEnd: string;
}

export interface TripResponse {
  id: string;
  cargoId: string;
  cargoDescription: string;
  cargoWeightKg: number;
  vehicleId: string;
  vehiclePlateNumber: string;
  driverId: string;
  driverFullName: string;
  origin: string;
  destination: string;
  scheduledStart: string;
  scheduledEnd: string;
  status: 'Planned' | 'Active' | 'Completed' | 'Cancelled' | string;
}

export interface VehicleAvailabilityResponse {
  vehicleId: string;
  available: boolean;
  status: 'Idle' | 'Busy' | 'Maintenance' | string;
  reason?: string | null;
}

export interface ApiProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  email: string;
  fullName: string;
  role: string;
  expiresAt: string;
}

export interface CurrentUserResponse {
  email: string;
  fullName: string;
  role: string;
}
