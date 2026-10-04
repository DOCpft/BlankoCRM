import { NavLink } from 'react-router-dom';
import type { CSSProperties } from 'react';
import styles from './Sidebar.module.css';
import type { SidebarProps } from './Sidebar.types';

export function Sidebar({
  items,
  isOpen,
  onClose,
  logo = 'BlankoCRM',
  dragOffset = 0,
  isDragging = false,
}: SidebarProps) {
  const sidebarStyle = { '--drag-offset': `${dragOffset * 100}%` } as CSSProperties;

  return (
    <>
      {isOpen && <button aria-label="Закрыть меню" className={styles.overlay} onClick={onClose} />}
      <aside
        aria-label="Основная навигация"
        className={`${styles.sidebar} ${isOpen ? styles.sidebarOpen : ''} ${isDragging ? styles.sidebarDragging : ''}`}
        style={sidebarStyle}
      >
        <div className={styles.logo}>{logo}</div>
        <nav className={styles.navigation}>
          {items.map((item) => (
            <NavLink
              key={item.id}
              to={item.href}
              end={item.href === '/'}
              onClick={onClose}
              className={({ isActive }) => `${styles.link} ${isActive ? styles.linkActive : ''}`}
            >
              <span aria-hidden="true">{item.icon}</span>
              <span>{item.label}</span>
              {item.badge !== undefined && <span className={styles.badge}>{item.badge}</span>}
            </NavLink>
          ))}
        </nav>
      </aside>
    </>
  );
}
