import { localDevAuthHeaders } from "./dev-auth";

type ApiOptions = {
  method?: string;
  body?: unknown;
  signal?: AbortSignal;
};

export async function api<T>(path: string, options: ApiOptions = {}): Promise<T> {
  const headers: HeadersInit = {
    Accept: "application/json",
    ...localDevAuthHeaders,
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
    throw new Error(await getErrorMessage(response));
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
      ...localDevAuthHeaders,
    },
    body: formData,
    signal: options.signal,
  });

  if (!response.ok) {
    throw new Error(await getErrorMessage(response));
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return response.json() as Promise<T>;
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
