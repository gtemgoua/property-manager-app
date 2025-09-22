export const toCurrencyCode = (raw: any): string => {
  const s = String(raw ?? '').toUpperCase();
  if (s === '2' || s === 'XAF') return 'XAF';
  if (s === '1' || s === 'EUR') return 'EUR';
  return 'USD';
};

export const toCurrencySymbol = (raw: any): string => {
  const code = toCurrencyCode(raw);
  if (code === 'XAF') return 'FCFA';
  if (code === 'EUR') return '€';
  return '$';
};

export const formatCurrency = (amount: number, raw: any, locale?: string, options?: Intl.NumberFormatOptions) => {
  const code = toCurrencyCode(raw);
  const maximumFractionDigits = code === 'XAF' ? 0 : 2;
  const opts: Intl.NumberFormatOptions = {
    style: 'currency',
    currency: code,
    maximumFractionDigits,
    ...options,
  };
  return new Intl.NumberFormat(locale ?? undefined, opts).format(amount);
};

export default {
  toCurrencyCode,
  toCurrencySymbol,
  formatCurrency,
};
