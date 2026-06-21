import { AxiosError } from "axios";

type ApiErrorBody = {
  message?: string;
  error?: string;
  title?: string;
  detail?: string;
  errors?: Record<string, string[]> | string[];
};

export function getApiErrorMessage(error: unknown, fallback: string) {
  const axiosError = error as AxiosError<ApiErrorBody>;
  const body = axiosError.response?.data;

  if (!body) {
    return fallback;
  }

  if (Array.isArray(body.errors)) {
    return body.errors.join(" ");
  }

  if (body.errors) {
    return Object.values(body.errors).flat().join(" ");
  }

  if (body.message) return body.message;
  if (body.error) return body.error;
  if (body.detail) return body.detail;
  if (body.title) return translateKnownError(body.title);

  return fallback;
}

function translateKnownError(message: string) {
  if (message === "One or more validation errors occurred.") {
    return "Um ou mais campos estão inválidos.";
  }

  return message;
}
