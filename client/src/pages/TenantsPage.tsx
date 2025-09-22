import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { format } from 'date-fns';
import Modal from '../components/Modal';
import FormField from '../components/FormField';
import { createTenant, deleteTenant, getTenants } from '../api/tenants';
import { useNotification } from '../context/NotificationContext';

const initialForm = {
  firstName: '',
  lastName: '',
  email: '',
  phoneNumber: '',
  emergencyContactName: '',
  emergencyContactPhone: '',
  notes: ''
};

const TenantsPage = () => {
  const { data: tenants } = useQuery({ queryKey: ['tenants'], queryFn: getTenants });
  const queryClient = useQueryClient();
  const { notify } = useNotification();
  const [form, setForm] = useState(initialForm);
  const [open, setOpen] = useState(false);

  const createMutation = useMutation({
    mutationFn: createTenant,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tenants'] });
      setForm(initialForm);
      setOpen(false);
      notify('Tenant added successfully', 'success');
    },
    onError: () => notify('Unable to add tenant', 'error')
  });

  const deleteMutation = useMutation({
    mutationFn: deleteTenant,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tenants'] });
      notify('Tenant deleted', 'success');
    },
    onError: () => notify('Unable to delete tenant', 'error')
  });

  const handleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    createMutation.mutate(form);
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
      <div className="section-title">
        <h3>Tenant Directory</h3>
        <button type="button" className="primary-button" onClick={() => setOpen(true)}>
          Add Tenant
        </button>
      </div>

      <div className="table-wrapper">
        <table className="table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Email</th>
              <th>Phone</th>
              <th>Created</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {tenants?.map(tenant => (
              <tr key={tenant.id}>
                <td>{tenant.firstName} {tenant.lastName}</td>
                <td>{tenant.email}</td>
                <td>{tenant.phoneNumber}</td>
                <td>{format(new Date(tenant.createdAt), 'MMM dd, yyyy')}</td>
                <td>
                  <button
                    type="button"
                    className="secondary-button"
                    onClick={() => deleteMutation.mutate(tenant.id)}
                  >
                    Remove
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <Modal title="Add Tenant" open={open} onClose={() => setOpen(false)}>
        <form onSubmit={handleSubmit} className="form-grid">
          <FormField label="First name" name="firstName">
            <input
              id="firstName"
              value={form.firstName}
              onChange={event => setForm({ ...form, firstName: event.target.value })}
              required
            />
          </FormField>
          <FormField label="Last name" name="lastName">
            <input
              id="lastName"
              value={form.lastName}
              onChange={event => setForm({ ...form, lastName: event.target.value })}
              required
            />
          </FormField>
          <FormField label="Email" name="email">
            <input
              id="email"
              type="email"
              value={form.email}
              onChange={event => setForm({ ...form, email: event.target.value })}
              required
            />
          </FormField>
          <FormField label="Phone" name="phoneNumber">
            <input
              id="phoneNumber"
              value={form.phoneNumber}
              onChange={event => setForm({ ...form, phoneNumber: event.target.value })}
              required
            />
          </FormField>
          <FormField label="Emergency contact" name="emergencyContactName">
            <input
              id="emergencyContactName"
              value={form.emergencyContactName}
              onChange={event => setForm({ ...form, emergencyContactName: event.target.value })}
            />
          </FormField>
          <FormField label="Emergency phone" name="emergencyContactPhone">
            <input
              id="emergencyContactPhone"
              value={form.emergencyContactPhone}
              onChange={event => setForm({ ...form, emergencyContactPhone: event.target.value })}
            />
          </FormField>
          <label className="form-control" htmlFor="notes">
            <span>Notes</span>
            <textarea
              id="notes"
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
              Save Tenant
            </button>
          </div>
        </form>
      </Modal>
    </div>
  );
};

export default TenantsPage;
