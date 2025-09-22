import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import Modal from '../components/Modal';
import FormField from '../components/FormField';
import { createUnit, deleteUnit, getUnits } from '../api/units';
import type { RentalUnitStatus } from '../types/shared';
import { useNotification } from '../context/NotificationContext';
import { formatCurrency } from '../utils/currency';

const statuses: RentalUnitStatus[] = ['Available', 'Occupied', 'Maintenance', 'Archived'];

const initialForm = {
  name: '',
  addressLine1: '',
  addressLine2: '',
  city: '',
  state: '',
  postalCode: '',
  monthlyRent: 0,
  bedrooms: 1,
  bathrooms: 1,
  squareFeet: 50,
  status: 'Available' as RentalUnitStatus,
  notes: ''
};

const UnitsPage = () => {
  const { data: units } = useQuery({ queryKey: ['units'], queryFn: getUnits });
  const queryClient = useQueryClient();
  const { notify } = useNotification();
  const [open, setOpen] = useState(false);
  const [form, setForm] = useState(initialForm);

  const createMutation = useMutation({
    mutationFn: createUnit,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['units'] });
      setForm(initialForm);
      setOpen(false);
      notify('Rental unit added', 'success');
    },
    onError: () => notify('Unable to add unit', 'error')
  });

  const deleteMutation = useMutation({
    mutationFn: deleteUnit,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['units'] });
      notify('Unit removed', 'success');
    },
    onError: () => notify('Unable to remove unit', 'error')
  });

  const handleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    createMutation.mutate({ ...form, monthlyRent: Number(form.monthlyRent) });
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
      <div className="section-title">
        <h3>Rental Units</h3>
        <button type="button" className="primary-button" onClick={() => setOpen(true)}>
          Add Unit
        </button>
      </div>

      <div className="table-wrapper">
        <table className="table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Rent</th>
              <th>Bedrooms</th>
              <th>Status</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {units?.map(unit => (
              <tr key={unit.id}>
                <td>{unit.name}</td>
                <td>{formatCurrency(unit.monthlyRent, 'XAF')}</td>
                <td>{unit.bedrooms} bd / {unit.bathrooms} ba</td>
                <td>{unit.status}</td>
                <td>
                  <button type="button" className="secondary-button" onClick={() => deleteMutation.mutate(unit.id)}>
                    Remove
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <Modal title="Add Unit" open={open} onClose={() => setOpen(false)}>
        <form onSubmit={handleSubmit} className="form-grid">
          <FormField label="Name" name="name">
            <input id="name" value={form.name} onChange={event => setForm({ ...form, name: event.target.value })} required />
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
          <FormField label="Bedrooms" name="bedrooms">
            <input
              id="bedrooms"
              type="number"
              min={0}
              value={form.bedrooms}
              onChange={event => setForm({ ...form, bedrooms: Number(event.target.value) })}
              required
            />
          </FormField>
          <FormField label="Bathrooms" name="bathrooms">
            <input
              id="bathrooms"
              type="number"
              min={0}
              value={form.bathrooms}
              onChange={event => setForm({ ...form, bathrooms: Number(event.target.value) })}
              required
            />
          </FormField>
          <FormField label="Square feet" name="squareFeet">
            <input
              id="squareFeet"
              type="number"
              min={0}
              value={form.squareFeet}
              onChange={event => setForm({ ...form, squareFeet: Number(event.target.value) })}
            />
          </FormField>
          <FormField label="Status" name="status">
            <select
              id="status"
              value={form.status}
              onChange={event => setForm({ ...form, status: event.target.value as RentalUnitStatus })}
            >
              {statuses.map(status => (
                <option key={status} value={status}>{status}</option>
              ))}
            </select>
          </FormField>
          <FormField label="Address line 1" name="addressLine1">
            <input
              id="addressLine1"
              value={form.addressLine1}
              onChange={event => setForm({ ...form, addressLine1: event.target.value })}
              required
            />
          </FormField>
          <FormField label="Address line 2" name="addressLine2">
            <input
              id="addressLine2"
              value={form.addressLine2}
              onChange={event => setForm({ ...form, addressLine2: event.target.value })}
            />
          </FormField>
          <FormField label="City" name="city">
            <input id="city" value={form.city} onChange={event => setForm({ ...form, city: event.target.value })} required />
          </FormField>
          <FormField label="State" name="state">
            <input id="state" value={form.state} onChange={event => setForm({ ...form, state: event.target.value })} required />
          </FormField>
          <FormField label="Postal code" name="postalCode">
            <input
              id="postalCode"
              value={form.postalCode}
              onChange={event => setForm({ ...form, postalCode: event.target.value })}
              required
            />
          </FormField>
          <label className="form-control" htmlFor="notes-unit">
            <span>Notes</span>
            <textarea
              id="notes-unit"
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
              Save Unit
            </button>
          </div>
        </form>
      </Modal>
    </div>
  );
};

export default UnitsPage;
