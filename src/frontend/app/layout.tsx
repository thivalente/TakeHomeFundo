import type { Metadata } from 'next';
import './globals.css';

export const metadata: Metadata = {
  title: 'Task Home Fundo',
  description: 'Simple loan application flow',
  icons: { icon: '/icon.svg' },
};

export default function RootLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return <html lang="en"><body>{children}</body></html>;
}
