"use client";

import { usePathname, useRouter } from "next/navigation";
import { useEffect } from "react";
import { useAuth } from "@/lib/auth";

export function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const pathname = usePathname();
  const { token, isReady } = useAuth();

  useEffect(() => {
    if (isReady && !token && pathname !== "/login") {
      router.replace("/login");
    }
  }, [isReady, pathname, router, token]);

  if (!isReady) {
    return <div className="flex min-h-screen items-center justify-center text-sm text-slate-600">Carregando...</div>;
  }

  if (!token) {
    return null;
  }

  return children;
}
