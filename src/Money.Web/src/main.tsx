import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { App } from './App';
import { aplicarTemaGuardado } from './comum/tema';
import './estilo.css';

aplicarTemaGuardado();

createRoot(document.getElementById('raiz') as HTMLElement).render(
  <StrictMode>
    <App />
  </StrictMode>,
);
