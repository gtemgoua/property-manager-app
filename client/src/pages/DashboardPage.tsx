import { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Line } from 'react-chartjs-2';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Tooltip,
  Legend
} from 'chart.js';
import { format } from 'date-fns';
import { formatCurrency } from '../utils/currency';
import Card from '../components/Card';
import StatusBadge from '../components/StatusBadge';
import { getDashboardMetrics } from '../api/reports';
import { getAlerts } from '../api/alerts';

ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, Tooltip, Legend);

const DashboardPage = () => {
  const { data: metrics, isLoading } = useQuery({
    queryKey: ['dashboard-metrics'],
    queryFn: () => getDashboardMetrics()
  });

  const { data: alerts } = useQuery({
    queryKey: ['alerts'],
    queryFn: () => getAlerts()
  });

  const rentChart = useMemo(() => {
    if (!metrics) return undefined;
    const labels = metrics.rentCollection.map(point => format(new Date(point.year, point.month - 1), 'MMM yyyy'));
    return {
      labels,
      datasets: [
        {
          label: 'Amount Due',
          data: metrics.rentCollection.map(point => point.amountDue),
          borderColor: '#2563eb',
          backgroundColor: 'rgba(37, 99, 235, 0.15)',
          tension: 0.4,
          fill: true
        },
        {
          label: 'Amount Paid',
          data: metrics.rentCollection.map(point => point.amountPaid),
          borderColor: '#16a34a',
          backgroundColor: 'rgba(22, 163, 74, 0.15)',
          tension: 0.4,
          fill: true
        }
      ]
    };
  }, [metrics]);

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
      <div className="card-grid">
        <Card title="Active Tenants" value={isLoading ? '...' : metrics?.totalTenants ?? 0} />
        <Card title="Units" value={isLoading ? '...' : metrics?.totalUnits ?? 0} footer={`${metrics?.occupiedUnits ?? 0} occupied`} />
        <Card title="Monthly Recurring" value={isLoading ? '...' : `$${metrics?.monthlyRecurringRevenue?.toFixed(2)}`} />
        <Card title="Outstanding" value={isLoading ? '...' : `$${metrics?.monthlyOutstanding?.toFixed(2)}`} />
      </div>

      {alerts && alerts.length > 0 && (
        <section>
          <h3 className="section-title">Late Payment Alerts</h3>
          <div className="table-wrapper">
            <table className="table">
              <thead>
                <tr>
                  <th>Message</th>
                  <th>Created</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {alerts.map(alert => (
                  <tr key={alert.id}>
                    <td>{alert.message}</td>
                    <td>{format(new Date(alert.alertDate), 'MMM dd, yyyy')}</td>
                    <td><StatusBadge status={alert.isAcknowledged ? 'Acknowledged' : 'Late'} /></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>
      )}

      <section>
        <h3 className="section-title">Rent Performance</h3>
        <div style={{ background: '#ffffff', borderRadius: '1rem', padding: '1rem', border: '1px solid #e5e7eb' }}>
          {rentChart ? <Line data={rentChart} /> : <p>Loading chart...</p>}
        </div>
      </section>
    </div>
  );
};

export default DashboardPage;
