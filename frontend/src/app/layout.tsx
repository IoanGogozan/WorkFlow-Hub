import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  metadataBase: new URL("https://workflow.norvix.no"),
  title: {
    default: "Norvix WorkFlow Hub",
    template: "%s | Norvix WorkFlow Hub",
  },
  description:
    "A verifiable portfolio demo for workflow automation and system integration in technical service companies.",
  openGraph: {
    title: "Norvix WorkFlow Hub",
    description:
      "One fictional service request, coordinated across cases, documents, downstream systems, and inspectable evidence.",
    type: "website",
    url: "/",
  },
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en" className="h-full antialiased" suppressHydrationWarning>
      <body className="flex min-h-full flex-col bg-[#f5f7fb] text-[#162033]">
        {children}
      </body>
    </html>
  );
}
