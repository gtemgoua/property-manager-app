import { createContext, useCallback, useContext, useMemo, useState } from 'react';
import type { ReactNode } from 'react';
import { createPortal } from 'react-dom';

export type NotificationType = 'success' | 'error' | 'info';

export interface Notification {
  id: string;
  message: string;
  type: NotificationType;
}

interface NotificationContextValue {
  notify: (message: string, type?: NotificationType) => void;
}

const NotificationContext = createContext<NotificationContextValue | undefined>(undefined);

export const NotificationProvider = ({ children }: { children: ReactNode }) => {
  const [notifications, setNotifications] = useState<Notification[]>([]);

  const notify = useCallback((message: string, type: NotificationType = 'info') => {
    const id = crypto.randomUUID();
    setNotifications(current => [...current, { id, message, type }]);
    setTimeout(() => {
      setNotifications(current => current.filter(notification => notification.id !== id));
    }, 5000);
  }, []);

  const value = useMemo(() => ({ notify }), [notify]);

  return (
    <NotificationContext.Provider value={value}>
      {children}
      {createPortal(
        <div className="notification-container">
          {notifications.map(notification => (
            <div key={notification.id} className={`notification notification-${notification.type}`}>
              {notification.message}
            </div>
          ))}
        </div>,
        document.body
      )}
    </NotificationContext.Provider>
  );
};

export const useNotification = () => {
  const context = useContext(NotificationContext);
  if (!context) {
    throw new Error('useNotification must be used within a NotificationProvider');
  }
  return context;
};
