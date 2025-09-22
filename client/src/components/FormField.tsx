import type { ReactNode } from 'react';

interface FormFieldProps {
  label: string;
  name: string;
  children: ReactNode;
}

const FormField = ({ label, name, children }: FormFieldProps) => (
  <label htmlFor={name} className="form-control">
    <span>{label}</span>
    {children}
  </label>
);

export default FormField;
