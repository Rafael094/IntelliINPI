"use client";

import { useRouter } from "next/navigation";
import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import { api, setUnauthorizedHandler, tokenStorageKey } from "@/lib/api";
import type { LoginResponse, User } from "@/lib/types";

type StoredAuth = {
  token: string;
  user: User;
};

type AuthContextValue = {
  token: string | null;
  user: User | null;
  isReady: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
};

const userStorageKey = "intelliinpi.user";
const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const [auth, setAuth] = useState<StoredAuth | null>(null);
  const [isReady, setIsReady] = useState(false);

  const logout = useCallback(() => {
    window.localStorage.removeItem(tokenStorageKey);
    window.localStorage.removeItem(userStorageKey);
    setAuth(null);
    router.replace("/login");
  }, [router]);

  useEffect(() => {
    const token = window.localStorage.getItem(tokenStorageKey);
    const userJson = window.localStorage.getItem(userStorageKey);

    if (token && userJson) {
      try {
        setAuth({ token, user: JSON.parse(userJson) as User });
      } catch {
        window.localStorage.removeItem(tokenStorageKey);
        window.localStorage.removeItem(userStorageKey);
      }
    }

    setUnauthorizedHandler(logout);
    setIsReady(true);
    return () => setUnauthorizedHandler(null);
  }, [logout]);

  const login = useCallback(
    async (email: string, password: string) => {
      const response = await api.post<LoginResponse>("/api/auth/login", { email, password });
      window.localStorage.setItem(tokenStorageKey, response.data.accessToken);
      window.localStorage.setItem(userStorageKey, JSON.stringify(response.data.user));
      setAuth({ token: response.data.accessToken, user: response.data.user });
      router.replace("/dashboard");
    },
    [router]
  );

  const value = useMemo<AuthContextValue>(
    () => ({
      token: auth?.token ?? null,
      user: auth?.user ?? null,
      isReady,
      login,
      logout
    }),
    [auth, isReady, login, logout]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const value = useContext(AuthContext);
  if (!value) {
    throw new Error("useAuth must be used inside AuthProvider.");
  }

  return value;
}
