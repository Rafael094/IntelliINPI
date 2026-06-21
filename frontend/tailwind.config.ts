import type { Config } from "tailwindcss";

const config: Config = {
  content: [
    "./src/pages/**/*.{js,ts,jsx,tsx,mdx}",
    "./src/components/**/*.{js,ts,jsx,tsx,mdx}",
    "./src/app/**/*.{js,ts,jsx,tsx,mdx}"
  ],
  theme: {
    extend: {
      colors: {
        ink: "#17212b",
        line: "#d8dee8",
        brand: "#0f766e",
        accent: "#b45309"
      },
      boxShadow: {
        panel: "0 1px 2px rgba(23, 33, 43, 0.08)"
      }
    }
  },
  plugins: []
};

export default config;
