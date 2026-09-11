'use client';

import Link from 'next/link';
import { useApplicationFlow } from '../application-flow-context';

export default function DeniedPage() {
  const { denialReasons: reasons } = useApplicationFlow();

  return (
    <main className="site-shell">
      <header className="site-header">
        <a className="brand" href="/" aria-label="Task Home Fundo home"><span className="brand-mark" aria-hidden="true">T</span><span>Task Home Fundo</span></a>
        <span className="header-note">Eligibility check</span>
      </header>

      <section className="hero denial-hero" aria-labelledby="denial-title">
        <div className="hero-copy"><p className="eyebrow">APPLICATION UPDATE</p><h1 id="denial-title">Your application wasn&apos;t approved.</h1><p className="hero-description">Based on the information provided, we can&apos;t offer an eligible application at this time.</p></div>

        <div className="application-card denial-card" role="alert" aria-live="polite">
          <div className="card-heading"><div><p className="section-kicker">WHY THIS HAPPENED</p><h2>Eligibility reasons</h2></div><span className="denial-icon" aria-hidden="true">!</span></div>
          {reasons.length > 0 ? <ul className="denial-reasons">{reasons.map((reason, index) => <li key={`${reason}-${index}`}>{reason}</li>)}</ul> : <p className="denial-fallback">We couldn&apos;t load the eligibility reasons. Please return to the form and try again.</p>}
          <Link className="button button-secondary denial-back-button" href="/"><span aria-hidden="true">←</span> Back to application</Link>
        </div>
      </section>

      <footer className="site-footer"><span>© 2026 Task Home Fundo</span><span>Local take-home demo</span></footer>
    </main>
  );
}
