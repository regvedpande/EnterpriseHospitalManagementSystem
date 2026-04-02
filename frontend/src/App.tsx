import { FormEvent, startTransition, useEffect, useState } from "react";
import { NavLink, Navigate, Route, Routes, useLocation, useNavigate } from "react-router-dom";
import { api } from "./lib/api";
import type { AiAssistantPage, LoginResponse, PortalBootstrap } from "./types";

const TOKEN_KEY = "medcore.portal.token";
const USER_KEY = "medcore.portal.user";

const demoUsers = [
  { label: "Receptionist", email: "receptionist@hospital.com", password: "Receptionist@123" },
  { label: "Doctor", email: "doctor@hospital.com", password: "Doctor@123" },
  { label: "Pharmacist", email: "pharmacist@hospital.com", password: "Pharmacist@123" },
  { label: "Patient", email: "patient@hospital.com", password: "Patient@123" },
  { label: "Admin", email: "admin@hospital.com", password: "Admin@123" },
];

export default function App() {
  const [token, setToken] = useState<string | null>(() => localStorage.getItem(TOKEN_KEY));
  const [bootstrap, setBootstrap] = useState<PortalBootstrap | null>(null);
  const [loading, setLoading] = useState(Boolean(token));
  const [error, setError] = useState("");

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
        <section className="loading-card">
          <div className="eyebrow">MedCore HMS</div>
          <h1>Loading portal</h1>
          <p>{error || "Connecting to the live backend and preparing your workspace."}</p>
        </section>
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
        <div className="eyebrow">MedCore HMS</div>
        <h1>Professional hospital portal demo</h1>
        <p>
          A clean React frontend for role-based dashboards and AI workflows, connected directly to the live .NET API.
        </p>

        <div className="auth-note-list">
          <article className="auth-note">
            <strong>Role-aware views</strong>
            <span>Each user lands in the correct workspace with relevant metrics and actions.</span>
          </article>
          <article className="auth-note">
            <strong>Live backend data</strong>
            <span>Dashboard cards and assistant results are driven by the deployed API.</span>
          </article>
          <article className="auth-note">
            <strong>Fast demo flow</strong>
            <span>Choose a seeded account below and move straight into the product.</span>
          </article>
        </div>
      </section>

      <section className="auth-panel">
        <form className="auth-card" onSubmit={handleSubmit}>
          <div className="eyebrow">Portal access</div>
          <h2>Sign in</h2>
          <p className="support-copy">Use one of the demo accounts or type a seeded credential manually.</p>

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
  const [navOpen, setNavOpen] = useState(false);

  useEffect(() => {
    if (location.pathname === "/") navigate("/dashboard", { replace: true });
  }, [location.pathname, navigate]);

  useEffect(() => {
    setNavOpen(false);
  }, [location.pathname]);

  return (
    <div className="portal-shell">
      <header className="app-header">
        <div className="app-header-left">
          <button
            type="button"
            className="menu-button"
            aria-label="Toggle navigation"
            onClick={() => setNavOpen((value) => !value)}
          >
            <span />
            <span />
            <span />
          </button>

          <div className="brand-block">
            <div className="brand-mark">MC</div>
            <div>
              <div className="brand-name">MedCore HMS</div>
              <div className="brand-subtitle">{bootstrap.user.roleDisplay} workspace</div>
            </div>
          </div>
        </div>

        <div className="profile-block">
          <div className="profile-copy">
            <strong>{bootstrap.user.name}</strong>
            <span>{bootstrap.user.roleDisplay}</span>
          </div>
          <div className="avatar-react">{bootstrap.user.initials}</div>
          <button type="button" className="ghost-button" onClick={onLogout}>
            Sign out
          </button>
        </div>
      </header>

      <div className="portal-layout">
        <div className={`sidebar-backdrop${navOpen ? " open" : ""}`} onClick={() => setNavOpen(false)} />

        <aside className={`sidebar${navOpen ? " open" : ""}`}>
          <div className="sidebar-section-label">Navigation</div>
          <nav className="nav-stack">
            {bootstrap.navigation.map((item) => (
              <NavLink
                key={item.key}
                to={item.route}
                className={({ isActive }) => `nav-item${isActive ? " active" : ""}`}
              >
                <span>{item.label}</span>
              </NavLink>
            ))}
          </nav>

          <div className="sidebar-meta">
            <div className="sidebar-section-label">Assistant status</div>
            <div className="provider-pill">{bootstrap.aiAssistant?.providerStatusLabel || "Dashboard ready"}</div>
          </div>
        </aside>

        <main className="portal-main">
          <header className="page-header">
            <div>
              <div className="eyebrow">Live backend session</div>
              <h1>{bootstrap.dashboard.title}</h1>
              <p>{bootstrap.dashboard.subtitle}</p>
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
    </div>
  );
}

function DashboardPage({ dashboard }: { dashboard: PortalBootstrap["dashboard"] }) {
  return (
    <div className="page-stack">
      <section className="overview-panel">
        <div className="overview-copy">
          <div className="eyebrow">Overview</div>
          <h2>{dashboard.title}</h2>
          <p>{dashboard.subtitle}</p>
        </div>
      </section>

      <section className="metric-grid">
        {dashboard.metrics.map((metric) => (
          <article key={metric.label} className="metric-card">
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
            <p>Latest backend records for the signed-in role.</p>
          </div>
        </div>

        <div className="activity-list">
          {dashboard.recentItems.map((item, index) => (
            <article key={`${item.title}-${index}`} className="activity-item">
              <div className="activity-primary">
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
      <section className="overview-panel">
        <div className="overview-copy">
          <div className="eyebrow">{assistant.providerStatusLabel}</div>
          <h2>{assistant.pageTitle}</h2>
          <p>{assistant.subtitle}</p>
        </div>

        <div className="overview-aside">
          <span className={`status-pill tone-${assistant.responseStatusTone}`}>{assistant.responseStatusLabel}</span>
          <p>{assistant.providerNote}</p>
        </div>
      </section>

      <section className="metric-grid">
        {assistant.metrics.map((metric) => (
          <article key={metric.label} className="metric-card">
            <div className="metric-label">{metric.label}</div>
            <div className="metric-value">{metric.value}</div>
            {metric.description ? <div className="metric-note">{metric.description}</div> : null}
          </article>
        ))}
      </section>

      <section className="assistant-layout">
        <article className="panel-card">
          <div className="panel-header">
            <div>
              <h3>Ask the assistant</h3>
              <p>Submit a case, medication question, or workflow request.</p>
            </div>
          </div>

          <form className="assistant-form" onSubmit={submitPrompt}>
            <label className="field">
              <span>{assistant.promptLabel}</span>
              <textarea
                rows={6}
                value={prompt}
                placeholder={assistant.promptPlaceholder}
                onChange={(event) => setPrompt(event.target.value)}
              />
            </label>

            <div className="prompt-list">
              {assistant.suggestedPrompts.map((item) => (
                <button key={item} type="button" className="prompt-chip" onClick={() => setPrompt(item)}>
                  {item}
                </button>
              ))}
            </div>

            {error ? <div className="form-error">{error}</div> : null}

            <button className="primary-button" type="submit" disabled={busy}>
              {busy ? "Generating..." : assistant.submitLabel}
            </button>
          </form>
        </article>

        <article className="panel-card">
          <div className="panel-header">
            <div>
              <h3>Role coverage</h3>
              <p>What this assistant is currently configured to help with.</p>
            </div>
          </div>

          <div className="chip-grid">
            {assistant.capabilities.map((capability) => (
              <span className="chip" key={capability}>
                {capability}
              </span>
            ))}
          </div>

          {assistant.liveInsights.length > 0 ? (
            <div className="insight-block">
              <h4>Live insights</h4>
              <ul className="detail-list">
                {assistant.liveInsights.map((item) => (
                  <li key={item}>{item}</li>
                ))}
              </ul>
            </div>
          ) : null}
        </article>
      </section>

      {assistant.sections.length > 0 ? (
        <section className="panel-card">
          <div className="panel-header">
            <div>
              <h3>{assistant.responseTitle}</h3>
              <p>{assistant.responseSummary}</p>
            </div>
          </div>

          {assistant.detectedSignals.length > 0 ? (
            <div className="chip-grid compact">
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

          {assistant.aiNarrative ? (
            <div className="narrative-block">
              <h4>Live AI output</h4>
              <pre className="ai-output">{assistant.aiNarrative}</pre>
            </div>
          ) : null}
        </section>
      ) : null}

      <section className="callout">{assistant.disclaimer}</section>
    </div>
  );
}

function UnavailableAssistant() {
  return (
    <section className="panel-card">
      <div className="eyebrow">Assistant unavailable</div>
      <h2>AI access is not enabled for this role in the React portal</h2>
      <p>This account currently has dashboard-only access in the demo shell.</p>
    </section>
  );
}

function AboutPage() {
  return (
    <div className="page-stack">
      <section className="overview-panel">
        <div className="overview-copy">
          <div className="eyebrow">Deployment</div>
          <h2>How this demo is wired</h2>
          <p>The React frontend runs separately from the .NET backend so the demo is easy to host and share.</p>
        </div>
      </section>

      <section className="section-grid">
        <article className="section-card">
          <h4>Backend</h4>
          <ul className="detail-list">
            <li>Render hosts the ASP.NET Core backend in Docker.</li>
            <li>The API is exposed under <code>/api</code>.</li>
            <li>SQLite is supported for lightweight demo deployments.</li>
          </ul>
        </article>
        <article className="section-card">
          <h4>Frontend</h4>
          <ul className="detail-list">
            <li>Vercel hosts the Vite React app from the <code>frontend</code> folder.</li>
            <li>The UI authenticates with JWT and talks to the backend directly.</li>
            <li>CORS is configured in the backend for approved frontend origins.</li>
          </ul>
        </article>
        <article className="section-card">
          <h4>Demo flow</h4>
          <ul className="detail-list">
            <li>Sign in with a seeded role account.</li>
            <li>Review the dashboard.</li>
            <li>Open the assistant and submit a prompt.</li>
          </ul>
        </article>
      </section>
    </div>
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
