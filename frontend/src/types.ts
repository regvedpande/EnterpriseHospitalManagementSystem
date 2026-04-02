export type PortalMetric = {
  label: string;
  value: string;
  icon: string;
  tone: string;
  description?: string | null;
};

export type PortalChart = {
  title: string;
  subtitle: string;
  labels: string[];
  values: number[];
};

export type PortalRecentItem = {
  title: string;
  subtitle: string;
  meta: string;
  status: string;
  icon: string;
  tone: string;
};

export type PortalDashboard = {
  title: string;
  subtitle: string;
  metrics: PortalMetric[];
  charts: PortalChart[];
  recentItems: PortalRecentItem[];
};

export type PortalNavigationItem = {
  key: string;
  label: string;
  route: string;
  icon: string;
};

export type PortalUser = {
  id: string;
  name: string;
  email: string;
  role: string;
  roleDisplay: string;
  defaultRoute: string;
  initials: string;
};

export type AiMetric = {
  label: string;
  value: string;
  icon: string;
  tone: string;
  description?: string | null;
};

export type AiFact = {
  label: string;
  value: string;
};

export type AiSection = {
  title: string;
  items: string[];
};

export type AiAssistantPage = {
  role: number;
  areaName: string;
  pageTitle: string;
  sidebarTitle: string;
  subtitle: string;
  promptLabel: string;
  promptPlaceholder: string;
  submitLabel: string;
  prompt: string;
  responseTitle: string;
  responseSummary: string;
  responseStatusLabel: string;
  responseStatusTone: string;
  disclaimer: string;
  providerStatusLabel: string;
  providerStatusTone: string;
  providerNote: string;
  aiNarrative?: string | null;
  capabilities: string[];
  suggestedPrompts: string[];
  liveInsights: string[];
  detectedSignals: string[];
  metrics: AiMetric[];
  responseFacts: AiFact[];
  sections: AiSection[];
};

export type PortalBootstrap = {
  user: PortalUser;
  dashboard: PortalDashboard;
  aiAssistant?: AiAssistantPage | null;
  navigation: PortalNavigationItem[];
};

export type LoginResponse = {
  token: string;
  expiresInMinutes: number;
  user: PortalUser;
};
