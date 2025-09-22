import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { format } from 'date-fns';
import Modal from '../components/Modal';
import FormField from '../components/FormField';
import { createContract, getContracts } from '../api/contracts';
import { getTenants } from '../api/tenants';
import { getUnits } from '../api/units';
import type { PaymentSchedule } from '../types/shared';
import { useNotification } from '../context/NotificationContext';
import { formatCurrency } from '../utils/currency';

const schedules: PaymentSchedule[] = ['Monthly', 'Quarterly', 'SemiAnnual', 'Annual'];

const initialForm = {
  tenantId: '',
  rentalUnitId: '',
  startDate: new Date().toISOString().substring(0, 10),
  endDate: '',
  monthlyRent: 0,
  depositAmount: 0,
  paymentDueDay: 5,
  paymentSchedule: 'Monthly' as PaymentSchedule,
  notes: ''
};

const ContractsPage = () => {
  const { data: contracts } = useQuery({ queryKey: ['contracts'], queryFn: getContracts });
  const { data: tenants } = useQuery({ queryKey: ['tenants'], queryFn: getTenants });
  const { data: units } = useQuery({ queryKey: ['units'], queryFn: getUnits });

  const availableUnits = useMemo(() => units?.filter(unit => unit.status !== 'Occupied') ?? [], [units]);

  const [open, setOpen] = useState(false);
  const [form, setForm] = useState(initialForm);
  const queryClient = useQueryClient();
  const { notify } = useNotification();

  const createMutation = useMutation({
    mutationFn: createContract,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['contracts'] });
      queryClient.invalidateQueries({ queryKey: ['units'] });
      setForm(initialForm);
      setOpen(false);
      notify('Contract created', 'success');
    },
    onError: (error: any) => {
      const message = error?.response?.data?.message ?? 'Unable to create contract';
      notify(message, 'error');
    }
  });

  const handleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    createMutation.mutate({ ...form, endDate: form.endDate || null, monthlyRent: Number(form.monthlyRent) });
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
      <div className="section-title">
        <h3>Lease Contracts</h3>
        <button type="button" className="primary-button" onClick={() => setOpen(true)}>
          New Contract
        </button>
      </div>

      <div className="table-wrapper">
        <table className="table">
          <thead>
            <tr>
              <th>Tenant</th>
              <th>Unit</th>
              <th>Start</th>
              <th>Rent</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            {contracts?.map(contract => (
              <tr key={contract.id}>
                <td>{contract.tenantName}</td>
                <td>{contract.rentalUnitName}</td>
                <td>{format(new Date(contract.startDate), 'MMM dd, yyyy')}</td>
                <td>{formatCurrency(contract.monthlyRent, contract.currency ?? 'XAF')}</td>
                <td>{contract.status}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <Modal title="New Contract" open={open} onClose={() => setOpen(false)}>
        <form onSubmit={handleSubmit} className="form-grid">
          <FormField label="Tenant" name="tenantId">
            <select
              id="tenantId"
              value={form.tenantId}
              onChange={event => setForm({ ...form, tenantId: event.target.value })}
              required
            >
              <option value="">Select tenant</option>
              {tenants?.map(tenant => (
                <option key={tenant.id} value={tenant.id}>
                  {tenant.firstName} {tenant.lastName}
                </option>
              ))}
            </select>
          </FormField>
          <FormField label="Rental unit" name="rentalUnitId">
            <select
              id="rentalUnitId"
              value={form.rentalUnitId}
              onChange={event => setForm({ ...form, rentalUnitId: event.target.value })}
              required
            >
              <option value="">Select unit</option>
              {availableUnits.map(unit => (
                <option key={unit.id} value={unit.id}>{unit.name}</option>
              ))}
            </select>
          </FormField>
          <FormField label="Start date" name="startDate">
            <input
              id="startDate"
              type="date"
              value={form.startDate}
              onChange={event => setForm({ ...form, startDate: event.target.value })}
              required
            />
          </FormField>
          <FormField label="End date" name="endDate">
            <input
              id="endDate"
              type="date"
              value={form.endDate}
              onChange={event => setForm({ ...form, endDate: event.target.value })}
            />
          </FormField>
          <FormField label="Monthly rent" name="monthlyRent">
            <input
              id="monthlyRent"
              type="number"
              min={0}
              step="0.01"
              value={form.monthlyRent}
              onChange={event => setForm({ ...form, monthlyRent: Number(event.target.value) })}
              required
            />
          </FormField>
          <FormField label="Deposit" name="depositAmount">
            <input
              id="depositAmount"
              type="number"
              min={0}
              step="0.01"
              value={form.depositAmount}
              onChange={event => setForm({ ...form, depositAmount: Number(event.target.value) })}
            />
          </FormField>
          <FormField label="Due day" name="paymentDueDay">
            <input
              id="paymentDueDay"
              type="number"
              min={1}
              max={28}
              value={form.paymentDueDay}
              onChange={event => setForm({ ...form, paymentDueDay: Number(event.target.value) })}
              required
            />
          </FormField>
          <FormField label="Schedule" name="paymentSchedule">
            <select
              id="paymentSchedule"
              value={form.paymentSchedule}
              onChange={event => setForm({ ...form, paymentSchedule: event.target.value as PaymentSchedule })}
            >
              {schedules.map(schedule => (
                <option key={schedule} value={schedule}>{schedule}</option>
              ))}
            </select>
          </FormField>
          <label className="form-control" htmlFor="contract-notes">
            <span>Notes</span>
            <textarea
              id="contract-notes"
              rows={3}
              value={form.notes}
              onChange={event => setForm({ ...form, notes: event.target.value })}
            />
          </label>
          <div className="button-row" style={{ gridColumn: '1 / -1' }}>
            <button type="button" className="secondary-button" onClick={() => setOpen(false)}>
              Cancel
            </button>
            <button type="submit" className="primary-button">
              Save Contract
            </button>
          </div>
        </form>
      </Modal>
    </div>
  );
};

export default ContractsPage;
