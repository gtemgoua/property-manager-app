import type { PaymentMethod, RentPaymentStatus, RentalUnitStatus } from './shared';

export interface Tenant {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  emergencyContactName?: string | null;
  emergencyContactPhone?: string | null;
  notes?: string | null;
  createdAt: string;
}

export interface RentalUnit {
  id: string;
  name: string;
  addressLine1: string;
  addressLine2?: string | null;
  city: string;
  state: string;
  postalCode: string;
  monthlyRent: number;
  bedrooms: number;
  bathrooms: number;
  squareFeet: number;
  status: RentalUnitStatus;
  notes?: string | null;
  createdAt: string;
}

export interface RentalContract {
  id: string;
  tenantId: string;
  tenantName: string;
  rentalUnitId: string;
  rentalUnitName: string;
  startDate: string;
  endDate?: string | null;
  monthlyRent: number;
  depositAmount: number;
  paymentDueDay: number;
  paymentSchedule: string;
  status: string;
  notes?: string | null;
  currency: string;
}

export interface RentPayment {
  id: string;
  rentalContractId: string;
  tenantName: string;
  rentalUnitName: string;
  dueDate: string;
  paidDate?: string | null;
  amountDue: number;
  amountPaid: number;
  lateFee?: number | null;
  status: RentPaymentStatus;
  paymentMethod: PaymentMethod;
  receiptNumber: string;
  receiptSent: boolean;
  receiptSentAt?: string | null;
  currency: string;
}

export interface PaymentAlert {
  id: string;
  rentPaymentId: string;
  message: string;
  isAcknowledged: boolean;
  alertDate: string;
}

export interface DashboardMetricsResponse {
  totalTenants: number;
  totalUnits: number;
  occupiedUnits: number;
  vacantUnits: number;
  monthlyRecurringRevenue: number;
  monthlyCollected: number;
  monthlyOutstanding: number;
  rentCollection: RentCollectionPoint[];
  occupancy: OccupancyPoint[];
}

export interface RentCollectionPoint {
  year: number;
  month: number;
  amountDue: number;
  amountPaid: number;
}

export interface OccupancyPoint {
  year: number;
  month: number;
  occupiedUnits: number;
  vacantUnits: number;
}
