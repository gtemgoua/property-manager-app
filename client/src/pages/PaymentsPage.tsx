import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { format } from 'date-fns';
import Modal from '../components/Modal';
import FormField from '../components/FormField';
import StatusBadge from '../components/StatusBadge';
import {
  createPayment,
  downloadReceipt,
  getUpcomingPayments,
  recordPayment,
  sendReceipt
} from '../api/payments';
import { getContracts } from '../api/contracts';
import type { CreatePaymentPayload, RecordPaymentPayload } from '../api/payments';
import type { PaymentMethod, Currency } from '../types/shared';
import { toCurrencyCode, toCurrencySymbol, formatCurrency } from '../utils/currency';
import { downloadBlob } from '../utils/download';
import { useNotification } from '../context/NotificationContext';

const paymentMethods: PaymentMethod[] = ['Cash', 'BankTransfer', 'MobileMoney', 'Cheque', 'Card'];

const initialRecordForm: RecordPaymentPayload = {
  amountPaid: 0,
  paidDate: new Date().toISOString().substring(0, 10),
  paymentMethod: 'Cash' as PaymentMethod,
  referenceNumber: '',
  lateFee: 0,
  notes: ''
};

const PaymentsPage = () => {
  const queryClient = useQueryClient();
  const { notify } = useNotification();
  const { data: payments } = useQuery({ queryKey: ['payments'], queryFn: () => getUpcomingPayments() });
  const { data: contracts } = useQuery({ queryKey: ['contracts'], queryFn: getContracts });
  const [recordModalOpen, setRecordModalOpen] = useState(false);
  const [sendModalOpen, setSendModalOpen] = useState(false);
  const [createModalOpen, setCreateModalOpen] = useState(false);
  const [activePaymentId, setActivePaymentId] = useState<string | null>(null);
  const [recordForm, setRecordForm] = useState<RecordPaymentPayload>(initialRecordForm);
  const [receiptEmail, setReceiptEmail] = useState('');
  const [selectedContractId, setSelectedContractId] = useState('');
  const [dueDate, setDueDate] = useState(new Date().toISOString().substring(0, 10));
  const [amountDue, setAmountDue] = useState(0);
  const [currency, setCurrency] = useState<Currency>('XAF');
  const [recordCurrency, setRecordCurrency] = useState<Currency>('XAF');
  const [sendPaymentCurrency, setSendPaymentCurrency] = useState<Currency>('XAF');

  const recordMutation = useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: RecordPaymentPayload }) => recordPayment(id, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['payments'] });
      setRecordModalOpen(false);
      notify('Payment recorded', 'success');
    },
    onError: () => notify('Unable to record payment', 'error')
  });

  const sendMutation = useMutation({
    mutationFn: ({ id, email }: { id: string; email: string }) => sendReceipt(id, email, undefined, true),
    onSuccess: () => {
      setSendModalOpen(false);
      notify('Receipt sent', 'success');
    },
    onError: () => notify('Unable to send receipt', 'error')
  });

  const createMutation = useMutation({
    mutationFn: (payload: CreatePaymentPayload) => createPayment(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['payments'] });
      setCreateModalOpen(false);
      notify('Payment scheduled', 'success');
    },
    onError: () => notify('Unable to schedule payment', 'error')
  });

  const activePayment = useMemo(() => payments?.find(payment => payment.id === activePaymentId), [payments, activePaymentId]);

  const handleRecordSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!activePaymentId) return;
    recordMutation.mutate({ id: activePaymentId, payload: recordForm });
  };

  const handleReceiptSend = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!activePaymentId) return;
    sendMutation.mutate({ id: activePaymentId, email: receiptEmail });
  };

  const handleScheduleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!selectedContractId) return;
    createMutation.mutate({
      rentalContractId: selectedContractId,
      dueDate,
      amountDue,
      currency,
      notes: ''
    });
  };

  const handleDownloadReceipt = async (payment: any) => {
    try {
          const currencyCode = toCurrencyCode((payment as any).currency);

      const blob = await downloadReceipt(payment.id);
      downloadBlob(blob, `receipt-${payment.id}-${currencyCode}.pdf`);
      notify('Receipt downloaded', 'success');
    } catch (error) {
      notify('Unable to download receipt', 'error');
    }
  };

  const openRecordModal = (id: string, amountDueValue: number, currencyValue?: any) => {
    setActivePaymentId(id);
    setRecordForm({ ...initialRecordForm, amountPaid: amountDueValue });
    setRecordCurrency((currencyValue as Currency) ?? 'XAF');
    setRecordModalOpen(true);
  };

  const openSendModal = (id: string, defaultEmail?: string, currencyValue?: any) => {
    setActivePaymentId(id);
    setReceiptEmail(defaultEmail ?? '');
    setSendPaymentCurrency((currencyValue as Currency) ?? 'XAF');
    setSendModalOpen(true);
  };

  const openCreateModal = () => {
    setCreateModalOpen(true);
    setSelectedContractId('');
    setDueDate(new Date().toISOString().substring(0, 10));
    setAmountDue(0);
    setCurrency('XAF');
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
      <div className="section-title">
        <h3>Rent Payments</h3>
        <button type="button" className="primary-button" onClick={openCreateModal}>
          Schedule Payment
        </button>
      </div>

      <div className="table-wrapper">
        <table className="table">
          <thead>
            <tr>
              <th>Tenant</th>
              <th>Unit</th>
              <th>Due</th>
              <th>Amount</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {payments?.map(payment => {
              // map backend currency (numeric or string) to ISO currency code for formatting
              const currencyCode = toCurrencyCode((payment as any).currency);

              return (
                <tr key={payment.id}>
                  <td>{payment.tenantName}</td>
                  <td>{payment.rentalUnitName}</td>
                  <td>{format(new Date(payment.dueDate), 'MMM dd, yyyy')}</td>
                  <td>{formatCurrency(payment.amountDue, (payment as any).currency)}</td>
                  <td><StatusBadge status={payment.status} /></td>
                  <td style={{ display: 'flex', gap: '0.5rem' }}>
                    <button type="button" className="secondary-button" onClick={() => openRecordModal(payment.id, payment.amountDue, payment.currency)}>
                      Record
                    </button>
                    <button type="button" className="secondary-button" onClick={() => handleDownloadReceipt(payment)}>
                      Receipt
                    </button>
                    <button type="button" className="secondary-button" onClick={() => openSendModal(payment.id, undefined, payment.currency)}>
                      Email
                    </button>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>

      <Modal title="Record Payment" open={recordModalOpen} onClose={() => setRecordModalOpen(false)}>
        <form onSubmit={handleRecordSubmit} className="form-grid">
          <FormField label="Amount paid" name="amountPaid">
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
              {/* currency prefix */}
              <span style={{ display: 'inline-flex', alignItems: 'center', justifyContent: 'center', padding: '0.25rem 0.5rem', background: '#f3f3f3', border: '1px solid #e1e1e1', borderRadius: '4px', color: '#333', fontSize: '0.95rem' }}>
                {toCurrencySymbol((activePayment as any)?.currency ?? recordCurrency)}
              </span>
              <input
                id="amountPaid"
                type="number"
                min={0}
                step="0.01"
                value={recordForm.amountPaid}
                onChange={event => setRecordForm({ ...recordForm, amountPaid: Number(event.target.value) })}
                required
                style={{ flex: '1 1 auto' }}
              />
            </div>
          </FormField>
          <div style={{ gridColumn: '1 / -1', color: '#666', fontSize: '0.9rem' }}>
            Outstanding: {
              (() => {
                const payment = activePayment as any | undefined;
                const due = payment ? Number(payment.amountDue ?? 0) : Number(recordForm.amountPaid ?? 0);
                const paid = payment ? Number(payment.amountPaid ?? 0) : 0;
                const outstanding = Math.max(0, due - paid);
                const currencyCode = toCurrencyCode(payment?.currency ?? recordCurrency);
                return formatCurrency(outstanding, currencyCode);
              })()
            }
          </div>
          <FormField label="Paid date" name="paidDate">
            <input
              id="paidDate"
              type="date"
              value={recordForm.paidDate}
              onChange={event => setRecordForm({ ...recordForm, paidDate: event.target.value })}
              required
            />
          </FormField>
          <FormField label="Method" name="paymentMethod">
            <select
              id="paymentMethod"
              value={recordForm.paymentMethod}
              onChange={event => setRecordForm({ ...recordForm, paymentMethod: event.target.value as PaymentMethod })}
            >
              {paymentMethods.map(method => (
                <option key={method} value={method}>{method}</option>
              ))}
            </select>
          </FormField>
          <FormField label="Reference #" name="referenceNumber">
            <input
              id="referenceNumber"
              value={recordForm.referenceNumber}
              onChange={event => setRecordForm({ ...recordForm, referenceNumber: event.target.value })}
            />
          </FormField>
          <FormField label="Late fee" name="lateFee">
            <input
              id="lateFee"
              type="number"
              min={0}
              step="0.01"
              value={recordForm.lateFee}
              onChange={event => setRecordForm({ ...recordForm, lateFee: Number(event.target.value) })}
            />
          </FormField>
          <label className="form-control" htmlFor="payment-notes">
            <span>Notes</span>
            <textarea
              id="payment-notes"
              rows={3}
              value={recordForm.notes}
              onChange={event => setRecordForm({ ...recordForm, notes: event.target.value })}
            />
          </label>
          <div className="button-row" style={{ gridColumn: '1 / -1' }}>
            <button type="button" className="secondary-button" onClick={() => setRecordModalOpen(false)}>
              Cancel
            </button>
            <button type="submit" className="primary-button">
              Save Payment
            </button>
          </div>
        </form>
      </Modal>

      <Modal title="Send Receipt" open={sendModalOpen} onClose={() => setSendModalOpen(false)}>
        <form onSubmit={handleReceiptSend} className="form-grid">
          <FormField label="Recipient email" name="receiptEmail">
            <input
              id="receiptEmail"
              type="email"
              value={receiptEmail}
              onChange={event => setReceiptEmail(event.target.value)}
              required
            />
          </FormField>
          <div className="button-row" style={{ gridColumn: '1 / -1' }}>
            <button type="button" className="secondary-button" onClick={() => setSendModalOpen(false)}>
              Cancel
            </button>
            <button type="submit" className="primary-button">
              Send Receipt
            </button>
          </div>
        </form>
      </Modal>

      <Modal title="Schedule Payment" open={createModalOpen} onClose={() => setCreateModalOpen(false)}>
        <form onSubmit={handleScheduleSubmit} className="form-grid">
          <FormField label="Contract" name="contract">
            <select
              id="contract"
              value={selectedContractId}
              onChange={event => {
                const contractId = event.target.value;
                setSelectedContractId(contractId);
                const selected = contracts?.find(contract => contract.id === contractId);
                setAmountDue(selected?.monthlyRent ?? 0);
              }}
              required
            >
              <option value="">Select contract</option>
              {contracts?.map(contract => (
                <option key={contract.id} value={contract.id}>
                  {contract.tenantName} – {contract.rentalUnitName}
                </option>
              ))}
            </select>
          </FormField>
          <FormField label="Due date" name="dueDate">
            <input id="dueDate" type="date" value={dueDate} onChange={event => setDueDate(event.target.value)} required />
          </FormField>
          <FormField label="Amount" name="amountDue">
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
              <span style={{ display: 'inline-flex', alignItems: 'center', justifyContent: 'center', padding: '0.25rem 0.5rem', background: '#f3f3f3', border: '1px solid #e1e1e1', borderRadius: '4px', color: '#333', fontSize: '0.95rem' }}>
                {(() => {
                  const selected = contracts?.find(c => c.id === selectedContractId);
                  const raw = selected?.currency ?? currency;
                  const c = String(raw);
                  if (c === '2' || c === 'XAF') return 'FCFA';
                  if (c === '1' || c === 'EUR') return '€';
                  return '$';
                })()}
              </span>
              <input
                id="amountDue"
                type="number"
                min={0}
                step="0.01"
                value={amountDue}
                onChange={event => setAmountDue(Number(event.target.value))}
                required
                style={{ flex: '1 1 auto' }}
              />
            </div>
          </FormField>
          <FormField label="Currency" name="currency">
            <select id="currency" value={currency} onChange={event => setCurrency(event.target.value as Currency)}>
              <option value="USD">USD</option>
              <option value="EUR">EUR</option>
              <option value="XAF">XAF</option>
            </select>
          </FormField>
          <div className="button-row" style={{ gridColumn: '1 / -1' }}>
            <button type="button" className="secondary-button" onClick={() => setCreateModalOpen(false)}>
              Cancel
            </button>
            <button type="submit" className="primary-button">
              Schedule
            </button>
          </div>
        </form>
      </Modal>
    </div>
  );
};

export default PaymentsPage;
