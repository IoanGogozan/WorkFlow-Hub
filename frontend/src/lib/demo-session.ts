const demoSessionTokenKey = "norvix.demoSessionToken";
const demoSessionExpiresAtKey = "norvix.demoSessionExpiresAt";

export type DemoSession = {
  sessionId: string;
  demoTenantId: string;
  token: string;
  expiresAt: string;
};

export function getDemoSessionToken() {
  if (typeof window === "undefined") {
    return null;
  }

  try {
    return window.sessionStorage.getItem(demoSessionTokenKey);
  } catch {
    return null;
  }
}

export function saveDemoSession(session: DemoSession) {
  window.sessionStorage.setItem(demoSessionTokenKey, session.token);
  window.sessionStorage.setItem(demoSessionExpiresAtKey, session.expiresAt);
}

export function clearDemoSession() {
  if (typeof window === "undefined") {
    return;
  }

  try {
    window.sessionStorage.removeItem(demoSessionTokenKey);
    window.sessionStorage.removeItem(demoSessionExpiresAtKey);
  } catch {
    // Ignore browser storage failures. The next API call will still fail closed.
  }
}

export function getDemoSessionExpiresAt() {
  if (typeof window === "undefined") {
    return null;
  }

  try {
    return window.sessionStorage.getItem(demoSessionExpiresAtKey);
  } catch {
    return null;
  }
}

export function redirectToDemoStart(reason: "expired" | "missing" | "invalid") {
  if (typeof window === "undefined" || window.location.pathname === "/demo") {
    return;
  }

  window.location.assign(`/demo?reason=${reason}`);
}
