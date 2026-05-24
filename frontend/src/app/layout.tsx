import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "Norvix WorkFlow Hub",
  description: "Operational workflow hub for Norwegian B2B service teams.",
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
