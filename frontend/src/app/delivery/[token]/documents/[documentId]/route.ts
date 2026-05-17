type PublicDeliveryDocumentRouteContext = {
  params: Promise<{ token: string; documentId: string }>;
};

export async function GET(
  _request: Request,
  { params }: PublicDeliveryDocumentRouteContext,
) {
  const { token, documentId } = await params;
  const backendUrl =
    process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5000";
  const response = await fetch(
    `${backendUrl}/delivery/${token}/documents/${documentId}`,
    { cache: "no-store" },
  );

  if (!response.ok) {
    return new Response(null, { status: response.status });
  }

  const headers = new Headers();
  copyHeader(response.headers, headers, "content-type");
  copyHeader(response.headers, headers, "content-disposition");
  copyHeader(response.headers, headers, "content-length");
  copyHeader(response.headers, headers, "accept-ranges");

  return new Response(response.body, {
    status: response.status,
    headers,
  });
}

function copyHeader(source: Headers, target: Headers, name: string) {
  const value = source.get(name);
  if (value) {
    target.set(name, value);
  }
}
