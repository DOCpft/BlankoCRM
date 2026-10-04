import { createRoot } from 'react-dom/client';
import '../../shared/src/ui/styles.css';
import { ClientApp } from './ClientBookingApp';

createRoot(document.getElementById('root')!).render(<ClientApp />);
