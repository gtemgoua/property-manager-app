import { Link, useLocation } from 'react-router-dom';

const titles: Record<string, string> = {
  '/dashboard': 'Performance Overview',
  '/tenants': 'Tenant Directory',
  '/units': 'Rental Units',
  '/contracts': 'Lease Contracts',
  '/payments': 'Rent Payments',
  '/reports': 'Reports & Exports'
};

const TopBar = ({ onMenuClick }: { onMenuClick: () => void }) => {
  const location = useLocation();
  const title = titles[location.pathname] ?? 'Property Manager';

  return (
    <header className="top-bar">
      <button className="mobile-nav-toggle" type="button" onClick={onMenuClick} aria-label="Toggle navigation">
        ☰
      </button>
      <div>
        <h2>{title}</h2>
        <small style={{ color: '#64748b' }}>Stay on top of tenants, leases and payments.</small>
      </div>
      <Link to="/reports" className="primary-button" style={{ textDecoration: 'none' }}>
        Export Data
      </Link>
    </header>
  );
};

export default TopBar;
