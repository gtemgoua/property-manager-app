import { Navigate, Route, Routes } from 'react-router-dom';
import Layout from './components/Layout';
import DashboardPage from './pages/DashboardPage';
import TenantsPage from './pages/TenantsPage';
import UnitsPage from './pages/UnitsPage';
import ContractsPage from './pages/ContractsPage';
import PaymentsPage from './pages/PaymentsPage';
import ReportsPage from './pages/ReportsPage';

const App = () => (
  <Layout>
    <Routes>
      <Route path="/" element={<Navigate to="/dashboard" replace />} />
      <Route path="/dashboard" element={<DashboardPage />} />
      <Route path="/tenants" element={<TenantsPage />} />
      <Route path="/units" element={<UnitsPage />} />
      <Route path="/contracts" element={<ContractsPage />} />
      <Route path="/payments" element={<PaymentsPage />} />
      <Route path="/reports" element={<ReportsPage />} />
    </Routes>
  </Layout>
);

export default App;
