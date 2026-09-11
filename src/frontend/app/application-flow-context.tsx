'use client';

import { createContext, useContext, useState, type ReactNode } from 'react';

export type ApplicationDraft = {
  firstName: string;
  lastName: string;
  companyName: string;
  ssn: string;
  address: string;
  state: string;
  requestedAmount: string;
};

type ApplicationFlow = {
  draft: ApplicationDraft | null;
  denialReasons: string[];
  setApplicationResult: (draft: ApplicationDraft, reasons: string[]) => void;
  clearDraft: () => void;
};

const ApplicationFlowContext = createContext<ApplicationFlow | null>(null);

export function ApplicationFlowProvider({ children }: { children: ReactNode }) {
  const [draft, setDraft] = useState<ApplicationDraft | null>(null);
  const [denialReasons, setDenialReasons] = useState<string[]>([]);

  return <ApplicationFlowContext.Provider value={{
    draft,
    denialReasons,
    setApplicationResult: (nextDraft, reasons) => {
      setDraft(nextDraft);
      setDenialReasons(reasons);
    },
    clearDraft: () => {
      setDraft(null);
      setDenialReasons([]);
    },
  }}>{children}</ApplicationFlowContext.Provider>;
}

export function useApplicationFlow() {
  const context = useContext(ApplicationFlowContext);
  if (!context) throw new Error('useApplicationFlow must be used within ApplicationFlowProvider');
  return context;
}
