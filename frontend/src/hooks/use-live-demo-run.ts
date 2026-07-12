"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import {
  createLiveDemoRun,
  getLiveDemoCapabilities,
  getLiveDemoRun,
  retryLiveDemoRun,
  type LiveDemoCapabilities,
  type LiveDemoRun,
} from "@/lib/live-demo";

const pollIntervalMilliseconds = 800;

export function useLiveDemoRun() {
  const [capabilities, setCapabilities] = useState<LiveDemoCapabilities | null>(null);
  const [run, setRun] = useState<LiveDemoRun | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isStarting, setIsStarting] = useState(false);
  const abortControllerRef = useRef<AbortController | null>(null);

  const isActive = run?.status === "Queued" || run?.status === "Running";
  const loadRun = useCallback(async (runId: string, signal?: AbortSignal) => {
    const nextRun = await getLiveDemoRun(runId, signal);
    setRun(nextRun);
    return nextRun;
  }, []);

  useEffect(() => {
    const controller = new AbortController();
    getLiveDemoCapabilities(controller.signal)
      .then(setCapabilities)
      .catch((loadError: unknown) => {
        if (!(loadError instanceof DOMException && loadError.name === "AbortError")) {
          setError(toPublicError(loadError));
        }
      });
    return () => controller.abort();
  }, []);

  useEffect(() => {
    if (!run || !isActive) {
      return;
    }

    const controller = new AbortController();
    abortControllerRef.current?.abort();
    abortControllerRef.current = controller;
    let timeoutId: ReturnType<typeof setTimeout> | undefined;

    const poll = async () => {
      try {
        const nextRun = await loadRun(run.runId, controller.signal);
        if (nextRun.status === "Queued" || nextRun.status === "Running") {
          timeoutId = setTimeout(poll, pollIntervalMilliseconds);
        }
      } catch (pollError: unknown) {
        if (!(pollError instanceof DOMException && pollError.name === "AbortError")) {
          setError(toPublicError(pollError));
        }
      }
    };

    void poll();
    return () => {
      controller.abort();
      if (timeoutId) {
        clearTimeout(timeoutId);
      }
    };
  }, [isActive, loadRun, run]);

  const start = useCallback(async () => {
    if (isStarting || isActive || capabilities?.enabled === false) {
      return;
    }

    setError(null);
    setIsStarting(true);
    try {
      const created = await createLiveDemoRun();
      await loadRun(created.runId);
    } catch (startError: unknown) {
      setError(toPublicError(startError));
    } finally {
      setIsStarting(false);
    }
  }, [capabilities?.enabled, isActive, isStarting, loadRun]);

  const retry = useCallback(async () => {
    if (!run?.canRetry || isStarting) {
      return;
    }

    setError(null);
    setIsStarting(true);
    try {
      const retried = await retryLiveDemoRun(run.runId);
      await loadRun(retried.runId);
    } catch (retryError: unknown) {
      setError(toPublicError(retryError));
    } finally {
      setIsStarting(false);
    }
  }, [isStarting, loadRun, run]);

  return {
    capabilities,
    error,
    isActive,
    isStarting,
    retry,
    run,
    start,
  };
}

function toPublicError(error: unknown) {
  return error instanceof Error ? error.message : "Live-demoen kunne ikke fullføres.";
}
