import { Outlet } from 'react-router-dom';
import '../styles/components.css';

/**
 * Layout component - wraps all pages
 * Provides consistent structure and styling
 */
export function Layout() {
  return (
    <div className="app-layout">
      <main className="main-content">
        <Outlet />
      </main>
    </div>
  );
}
