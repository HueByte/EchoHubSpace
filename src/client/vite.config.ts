import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],
  build: {
    rollupOptions: {
      output: {
        manualChunks: {
          "vendor-react": ["react", "react-dom", "react-router-dom"],
          "vendor-particles": ["@tsparticles/react", "@tsparticles/slim"],
          "vendor-signalr": ["@microsoft/signalr"],
        },
      },
    },
  },
  server: {
    proxy: {
      "/api": {
        target: "https://localhost:5001",
        changeOrigin: true,
        secure: false,
      },
      "/hubs": {
        target: "https://localhost:5001",
        changeOrigin: true,
        secure: false,
        ws: true,
      },
    },
  },
});
