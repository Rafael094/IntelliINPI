import { redirect } from "next/navigation";

export default function DeadlinesDisabledLayout({ children: _children }: { children: React.ReactNode }) {
  redirect("/home");
}
