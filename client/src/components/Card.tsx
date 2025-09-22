import type { ReactNode } from 'react';

interface CardProps {
  title: string;
  value: ReactNode;
  footer?: ReactNode;
}

const Card = ({ title, value, footer }: CardProps) => (
  <div className="card">
    <h3>{title}</h3>
    <strong>{value}</strong>
    {footer && <span style={{ color: '#94a3b8', fontSize: '0.85rem' }}>{footer}</span>}
  </div>
);

export default Card;
