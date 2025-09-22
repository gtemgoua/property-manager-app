import clsx from 'clsx';

type StatusInput = string | number | undefined | null;

const statusMap: Record<number, string> = {
  0: 'Pending',
  1: 'Paid',
  2: 'Partial',
  3: 'Late',
  4: 'Waived'
};

const normalizeStatus = (status: StatusInput) => {
  if (status === null || status === undefined) return 'Unknown';
  if (typeof status === 'number') return statusMap[status] ?? 'Unknown';
  return String(status);
};

const StatusBadge = ({ status }: { status: StatusInput }) => {
  const label = normalizeStatus(status);
  return <span className={clsx('status-badge', `status-${label.toLowerCase()}`)}>{label}</span>;
};

export default StatusBadge;
