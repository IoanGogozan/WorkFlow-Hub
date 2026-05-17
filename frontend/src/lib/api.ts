import { localDevAuthHeaders } from "./dev-auth";
import {
  clearDemoSession,
  getDemoSessionToken,
  redirectToDemoStart,
} from "./demo-session";

type ApiOptions = {
  method?: string;
  body?: unknown;
  signal?: AbortSignal;
};

export async function api<T>(path: string, options: ApiOptions = {}): Promise<T> {
  const headers: Record<string, string> = {
    Accept: "application/json",
    ...authHeaders(),
  };

  if (options.body !== undefined) {
    headers["Content-Type"] = "application/json";
  }

  const response = await fetch(path, {
    method: options.method ?? "GET",
    headers,
    body: options.body === undefined ? undefined : JSON.stringify(options.body),
    signal: options.signal,
  });

  if (!response.ok) {
    const message = await getErrorMessage(response);
    handleAuthFailure(response.status, message);
    throw new Error(message);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return response.json() as Promise<T>;
}

export async function apiForm<T>(
  path: string,
  formData: FormData,
  options: Omit<ApiOptions, "body"> = {},
): Promise<T> {
  const response = await fetch(path, {
    method: options.method ?? "POST",
    headers: {
      Accept: "application/json",
      ...authHeaders(),
    },
    body: formData,
    signal: options.signal,
  });

  if (!response.ok) {
    const message = await getErrorMessage(response);
    handleAuthFailure(response.status, message);
    throw new Error(message);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return response.json() as Promise<T>;
}

function authHeaders(): Record<string, string> {
  const token = getDemoSessionToken();
  if (token) {
    return { Authorization: `Bearer ${token}` };
  }

  if (process.env.NODE_ENV === "development") {
    return { ...localDevAuthHeaders };
  }

  return {};
}

function handleAuthFailure(status: number, message: string) {
  if (status !== 401) {
    return;
  }

  if (message.includes("expired")) {
    clearDemoSession();
    redirectToDemoStart("expired");
    return;
  }

  if (message.includes("Invalid demo session token")) {
    clearDemoSession();
    redirectToDemoStart("invalid");
    return;
  }

  if (message.includes("Demo session bearer token is required")) {
    redirectToDemoStart("missing");
  }
}

async function getErrorMessage(response: Response) {
  const text = await response.text();
  if (!text) {
    return `API request failed with ${response.status}`;
  }

  try {
    const parsed = JSON.parse(text) as { error?: string; title?: string };
    return parsed.error ?? parsed.title ?? text;
  } catch {
    return text;
  }
}
