"use client";

import { useRouter } from "next/navigation";
import { FormEvent, useState } from "react";
import { AppShell } from "@/components/app-shell";
import { ErrorState } from "@/components/error-state";
import { api } from "@/lib/api";
import type { IntakeItem } from "@/lib/types";

const initialForm = {
  source: "Manual",
  subject: "",
  body: "",
  customerName: "",
  organizationNumber: "",
  category: "",
  urgency: "",
};

export default function NewIntakePage() {
  const router = useRouter();
  const [form, setForm] = useState(initialForm);
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    try {
      setSubmitting(true);
      setError(null);
      const intake = await api<IntakeItem>("/api/intakes", {
        method: "POST",
        body: form,
      });
      router.push(`/intakes/${intake.id}`);
    } catch (submitError) {
      setError(
        submitError instanceof Error
          ? submitError.message
          : "Intake could not be created.",
      );
    } finally {
      setSubmitting(false);
    }
  }

  function updateField(name: keyof typeof form, value: string) {
    setForm((current) => ({ ...current, [name]: value }));
  }

  return (
    <AppShell>
      <div className="mx-auto max-w-3xl px-6 py-6">
        <div className="mb-6">
          <p className="text-sm font-medium text-[#64748b]">New intake</p>
          <h2 className="mt-2 text-3xl font-semibold">Create request</h2>
        </div>

        {error ? <div className="mb-5"><ErrorState message={error} /></div> : null}

        <form
          className="space-y-5 rounded-md border border-[#d8deea] bg-white p-6"
          onSubmit={submit}
        >
          <Field label="Source">
            <select
              className="w-full rounded-md border border-[#cbd5e1] px-3 py-2 text-sm focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#2563eb]"
              name="source"
              onChange={(event) => updateField("source", event.target.value)}
              value={form.source}
            >
              <option value="Manual">Manual</option>
              <option value="MockEmail">Mock email</option>
              <option value="MockForm">Mock form</option>
              <option value="Api">API</option>
            </select>
          </Field>

          <Field label="Subject">
            <input
              className="w-full rounded-md border border-[#cbd5e1] px-3 py-2 text-sm focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#2563eb]"
              maxLength={240}
              onChange={(event) => updateField("subject", event.target.value)}
              required
              value={form.subject}
            />
          </Field>

          <Field label="Body">
            <textarea
              className="min-h-40 w-full rounded-md border border-[#cbd5e1] px-3 py-2 text-sm focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#2563eb]"
              maxLength={8000}
              onChange={(event) => updateField("body", event.target.value)}
              required
              value={form.body}
            />
          </Field>

          <div className="grid gap-4 sm:grid-cols-2">
            <Field label="Customer name">
              <input
                className="w-full rounded-md border border-[#cbd5e1] px-3 py-2 text-sm focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#2563eb]"
                onChange={(event) => updateField("customerName", event.target.value)}
                value={form.customerName}
              />
            </Field>
            <Field label="Organization number">
              <input
                className="w-full rounded-md border border-[#cbd5e1] px-3 py-2 text-sm focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#2563eb]"
                onChange={(event) =>
                  updateField("organizationNumber", event.target.value)
                }
                value={form.organizationNumber}
              />
            </Field>
            <Field label="Category">
              <input
                className="w-full rounded-md border border-[#cbd5e1] px-3 py-2 text-sm focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#2563eb]"
                onChange={(event) => updateField("category", event.target.value)}
                value={form.category}
              />
            </Field>
            <Field label="Urgency">
              <select
                className="w-full rounded-md border border-[#cbd5e1] px-3 py-2 text-sm focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#2563eb]"
                onChange={(event) => updateField("urgency", event.target.value)}
                value={form.urgency}
              >
                <option value="">Not set</option>
                <option value="Low">Low</option>
                <option value="Normal">Normal</option>
                <option value="High">High</option>
                <option value="Urgent">Urgent</option>
              </select>
            </Field>
          </div>

          <button
            className="rounded-md bg-[#2563eb] px-4 py-2 text-sm font-semibold text-white hover:bg-[#1d4ed8] disabled:cursor-not-allowed disabled:opacity-60"
            disabled={submitting}
            type="submit"
          >
            {submitting ? "Creating..." : "Create intake"}
          </button>
        </form>
      </div>
    </AppShell>
  );
}

function Field({
  label,
  children,
}: {
  label: string;
  children: React.ReactNode;
}) {
  return (
    <label className="block">
      <span className="mb-1 block text-sm font-semibold text-[#334155]">
        {label}
      </span>
      {children}
    </label>
  );
}
