import api from './client';
import type { RentalContract } from '../types';
import type { ContractStatus, PaymentSchedule } from '../types/shared';

export const getContracts = async () => {
  const response = await api.get<RentalContract[]>('/rentalcontracts');
  return response.data;
};

export interface ContractPayload {
  tenantId: string;
  rentalUnitId: string;
  startDate: string;
  endDate?: string | null;
  monthlyRent: number;
  depositAmount: number;
  paymentDueDay: number;
  paymentSchedule: PaymentSchedule;
  notes?: string;
}

export const createContract = async (payload: ContractPayload) => {
  const response = await api.post<RentalContract>('/rentalcontracts', payload);
  return response.data;
};

export interface UpdateContractPayload {
  startDate: string;
  endDate?: string | null;
  monthlyRent: number;
  depositAmount: number;
  paymentDueDay: number;
  paymentSchedule: PaymentSchedule;
  status: ContractStatus;
  notes?: string;
}

export const updateContract = async (id: string, payload: UpdateContractPayload) => {
  const response = await api.put<RentalContract>(`/rentalcontracts/${id}`, payload);
  return response.data;
};
