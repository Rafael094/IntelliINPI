import axios from "axios";

const tokenStorageKey = "intelliinpi.accessToken";
let onUnauthorized: (() => void) | null = null;

export const api = axios.create({
  baseURL: ""
});

api.interceptors.request.use((config) => {
  if (typeof window === "undefined") {
    return config;
  }

  const token = window.localStorage.getItem(tokenStorageKey);
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401 && typeof window !== "undefined") {
      window.localStorage.removeItem(tokenStorageKey);
      onUnauthorized?.();
    }

    return Promise.reject(error);
  }
);

export function setUnauthorizedHandler(handler: (() => void) | null) {
  onUnauthorized = handler;
}

export { tokenStorageKey };
