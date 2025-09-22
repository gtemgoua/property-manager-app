import api from './client';
import type { RentPayment } from '../types';
import type { PaymentMethod } from '../types/shared';

export const getUpcomingPayments = async (from?: string, to?: string) => {
  const response = await api.get<RentPayment[]>('/rentpayments', {
    params: { from, to }
  });
  return response.data;
};

export const getPaymentsByContract = async (contractId: string) => {
  const response = await api.get<RentPayment[]>(`/rentpayments/contract/${contractId}`);
  return response.data;
};

export interface CreatePaymentPayload {
  rentalContractId: string;
  dueDate: string;
  amountDue: number;
  notes?: string;
  currency?: number | string;
}

export interface RecordPaymentPayload {
  amountPaid: number;
  paidDate: string;
  paymentMethod: PaymentMethod;
  referenceNumber?: string;
  lateFee?: number;
  notes?: string;
}

export const createPayment = async (payload: CreatePaymentPayload) => {
  const response = await api.post<RentPayment>('/rentpayments', payload);
  return response.data;
};

export const recordPayment = async (id: string, payload: RecordPaymentPayload) => {
  const response = await api.post<RentPayment>(`/rentpayments/${id}/record`, payload);
  return response.data;
};

export const downloadReceipt = async (id: string) => {
  const response = await api.get<ArrayBuffer>(`/rentpayments/${id}/receipt`, { responseType: 'arraybuffer' });
  const contentType = (response.headers as Record<string, string | undefined>)['content-type'] ?? 'application/pdf';
  return new Blob([response.data], { type: contentType });
};

export const sendReceipt = async (id: string, recipientEmail: string, recipientName?: string, attachPdf = true) => {
  await api.post(`/rentpayments/${id}/send-receipt`, { recipientEmail, recipientName, attachPdf });
};
