import api from './client';
import type { PaymentAlert } from '../types';

export const getAlerts = async () => {
  const response = await api.get<PaymentAlert[]>('/alerts');
  return response.data;
};

export const acknowledgeAlert = async (id: string) => {
  await api.post(`/alerts/${id}/acknowledge`);
};
