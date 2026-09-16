import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import path from "node:path";

// Configuração do Vite: alias "@" -> "src" (usado em todo o projeto, ver
// tsconfig.json) e proxy de /api para a Api em desenvolvimento, evitando
// problemas de CORS no ambiente local sem precisar apontar a variável de
// ambiente VITE_API_URL para a origem completa.
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src")
    }
  },
  server: {
    port: 5173,
    proxy: {
      "/api": {
        target: "http://localhost:8080",
        changeOrigin: true
      }
    }
  }
});