import * as signalR from "@microsoft/signalr";

export interface Server {
  id: string;
  name: string;
  description: string | null;
  host: string;
  port: number;
  userCount: number;
  isOnline: boolean;
  createdAt: string;
}

const API_BASE = import.meta.env.VITE_API_URL ?? "";

export async function fetchServers(): Promise<Server[]> {
  const res = await fetch(`${API_BASE}/api/servers`);
  if (!res.ok) throw new Error("Failed to fetch servers");
  return res.json();
}

export function createServerHubConnection(): signalR.HubConnection {
  return new signalR.HubConnectionBuilder()
    .withUrl(`${API_BASE}/hubs/servers`)
    .withAutomaticReconnect()
    .build();
}
