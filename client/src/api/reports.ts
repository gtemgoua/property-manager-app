import api from './client';
import type { DashboardMetricsResponse } from '../types';

export const getDashboardMetrics = async (from?: string, to?: string) => {
  const response = await api.get<DashboardMetricsResponse>('/reports/dashboard', { params: { from, to } });
  return response.data;
};

export const downloadPaymentsExcel = async (from?: string, to?: string, currency?: string) => {
  const response = await api.get<ArrayBuffer>('/reports/payments/excel', {
    params: { from, to, currency },
    responseType: 'arraybuffer'
  });

  const headers = response.headers as Record<string, string | undefined>;
  const contentType = headers['content-type'] ?? 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet';
  const contentDisposition = headers['content-disposition'] ?? '';

  // Try to parse filename from Content-Disposition if present
  let filename: string | undefined;
  const match = /filename\*=UTF-8''(?<fn>[^;\n]+)/i.exec(contentDisposition) ?? /filename="?(?<fn>[^\";\n]+)"?/i.exec(contentDisposition);
  if (match && match.groups && match.groups.fn) {
    filename = decodeURIComponent(match.groups.fn);
  }

  return { blob: new Blob([response.data], { type: contentType }), filename };
};

export const downloadPaymentsPdf = async (from?: string, to?: string, currency?: string) => {
  const response = await api.get<ArrayBuffer>('/reports/payments/pdf', {
    params: { from, to, currency },
    responseType: 'arraybuffer'
  });

  const headers = response.headers as Record<string, string | undefined>;
  const contentType = headers['content-type'] ?? 'application/pdf';
  const contentDisposition = headers['content-disposition'] ?? '';

  let filename: string | undefined;
  const match = /filename\*=UTF-8''(?<fn>[^;\n]+)/i.exec(contentDisposition) ?? /filename="?(?<fn>[^\";\n]+)"?/i.exec(contentDisposition);
  if (match && match.groups && match.groups.fn) {
    filename = decodeURIComponent(match.groups.fn);
  }

  return { blob: new Blob([response.data], { type: contentType }), filename };
};
