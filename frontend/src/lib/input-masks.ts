function digits(value: string, length: number) {
  return value.replace(/\D/g, "").slice(0, length);
}

export function maskCpf(value: string) {
  const number = digits(value, 11);
  return number
    .replace(/^(\d{3})(\d)/, "$1.$2")
    .replace(/^(\d{3})\.(\d{3})(\d)/, "$1.$2.$3")
    .replace(/\.(\d{3})(\d)/, ".$1-$2");
}

export function maskCnpj(value: string) {
  const number = digits(value, 14);
  return number
    .replace(/^(\d{2})(\d)/, "$1.$2")
    .replace(/^(\d{2})\.(\d{3})(\d)/, "$1.$2.$3")
    .replace(/\.(\d{3})(\d)/, ".$1/$2")
    .replace(/(\/\d{4})(\d)/, "$1-$2");
}

export function maskPhone(value: string) {
  const number = digits(value, 11);
  if (number.length <= 10) {
    return number
      .replace(/^(\d{2})(\d)/, "($1) $2")
      .replace(/(\d{4})(\d)/, "$1-$2");
  }

  return number
    .replace(/^(\d{2})(\d)/, "($1) $2")
    .replace(/(\d{5})(\d)/, "$1-$2");
}

export function maskOrcid(value: string) {
  const normalized = value.toUpperCase().replace(/[^\dX]/g, "").slice(0, 16);
  return normalized.replace(/(.{4})(?=.)/g, "$1-");
}
