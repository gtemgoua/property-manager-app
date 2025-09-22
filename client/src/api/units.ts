import api from './client';
import type { RentalUnit } from '../types';
import type { RentalUnitStatus } from '../types/shared';

export const getUnits = async () => {
  const response = await api.get<RentalUnit[]>('/rentalunits');
  return response.data;
};

export interface UnitPayload {
  name: string;
  addressLine1: string;
  addressLine2?: string;
  city: string;
  state: string;
  postalCode: string;
  monthlyRent: number;
  bedrooms: number;
  bathrooms: number;
  squareFeet: number;
  status: RentalUnitStatus;
  notes?: string;
}

export const createUnit = async (payload: UnitPayload) => {
  const response = await api.post<RentalUnit>('/rentalunits', payload);
  return response.data;
};

export const updateUnit = async (id: string, payload: UnitPayload) => {
  const response = await api.put<RentalUnit>(`/rentalunits/${id}`, payload);
  return response.data;
};

export const deleteUnit = async (id: string) => {
  await api.delete(`/rentalunits/${id}`);
};
