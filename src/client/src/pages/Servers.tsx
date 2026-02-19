import { useEffect, useState, useRef, useCallback } from "react";
import {
  type Server,
  fetchServers,
  createServerHubConnection,
} from "../api/servers";
import type { HubConnection } from "@microsoft/signalr";
import { GoPeople } from "react-icons/go";
import styles from "./Servers.module.css";

export default function Servers() {
  const [servers, setServers] = useState<Server[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const connectionRef = useRef<HubConnection | null>(null);

  const upsertServer = useCallback((updated: Server) => {
    setServers((prev) => {
      const idx = prev.findIndex(
        (s) => s.host === updated.host && s.port === updated.port,
      );
      if (idx >= 0) {
        const next = [...prev];
        next[idx] = updated;
        return next;
      }
      return [...prev, updated];
    });
  }, []);

  const markOffline = useCallback(
    (info: { host: string; port: number }) => {
      setServers((prev) =>
        prev.map((s) =>
          s.host === info.host && s.port === info.port
            ? { ...s, isOnline: false, userCount: 0 }
            : s,
        ),
      );
    },
    [],
  );

  useEffect(() => {
    fetchServers()
      .then(setServers)
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));

    const connection = createServerHubConnection();
    connectionRef.current = connection;

    connection.on("ServerUpdated", upsertServer);
    connection.on("ServerOffline", markOffline);

    connection
      .start()
      .then(() => connection.invoke("JoinWebClients"))
      .catch(() => {
        /* hub not available yet, REST data still works */
      });

    return () => {
      connection.off("ServerUpdated");
      connection.off("ServerOffline");
      connection.stop();
    };
  }, [upsertServer, markOffline]);

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <h1 className={styles.title}>Servers</h1>
        <p className={styles.subtitle}>
          Available EchoHub servers you can connect to.
        </p>
      </div>

      <div className={styles.list}>
        {loading && (
          <div className={styles.empty}>Loading servers...</div>
        )}

        {error && (
          <div className={styles.empty}>
            Could not load servers. Make sure the API is running.
          </div>
        )}

        {!loading && !error && servers.length === 0 && (
          <div className={styles.empty}>No servers available yet.</div>
        )}

        {servers.map((server) => (
          <div key={server.id} className={styles.card}>
            <div className={styles.cardHeader}>
              <div className={styles.cardTitle}>
                <span
                  className={styles.status}
                  data-online={server.isOnline}
                />
                <span>{server.name}</span>
              </div>
              <span className={styles.users}>
                <GoPeople size={13} />
                {server.userCount}
              </span>
            </div>
            {server.description && (
              <p className={styles.description}>{server.description}</p>
            )}
            <div className={styles.cardFooter}>
              <span className={styles.host}>
                {server.host}:{server.port}
              </span>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
