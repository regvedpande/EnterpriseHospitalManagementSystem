import type { AiAssistantPage, LoginResponse, PortalBootstrap } from "../types";

const baseUrl = (import.meta.env.VITE_API_BASE_URL || "/api").replace(/\/$/, "");

function buildUrl(path: string) {
  return `${baseUrl}${path.startsWith("/") ? path : `/${path}`}`;
}

async function request<T>(path: string, options: RequestInit = {}, token?: string): Promise<T> {
  const response = await fetch(buildUrl(path), {
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(options.headers || {}),
    },
  });

  if (!response.ok) {
    const body = await response.text();
    throw new Error(body || `Request failed with ${response.status}`);
  }

  return (await response.json()) as T;
}

export const api = {
  login(email: string, password: string) {
    return request<LoginResponse>("/ApiAuth/login", {
      method: "POST",
      body: JSON.stringify({ email, password }),
    });
  },
  me(token: string) {
    return request("/ApiAuth/me", { method: "GET" }, token);
  },
  bootstrap(token: string) {
    return request<PortalBootstrap>("/portal/bootstrap", { method: "GET" }, token);
  },
  askAssistant(token: string, prompt: string) {
    return request<AiAssistantPage>(
      "/portal/assistant",
      {
        method: "POST",
        body: JSON.stringify({ prompt }),
      },
      token,
    );
  },
};
