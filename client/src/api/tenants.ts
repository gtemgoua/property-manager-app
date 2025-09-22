import api from './client';
import type { Tenant } from '../types';

export const getTenants = async () => {
  const response = await api.get<Tenant[]>('/tenants');
  return response.data;
};

export interface TenantPayload {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  emergencyContactName?: string;
  emergencyContactPhone?: string;
  notes?: string;
}

export const createTenant = async (payload: TenantPayload) => {
  const response = await api.post<Tenant>('/tenants', payload);
  return response.data;
};

export const updateTenant = async (id: string, payload: TenantPayload) => {
  const response = await api.put<Tenant>(`/tenants/${id}`, payload);
  return response.data;
};

export const deleteTenant = async (id: string) => {
  await api.delete(`/tenants/${id}`);
};
