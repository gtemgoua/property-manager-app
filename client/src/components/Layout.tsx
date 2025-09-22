import { useState } from 'react';
import { NavLink } from 'react-router-dom';
import type { ReactNode } from 'react';
import TopBar from './TopBar';

const navItems = [
  { to: '/dashboard', label: 'Dashboard' },
  { to: '/tenants', label: 'Tenants' },
  { to: '/units', label: 'Units' },
  { to: '/contracts', label: 'Contracts' },
  { to: '/payments', label: 'Payments' },
  { to: '/reports', label: 'Reports' }
];

const Layout = ({ children }: { children: ReactNode }) => {
  const [sidebarOpen, setSidebarOpen] = useState(false);

  return (
    <div className="app-layout">
      <aside className={`sidebar ${sidebarOpen ? 'open' : ''}`}>
        <div>
          <h1>Property Manager</h1>
          <p style={{ color: 'rgba(255,255,255,0.7)', margin: '0' }}>Smart rent operations</p>
        </div>
        <nav>
          {navItems.map(item => (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) => (isActive ? 'active' : '')}
              onClick={() => setSidebarOpen(false)}
            >
              {item.label}
            </NavLink>
          ))}
        </nav>
      </aside>
      {sidebarOpen && (
        <div className="modal-overlay" onClick={() => setSidebarOpen(false)} aria-hidden />
      )}
      <main className="main-content">
        <TopBar onMenuClick={() => setSidebarOpen(open => !open)} />
        {children}
      </main>
    </div>
  );
};

export default Layout;
