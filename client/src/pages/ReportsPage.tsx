import { useState } from 'react';
import { downloadPaymentsExcel, downloadPaymentsPdf } from '../api/reports';
import { downloadBlob } from '../utils/download';
import { useNotification } from '../context/NotificationContext';
import { Currency } from '../types/shared';

const ReportsPage = () => {
  const [from, setFrom] = useState(new Date(new Date().setMonth(new Date().getMonth() - 3)).toISOString().substring(0, 10));
  const [to, setTo] = useState(new Date().toISOString().substring(0, 10));
  const { notify } = useNotification();
  const [currency, setCurrency] = useState<Currency | ''>('');

  const handleDownloadExcel = async () => {
    try {
      const result = await downloadPaymentsExcel(from, to, currency || undefined);
      const filename = result.filename ?? `payments-${from}-${to}.xlsx`;
      downloadBlob(result.blob, filename);
      notify('Excel report ready', 'success');
    } catch (error) {
      notify('Unable to generate Excel report', 'error');
    }
  };

  const handleDownloadPdf = async () => {
    try {
      const result = await downloadPaymentsPdf(from, to, currency || undefined);
      const filename = result.filename ?? `payments-${from}-${to}.pdf`;
      downloadBlob(result.blob, filename);
      notify('PDF report ready', 'success');
    } catch (error) {
      notify('Unable to generate PDF report', 'error');
    }
  };

  return (
    <section style={{ display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
      <h3>Export Payments</h3>
      <div className="form-grid">
        <label className="form-control" htmlFor="from">
          <span>From</span>
          <input id="from" type="date" value={from} onChange={event => setFrom(event.target.value)} />
        </label>
        <label className="form-control" htmlFor="to">
          <span>To</span>
          <input id="to" type="date" value={to} onChange={event => setTo(event.target.value)} />
        </label>
        <label className="form-control" htmlFor="currency">
          <span>Currency</span>
          <select id="currency" value={currency} onChange={e => setCurrency(e.target.value as Currency | '')}>
            <option value="">All</option>
            <option value="USD">USD</option>
            <option value="EUR">EUR</option>
            <option value="XAF">XAF</option>
          </select>
        </label>
      </div>
      <div className="button-row">
        <button type="button" className="secondary-button" onClick={handleDownloadPdf}>
          Download PDF
        </button>
        <button type="button" className="primary-button" onClick={handleDownloadExcel}>
          Download Excel
        </button>
      </div>
    </section>
  );
};

export default ReportsPage;
