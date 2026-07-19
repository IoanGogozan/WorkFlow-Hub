import { redirect } from "next/navigation";

export default function LivePreviewRedirectPage() {
  redirect("/demo/run");
}
