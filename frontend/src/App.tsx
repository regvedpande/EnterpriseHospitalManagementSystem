import { FormEvent, startTransition, useEffect, useState } from "react";
import { NavLink, Navigate, Route, Routes, useLocation, useNavigate } from "react-router-dom";
import { api } from "./lib/api";
import type { AiAssistantPage, LoginResponse, PortalBootstrap } from "./types";

const TOKEN_KEY = "medcore.portal.token";
const USER_KEY = "medcore.portal.user";

const demoUsers = [
  { label: "Admin", email: "admin@hospital.com", password: "Admin@123" },
  { label: "Doctor", email: "doctor@hospital.com", password: "Doctor@123" },
  { label: "Receptionist", email: "receptionist@hospital.com", password: "Receptionist@123" },
  { label: "Pharmacist", email: "pharmacist@hospital.com", password: "Pharmacist@123" },
  { label: "Patient", email: "patient@hospital.com", password: "Patient@123" },
];

export default function App() {
  const [token, setToken] = useState<string | null>(() => localStorage.getItem(TOKEN_KEY));
  const [bootstrap, setBootstrap] = useState<PortalBootstrap | null>(null);
  const [loading, setLoading] = useState(Boolean(token));
  const [error, setError] = useState<string>("");

  useEffect(() => {
    if (!token) {
      setBootstrap(null);
      setLoading(false);
      return;
    }

    let cancelled = false;
    setLoading(true);
    setError("");

    api.bootstrap(token)
      .then((result) => {
        if (cancelled) return;
        setBootstrap(result);
        localStorage.setItem(USER_KEY, JSON.stringify(result.user));
      })
      .catch((err) => {
        if (cancelled) return;
        setError(err instanceof Error ? err.message : "Unable to load portal data.");
        clearSession(setToken, setBootstrap);
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [token]);

  if (!token) {
    return (
      <LoginPage
        onLoggedIn={(response) => {
          localStorage.setItem(TOKEN_KEY, response.token);
          localStorage.setItem(USER_KEY, JSON.stringify(response.user));
          setToken(response.token);
        }}
      />
    );
  }

  if (loading || !bootstrap) {
    return (
      <div className="screen-center">
        <div className="loading-card">
          <div className="eyebrow">Connecting to MedCore HMS</div>
          <h1>Loading your live portal</h1>
          <p>{error || "Pulling dashboard metrics, role access, and AI assistant status from the backend."}</p>
        </div>
      </div>
    );
  }

  return (
    <PortalShell
      bootstrap={bootstrap}
      onBootstrapChange={setBootstrap}
      token={token}
      onLogout={() => clearSession(setToken, setBootstrap)}
    />
  );
}

function LoginPage({ onLoggedIn }: { onLoggedIn: (response: LoginResponse) => void }) {
  const [email, setEmail] = useState("receptionist@hospital.com");
  const [password, setPassword] = useState("Receptionist@123");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setBusy(true);
    setError("");

    try {
      const response = await api.login(email, password);
      onLoggedIn(response);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Login failed.");
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="auth-layout">
      <section className="auth-hero">
        <div className="eyebrow">Recruiter-ready live demo</div>
        <h1>MedCore HMS React Portal</h1>
        <p>
          This standalone frontend talks to the .NET API over JWT, so you can demo the product without relying on
          Razor pages in production.
        </p>
        <div className="credential-grid">
          {demoUsers.map((user) => (
            <button
              key={user.email}
              type="button"
              className="credential-chip"
              onClick={() => {
                setEmail(user.email);
                setPassword(user.password);
              }}
            >
              <span>{user.label}</span>
              <small>{user.email}</small>
            </button>
          ))}
        </div>
      </section>

      <section className="auth-panel">
        <form className="auth-card" onSubmit={handleSubmit}>
          <div className="eyebrow">Portal access</div>
          <h2>Sign in</h2>
          <label className="field">
            <span>Email</span>
            <input value={email} onChange={(event) => setEmail(event.target.value)} type="email" required />
          </label>
          <label className="field">
            <span>Password</span>
            <input value={password} onChange={(event) => setPassword(event.target.value)} type="password" required />
          </label>
          {error ? <div className="form-error">{error}</div> : null}
          <button className="primary-button" type="submit" disabled={busy}>
            {busy ? "Signing in..." : "Enter portal"}
          </button>
        </form>
      </section>
    </div>
  );
}

function PortalShell({
  bootstrap,
  onBootstrapChange,
  token,
  onLogout,
}: {
  bootstrap: PortalBootstrap;
  onBootstrapChange: (value: PortalBootstrap) => void;
  token: string;
  onLogout: () => void;
}) {
  const location = useLocation();
  const navigate = useNavigate();

  useEffect(() => {
    if (location.pathname === "/") navigate("/dashboard", { replace: true });
  }, [location.pathname, navigate]);

  return (
    <div className="portal-layout">
      <aside className="sidebar">
        <div className="brand-block">
          <div className="brand-mark">MC</div>
          <div>
            <div className="brand-name">MedCore HMS</div>
            <div className="brand-subtitle">{bootstrap.user.roleDisplay} portal</div>
          </div>
        </div>

        <nav className="nav-stack">
          {bootstrap.navigation.map((item) => (
            <NavLink key={item.key} to={item.route} className={({ isActive }) => `nav-item${isActive ? " active" : ""}`}>
              {item.label}
            </NavLink>
          ))}
        </nav>

        <div className="sidebar-footer">
          <div className="provider-pill">
            {bootstrap.aiAssistant?.providerStatusLabel || "Dashboard ready"}
          </div>
        </div>
      </aside>

      <main className="portal-main">
        <header className="topbar-react">
          <div>
            <div className="eyebrow">Live backend session</div>
            <h1>{bootstrap.dashboard.title}</h1>
          </div>
          <div className="profile-block">
            <div className="avatar-react">{bootstrap.user.initials}</div>
            <div>
              <strong>{bootstrap.user.name}</strong>
              <div>{bootstrap.user.roleDisplay}</div>
            </div>
            <button type="button" className="ghost-button" onClick={onLogout}>
              Sign out
            </button>
          </div>
        </header>

        <Routes>
          <Route path="/dashboard" element={<DashboardPage dashboard={bootstrap.dashboard} />} />
          <Route
            path="/assistant"
            element={
              bootstrap.aiAssistant ? (
                <AssistantPage
                  token={token}
                  initial={bootstrap.aiAssistant}
                  onUpdated={(assistant) => onBootstrapChange({ ...bootstrap, aiAssistant: assistant })}
                />
              ) : (
                <UnavailableAssistant />
              )
            }
          />
          <Route path="/about" element={<AboutPage />} />
          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Routes>
      </main>
    </div>
  );
}

function DashboardPage({ dashboard }: { dashboard: PortalBootstrap["dashboard"] }) {
  return (
    <div className="page-stack">
      <section className="hero-card">
        <div className="eyebrow">Operational summary</div>
        <h2>{dashboard.title}</h2>
        <p>{dashboard.subtitle}</p>
      </section>

      <section className="metric-grid">
        {dashboard.metrics.map((metric) => (
          <article key={metric.label} className={`metric-card tone-${metric.tone}`}>
            <div className="metric-label">{metric.label}</div>
            <div className="metric-value">{metric.value}</div>
            {metric.description ? <div className="metric-note">{metric.description}</div> : null}
          </article>
        ))}
      </section>

      <section className="two-column">
        {dashboard.charts.map((chart) => (
          <article key={chart.title} className="panel-card">
            <div className="panel-header">
              <div>
                <h3>{chart.title}</h3>
                <p>{chart.subtitle}</p>
              </div>
            </div>
            <div className="chart-bars">
              {chart.labels.map((label, index) => {
                const value = chart.values[index] ?? 0;
                const maxValue = Math.max(...chart.values, 1);
                const width = `${Math.max((value / maxValue) * 100, 6)}%`;

                return (
                  <div className="chart-row" key={`${chart.title}-${label}`}>
                    <div className="chart-label">{label}</div>
                    <div className="chart-track">
                      <div className="chart-fill" style={{ width }} />
                    </div>
                    <div className="chart-value">{value}</div>
                  </div>
                );
              })}
            </div>
          </article>
        ))}
      </section>

      <section className="panel-card">
        <div className="panel-header">
          <div>
            <h3>Recent activity</h3>
            <p>Live items from the backend for the current role.</p>
          </div>
        </div>
        <div className="activity-list">
          {dashboard.recentItems.map((item, index) => (
            <article key={`${item.title}-${index}`} className="activity-item">
              <div>
                <strong>{item.title}</strong>
                <div>{item.subtitle}</div>
              </div>
              <div className="activity-meta">
                <span>{item.meta}</span>
                <span className={`status-pill tone-${item.tone}`}>{item.status}</span>
              </div>
            </article>
          ))}
        </div>
      </section>
    </div>
  );
}

function AssistantPage({
  token,
  initial,
  onUpdated,
}: {
  token: string;
  initial: AiAssistantPage;
  onUpdated: (value: AiAssistantPage) => void;
}) {
  const [assistant, setAssistant] = useState(initial);
  const [prompt, setPrompt] = useState(initial.prompt || "");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    setAssistant(initial);
    setPrompt(initial.prompt || "");
  }, [initial]);

  async function submitPrompt(event: FormEvent) {
    event.preventDefault();
    if (!prompt.trim()) return;

    setBusy(true);
    setError("");
    try {
      const next = await api.askAssistant(token, prompt.trim());
      startTransition(() => {
        setAssistant(next);
        onUpdated(next);
      });
    } catch (err) {
      setError(err instanceof Error ? err.message : "AI request failed.");
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="page-stack">
      <section className="hero-card">
        <div className="eyebrow">{assistant.providerStatusLabel}</div>
        <h2>{assistant.pageTitle}</h2>
        <p>{assistant.subtitle}</p>
        <div className="provider-inline">{assistant.providerNote}</div>
      </section>

      <section className="metric-grid">
        {assistant.metrics.map((metric) => (
          <article key={metric.label} className={`metric-card tone-${metric.tone}`}>
            <div className="metric-label">{metric.label}</div>
            <div className="metric-value">{metric.value}</div>
            {metric.description ? <div className="metric-note">{metric.description}</div> : null}
          </article>
        ))}
      </section>

      <section className="two-column">
        <article className="panel-card">
          <div className="panel-header">
            <div>
              <h3>Capabilities</h3>
              <p>Role-specific tools already enabled for this assistant.</p>
            </div>
          </div>
          <div className="chip-grid">
            {assistant.capabilities.map((capability) => (
              <span className="chip" key={capability}>
                {capability}
              </span>
            ))}
          </div>
          <div className="prompt-list">
            {assistant.suggestedPrompts.map((item) => (
              <button key={item} type="button" className="prompt-chip" onClick={() => setPrompt(item)}>
                {item}
              </button>
            ))}
          </div>
        </article>

        <article className="panel-card">
          <div className="panel-header">
            <div>
              <h3>Live insights</h3>
              <p>Data-backed context shipped by the backend.</p>
            </div>
          </div>
          <ul className="detail-list">
            {assistant.liveInsights.map((item) => (
              <li key={item}>{item}</li>
            ))}
          </ul>
        </article>
      </section>

      <section className="panel-card">
        <div className="panel-header">
          <div>
            <h3>{assistant.sidebarTitle} prompt</h3>
            <p>Submit a real case or question to the live API-backed assistant.</p>
          </div>
        </div>
        <form className="assistant-form" onSubmit={submitPrompt}>
          <label className="field">
            <span>{assistant.promptLabel}</span>
            <textarea
              rows={5}
              value={prompt}
              placeholder={assistant.promptPlaceholder}
              onChange={(event) => setPrompt(event.target.value)}
            />
          </label>
          {error ? <div className="form-error">{error}</div> : null}
          <button className="primary-button" type="submit" disabled={busy}>
            {busy ? "Generating..." : assistant.submitLabel}
          </button>
        </form>
      </section>

      {assistant.sections.length > 0 ? (
        <section className="panel-card">
          <div className="panel-header">
            <div>
              <h3>{assistant.responseTitle}</h3>
              <p>{assistant.responseSummary}</p>
            </div>
            <span className={`status-pill tone-${assistant.responseStatusTone}`}>{assistant.responseStatusLabel}</span>
          </div>

          {assistant.aiNarrative ? <pre className="ai-output">{assistant.aiNarrative}</pre> : null}

          {assistant.detectedSignals.length > 0 ? (
            <div className="chip-grid">
              {assistant.detectedSignals.map((signal) => (
                <span className="chip" key={signal}>
                  {signal}
                </span>
              ))}
            </div>
          ) : null}

          {assistant.responseFacts.length > 0 ? (
            <div className="fact-grid">
              {assistant.responseFacts.map((fact) => (
                <article key={fact.label} className="fact-card">
                  <span>{fact.label}</span>
                  <strong>{fact.value}</strong>
                </article>
              ))}
            </div>
          ) : null}

          <div className="section-grid">
            {assistant.sections.map((section) => (
              <article className="section-card" key={section.title}>
                <h4>{section.title}</h4>
                <ul className="detail-list">
                  {section.items.map((item) => (
                    <li key={item}>{item}</li>
                  ))}
                </ul>
              </article>
            ))}
          </div>
        </section>
      ) : null}

      <section className="callout">{assistant.disclaimer}</section>
    </div>
  );
}

function UnavailableAssistant() {
  return (
    <section className="panel-card">
      <h2>AI assistant unavailable for this role</h2>
      <p>This account currently has dashboard-only access in the React demo shell.</p>
    </section>
  );
}

function AboutPage() {
  return (
    <section className="panel-card page-stack">
      <div>
        <div className="eyebrow">Deployment checklist</div>
        <h2>How this demo is meant to run</h2>
      </div>
      <ul className="detail-list">
        <li>Backend runs in Docker on Render and exposes the .NET API at `/api`.</li>
        <li>Frontend runs on Vercel and points `VITE_API_BASE_URL` at the Render API base URL.</li>
        <li>The React app uses JWT auth, so it does not depend on Razor cookies or antiforgery tokens.</li>
        <li>The backend supports SQLite for demo deployment and SQL Server for richer local development.</li>
      </ul>
    </section>
  );
}

function clearSession(
  setToken: (value: string | null) => void,
  setBootstrap: (value: PortalBootstrap | null) => void,
) {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(USER_KEY);
  setBootstrap(null);
  setToken(null);
}
