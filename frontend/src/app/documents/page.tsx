"use client";

import Link from "next/link";
import { FormEvent, useEffect, useState } from "react";
import { AppShell } from "@/components/app-shell";
import { EmptyState } from "@/components/empty-state";
import { ErrorState } from "@/components/error-state";
import { LoadingState } from "@/components/loading-state";
import { StatusBadge } from "@/components/status-badge";
import { api, apiForm } from "@/lib/api";
import { getDemoSessionToken } from "@/lib/demo-session";
import { formatDate, formatDateTime } from "@/lib/format";
import type { DocumentRecord } from "@/lib/types";

export default function DocumentsPage() {
  const [documents, setDocuments] = useState<DocumentRecord[] | null>(null);
  const [title, setTitle] = useState("");
  const [file, setFile] = useState<File | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [uploadError, setUploadError] = useState<string | null>(null);
  const [uploading, setUploading] = useState(false);
  const [creatingSample, setCreatingSample] = useState(false);
  const [isPublicDemo] = useState(() => Boolean(getDemoSessionToken()));

  useEffect(() => {
    const controller = new AbortController();

    async function loadDocuments() {
      try {
        setError(null);
        setDocuments(
          await api<DocumentRecord[]>("/api/documents", {
            signal: controller.signal,
          }),
        );
      } catch (loadError) {
        if (!controller.signal.aborted) {
          setError(
            loadError instanceof Error
              ? loadError.message
              : "Documents could not be loaded.",
          );
        }
      }
    }

    void loadDocuments();
    return () => controller.abort();
  }, []);

  async function uploadDocument(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (isPublicDemo) {
      setUploadError("Public demo upload is disabled. Use sample documents instead.");
      return;
    }

    if (!file) {
      setUploadError("Choose a file before uploading.");
      return;
    }

    const formData = new FormData();
    formData.append("file", file);
    formData.append("title", title);

    try {
      setUploading(true);
      setUploadError(null);
      const uploaded = await apiForm<DocumentRecord>("/api/documents", formData);
      setDocuments((current) => [uploaded, ...(current ?? [])]);
      setTitle("");
      setFile(null);
      event.currentTarget.reset();
    } catch (uploadFailure) {
      setUploadError(
        uploadFailure instanceof Error
          ? uploadFailure.message
          : "Document could not be uploaded.",
      );
    } finally {
      setUploading(false);
    }
  }

  async function createSampleDocument() {
    try {
      setCreatingSample(true);
      setUploadError(null);
      const sample = await api<DocumentRecord>("/api/documents/sample", {
        method: "POST",
      });
      setDocuments((current) => [sample, ...(current ?? [])]);
    } catch (sampleFailure) {
      setUploadError(
        sampleFailure instanceof Error
          ? sampleFailure.message
          : "Sample document could not be created.",
      );
    } finally {
      setCreatingSample(false);
    }
  }

  return (
    <AppShell>
      <div className="mx-auto grid max-w-7xl gap-6 px-6 py-6 lg:grid-cols-[1fr_360px]">
        <section className="space-y-6">
          <div>
            <p className="text-sm font-medium text-[#64748b]">
              Document workflow
            </p>
            <h2 className="mt-2 text-3xl font-semibold">Documents</h2>
          </div>

          {error ? (
            <ErrorState message={error} />
          ) : !documents ? (
            <LoadingState label="Loading documents" />
          ) : documents.length === 0 ? (
            <EmptyState
              message="Upload a PDF, Office document, or image to start classification."
              title="No documents yet"
            />
          ) : (
            <section className="overflow-hidden rounded-md border border-[#d8deea] bg-white">
              <div className="divide-y divide-[#e2e8f0]">
                {documents.map((document) => (
                  <article
                    className="grid gap-4 p-5 md:grid-cols-[1fr_180px_140px_80px]"
                    key={document.id}
                  >
                    <div>
                      <div className="flex flex-wrap items-center gap-2">
                        <StatusBadge status={document.status} />
                        {document.documentType ? (
                          <span className="text-sm font-medium text-[#64748b]">
                            {document.documentType}
                          </span>
                        ) : null}
                      </div>
                      <Link
                        className="mt-2 block text-lg font-semibold text-[#162033] hover:text-[#2563eb]"
                        href={`/documents/${document.id}`}
                      >
                        {document.title}
                      </Link>
                      <p className="mt-2 text-sm text-[#64748b]">
                        Case: {document.caseId ?? "Not linked"}
                      </p>
                    </div>
                    <div className="text-sm text-[#475569]">
                      <p className="font-medium text-[#162033]">Expiry</p>
                      <p className="mt-1">{formatDate(document.expiryDate)}</p>
                    </div>
                    <div className="text-sm text-[#475569]">
                      <p className="font-medium text-[#162033]">Created</p>
                      <p className="mt-1">{formatDateTime(document.createdAt)}</p>
                    </div>
                    <div className="md:text-right">
                      <Link
                        className="text-sm font-semibold text-[#2563eb] hover:text-[#1d4ed8]"
                        href={`/documents/${document.id}`}
                      >
                        Open
                      </Link>
                    </div>
                  </article>
                ))}
              </div>
            </section>
          )}
        </section>

        <aside>
          <form
            className="space-y-5 rounded-md border border-[#d8deea] bg-white p-5"
            onSubmit={uploadDocument}
          >
            <div>
              <h3 className="text-lg font-semibold">Upload document</h3>
              <p className="mt-2 text-sm leading-6 text-[#64748b]">
                {isPublicDemo
                  ? "Public demo upload is disabled to prevent personal or confidential files from being submitted."
                  : "Allowed: PDF, PNG, JPG, JPEG. Maximum size is 5 MB."}
              </p>
            </div>
            {isPublicDemo ? (
              <div className="rounded-md border border-[#fde68a] bg-[#fffbeb] p-3 text-sm text-[#92400e]">
                Use the generated sample document for classification and delivery.
              </div>
            ) : null}
            {uploadError ? <ErrorState message={uploadError} /> : null}
            {isPublicDemo ? (
              <button
                className="rounded-md bg-[#047857] px-4 py-2 text-sm font-semibold text-white hover:bg-[#065f46] disabled:cursor-not-allowed disabled:opacity-60"
                disabled={creatingSample}
                onClick={createSampleDocument}
                type="button"
              >
                {creatingSample ? "Creating sample..." : "Use sample document"}
              </button>
            ) : null}
            <label className="block">
              <span className="mb-1 block text-sm font-semibold text-[#334155]">
                Title
              </span>
              <input
                className="w-full rounded-md border border-[#cbd5e1] px-3 py-2 text-sm focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#2563eb]"
                disabled={isPublicDemo}
                onChange={(event) => setTitle(event.target.value)}
                value={title}
              />
            </label>
            <label className="block">
              <span className="mb-1 block text-sm font-semibold text-[#334155]">
                File
              </span>
              <input
                accept=".pdf,.docx,.xlsx,.png,.jpg,.jpeg"
                className="w-full rounded-md border border-[#cbd5e1] px-3 py-2 text-sm file:mr-3 file:rounded-md file:border-0 file:bg-[#eef2ff] file:px-3 file:py-1.5 file:text-sm file:font-semibold file:text-[#3730a3] focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#2563eb]"
                disabled={isPublicDemo}
                onChange={(event) => setFile(event.target.files?.[0] ?? null)}
                required={!isPublicDemo}
                type="file"
              />
            </label>
            <button
              className="rounded-md bg-[#2563eb] px-4 py-2 text-sm font-semibold text-white hover:bg-[#1d4ed8] disabled:cursor-not-allowed disabled:opacity-60"
              disabled={uploading || isPublicDemo}
              type="submit"
            >
              {isPublicDemo ? "Upload disabled" : uploading ? "Uploading..." : "Upload document"}
            </button>
          </form>
        </aside>
      </div>
    </AppShell>
  );
}
