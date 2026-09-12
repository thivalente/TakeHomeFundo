'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { Controller, type UseFormRegister, useForm } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useApplicationFlow } from './application-flow-context';

const states = [
  ['AL', 'Alabama'], ['AK', 'Alaska'], ['AZ', 'Arizona'], ['AR', 'Arkansas'], ['CA', 'California'],
  ['CO', 'Colorado'], ['CT', 'Connecticut'], ['DE', 'Delaware'], ['DC', 'District of Columbia'],
  ['FL', 'Florida'], ['GA', 'Georgia'], ['HI', 'Hawaii'], ['ID', 'Idaho'], ['IL', 'Illinois'],
  ['IN', 'Indiana'], ['IA', 'Iowa'], ['KS', 'Kansas'], ['KY', 'Kentucky'], ['LA', 'Louisiana'],
  ['ME', 'Maine'], ['MD', 'Maryland'], ['MA', 'Massachusetts'], ['MI', 'Michigan'], ['MN', 'Minnesota'],
  ['MS', 'Mississippi'], ['MO', 'Missouri'], ['MT', 'Montana'], ['NE', 'Nebraska'], ['NV', 'Nevada'],
  ['NH', 'New Hampshire'], ['NJ', 'New Jersey'], ['NM', 'New Mexico'], ['NY', 'New York'],
  ['NC', 'North Carolina'], ['ND', 'North Dakota'], ['OH', 'Ohio'], ['OK', 'Oklahoma'], ['OR', 'Oregon'],
  ['PA', 'Pennsylvania'], ['RI', 'Rhode Island'], ['SC', 'South Carolina'], ['SD', 'South Dakota'],
  ['TN', 'Tennessee'], ['TX', 'Texas'], ['UT', 'Utah'], ['VT', 'Vermont'], ['VA', 'Virginia'],
  ['WA', 'Washington'], ['WV', 'West Virginia'], ['WI', 'Wisconsin'], ['WY', 'Wyoming'],
] as const;

const stateCodes = states.map(([code]) => code) as [string, ...string[]];
const applicationSchema = z.object({
  firstName: z.string().trim().min(1, 'First name is required.').max(100, 'First name must be 100 characters or fewer.'),
  lastName: z.string().trim().min(1, 'Last name is required.').max(100, 'Last name must be 100 characters or fewer.'),
  companyName: z.string().trim().min(1, 'Company name is required.').max(200, 'Company name must be 200 characters or fewer.'),
  ssn: z.string().refine((value) => value.replace(/\D/g, '').length === 9, 'Enter a valid 9-digit SSN.'),
  address: z.string().trim().min(1, 'Address is required.').max(300, 'Address must be 300 characters or fewer.'),
  state: z.string().refine((value) => stateCodes.includes(value), 'Select a state.'),
  requestedAmount: z.string()
    .min(1, 'Requested amount is required.')
    .refine((value) => /^\d+(\.\d{1,2})?$/.test(value.replace(/,/g, '')), 'Enter a valid amount with up to 2 decimal places.')
    .refine((value) => Number(value.replace(/,/g, '')) > 0 && Number(value.replace(/,/g, '')) < 1_000_000, 'Amount must be greater than $0 and less than $1,000,000.'),
});

type ApplicationForm = z.infer<typeof applicationSchema>;

const emptyApplication: ApplicationForm = {
  firstName: '',
  lastName: '',
  companyName: '',
  ssn: '',
  address: '',
  state: '',
  requestedAmount: '',
};

function formatSsn(value: string) {
  const digits = value.replace(/\D/g, '').slice(0, 9);
  if (digits.length <= 3) return digits;
  if (digits.length <= 5) return `${digits.slice(0, 3)}-${digits.slice(3)}`;
  return `${digits.slice(0, 3)}-${digits.slice(3, 5)}-${digits.slice(5)}`;
}

function formatAmount(value: string) {
  const sanitized = value.replace(/[^\d.]/g, '');
  if (!sanitized) return '';
  const [whole = '', decimals] = sanitized.split('.');
  const formattedWhole = (whole.slice(0, 6) || '0').replace(/^0+(?=\d)/, '').replace(/\B(?=(\d{3})+(?!\d))/g, ',');
  return decimals === undefined ? formattedWhole : `${formattedWhole}.${decimals.slice(0, 2)}`;
}

function errorId(field: keyof ApplicationForm) { return `${field}-error`; }

export default function Home() {
  const router = useRouter();
  const { draft, setApplicationResult, clearDraft } = useApplicationFlow();
  const [showSsn, setShowSsn] = useState(false);
  const [formNotice, setFormNotice] = useState('');
  const [applicationReference, setApplicationReference] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const { register, control, handleSubmit, reset, setError, setFocus, formState: { errors } } = useForm<ApplicationForm>({
    resolver: zodResolver(applicationSchema),
    mode: 'onSubmit',
    defaultValues: emptyApplication,
  });

  useEffect(() => { if (draft) reset(draft); }, [draft, reset]);

  const onValid = async (values: ApplicationForm) => {
    setIsSubmitting(true);
    setFormNotice('');
    setApplicationReference('');
    try {
      const apiUrl = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:8317';
      const response = await fetch(`${apiUrl}/api/applications`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ ...values, ssn: values.ssn.replace(/\D/g, ''), requestedAmount: Number(values.requestedAmount.replace(/,/g, '')) }),
      });
      const body = await response.json().catch(() => null) as ApiResponse | null;
      if (response.ok) {
        clearDraft();
        setFormNotice('Your application was approved successfully.');
        setApplicationReference(body?.data?.applicationId ? `Reference: ${body.data.applicationId}` : '');
      } else {
        showApiErrors(response.status, body, values);
      }
    } catch {
      setFormNotice('We couldn’t submit your application. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const onClear = () => {
    clearDraft();
    reset(emptyApplication);
    setShowSsn(false);
    setFormNotice('');
    setApplicationReference('');
  };

  const showApiErrors = (status: number, body: ApiResponse | null, values: ApplicationForm) => {
    const apiErrors = body?.errors ?? [];
    if (status !== 422) {
      apiErrors.forEach((error) => {
        const field = toFormField(error.field);
        if (field) setError(field, { type: 'server', message: error.message });
      });
    }
    const messages = apiErrors.map((error) => error.message).filter(Boolean);
    if (status === 422 && messages.length > 0) {
      setApplicationResult(values, messages);
      router.push(new URL('/denied/', window.location.origin).toString());
    } else {
      setFormNotice(status === 400 && messages.length > 0 ? messages.join(' ') : 'We couldn’t submit your application. Please try again.');
    }
  };
  const onInvalid = (formErrors: Partial<Record<keyof ApplicationForm, unknown>>) => {
    const firstInvalid = Object.keys(formErrors)[0] as keyof ApplicationForm | undefined;
    setFormNotice('Please review the highlighted fields.');
    if (firstInvalid) setFocus(firstInvalid);
  };

  const fieldError = (field: keyof ApplicationForm) => errors[field]?.message;

  return (
    <main className="site-shell">
      <header className="site-header">
        <a className="brand" href="/" aria-label="Task Home Fundo home"><span className="brand-mark" aria-hidden="true">T</span><span>Task Home Fundo</span></a>
        <span className="header-note">Eligibility check</span>
      </header>

      <section className="hero" aria-labelledby="page-title">
        <div className="hero-copy"><p className="eyebrow">PERSONAL LOAN APPLICATION</p><h1 id="page-title">Submit your loan application.</h1><p className="hero-description">Enter your details and requested amount to check your eligibility.</p></div>

        <form className="application-card" aria-label="Loan application form" noValidate onSubmit={handleSubmit(onValid, onInvalid)}>
          <div className="card-heading"><div><p className="section-kicker">APPLICATION DETAILS</p><h2>Your application</h2></div><span className="step-indicator">1 <span>of 1</span></span></div>

          <div className="field-grid two-columns">
            <Field label="First Name" id="firstName" placeholder="Jane" maxLength={100} error={fieldError('firstName')} register={register} />
            <Field label="Last Name" id="lastName" placeholder="Doe" maxLength={100} error={fieldError('lastName')} register={register} />
            <Field label="Company Name" id="companyName" placeholder="Acme Inc." maxLength={200} error={fieldError('companyName')} register={register} />
            <div className="field-wrap"><label className="field" htmlFor="ssn"><span>SSN</span><div className="input-with-action"><Controller name="ssn" control={control} render={({ field }) => <input {...field} id="ssn" placeholder="000-00-0000" type={showSsn ? 'text' : 'password'} inputMode="numeric" autoComplete="off" aria-invalid={Boolean(errors.ssn)} aria-describedby={errors.ssn ? errorId('ssn') : undefined} onChange={(event) => field.onChange(formatSsn(event.target.value))} />} /><button className="input-action" type="button" onClick={() => setShowSsn((visible) => !visible)} aria-label={showSsn ? 'Hide SSN' : 'Show SSN'}>{showSsn ? 'Hide' : 'Show'}</button></div></label><FieldError id={errorId('ssn')} message={fieldError('ssn')} /></div>
          </div>

          <div className="field-grid address-row"><Field label="Address" id="address" placeholder="123 Main Street" maxLength={300} error={fieldError('address')} register={register} /><div className="field-wrap"><label className="field" htmlFor="state"><span>State</span><select id="state" defaultValue="" {...register('state')} aria-invalid={Boolean(errors.state)} aria-describedby={errors.state ? errorId('state') : undefined}><option value="" disabled>Select</option>{states.map(([code]) => <option value={code} key={code}>{code}</option>)}</select></label><FieldError id={errorId('state')} message={fieldError('state')} /></div></div>

          <div className="field-wrap"><label className="field" htmlFor="requestedAmount"><span>Requested Amount</span><div className="amount-input"><span aria-hidden="true">$</span><Controller name="requestedAmount" control={control} render={({ field }) => <input {...field} id="requestedAmount" placeholder="0.00" maxLength={10} inputMode="decimal" type="text" aria-invalid={Boolean(errors.requestedAmount)} aria-describedby={errors.requestedAmount ? errorId('requestedAmount') : undefined} onChange={(event) => field.onChange(formatAmount(event.target.value))} />} /></div></label><FieldError id={errorId('requestedAmount')} message={fieldError('requestedAmount')} /></div>

          <div className="form-actions"><button className="button button-primary" type="submit" disabled={isSubmitting}>{isSubmitting ? <><span className="spinner" aria-hidden="true" />Submitting...</> : <>Apply <span aria-hidden="true">→</span></>}</button><button className="button button-secondary" type="button" disabled={isSubmitting} onClick={onClear}>Clear</button></div>
          {formNotice === 'Your application was approved successfully.' ? <div className="approval-card" role="status" aria-live="polite"><div className="approval-icon" aria-hidden="true">✓</div><div><p className="approval-kicker">APPLICATION APPROVED</p><p className="approval-title">You&apos;re all set!</p><p className="approval-copy">Your application was approved successfully.</p>{applicationReference ? <p className="application-reference">{applicationReference}</p> : null}</div></div> : <p className="form-notice" aria-live="polite">{formNotice}</p>}
          <p className="privacy-note"><span aria-hidden="true">●</span> Review your details before submitting.</p>
        </form>
      </section>

      <footer className="site-footer"><span>© 2026 Task Home Fundo</span><span>Local take-home demo</span></footer>
    </main>
  );
}

function Field({ label, id, placeholder, maxLength, error, register }: { label: string; id: keyof ApplicationForm; placeholder: string; maxLength: number; error?: string; register: UseFormRegister<ApplicationForm> }) {
  return <div className="field-wrap"><label className="field" htmlFor={id}><span>{label}</span><input id={id} placeholder={placeholder} maxLength={maxLength} type="text" {...register(id)} aria-invalid={Boolean(error)} aria-describedby={error ? errorId(id) : undefined} /></label><FieldError id={errorId(id)} message={error} /></div>;
}

function FieldError({ id, message }: { id: string; message?: string }) {
  return <p className={`field-error${message ? ' visible' : ''}`} id={id} role={message ? 'alert' : undefined}>{message || '\u00a0'}</p>;
}

type ApiResponse = {
  data?: { applicationId?: string } | null;
  errors?: Array<{ code?: string; field?: string | null; message: string }>;
};

function toFormField(field?: string | null): keyof ApplicationForm | undefined {
  if (!field) return undefined;
  const candidate = `${field.charAt(0).toLowerCase()}${field.slice(1)}`;
  return ['firstName', 'lastName', 'companyName', 'ssn', 'address', 'state', 'requestedAmount'].includes(candidate)
    ? candidate as keyof ApplicationForm
    : undefined;
}
