import { useEffect, useState, useRef, useCallback } from "react";
import {
  type Server,
  fetchServers,
  createServerHubConnection,
} from "../api/servers";
import type { HubConnection } from "@microsoft/signalr";
import { GoPeople, GoChevronDown } from "react-icons/go";
import { tagColor } from "../utils/tagColor";
import styles from "./Servers.module.css";

export default function Servers() {
  const [servers, setServers] = useState<Server[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [expandedIds, setExpandedIds] = useState<Set<string>>(new Set());
  const connectionRef = useRef<HubConnection | null>(null);

  const upsertServer = useCallback((updated: Server) => {
    setServers((prev) => {
      const idx = prev.findIndex((s) => s.id === updated.id);
      if (idx >= 0) {
        const next = [...prev];
        next[idx] = updated;
        return next;
      }
      return [...prev, updated];
    });
  }, []);

  const markOffline = useCallback((info: { id: string }) => {
    setServers((prev) =>
      prev.map((s) =>
        s.id === info.id ? { ...s, isOnline: false, userCount: 0 } : s,
      ),
    );
  }, []);

  const toggleExpanded = useCallback((id: string) => {
    setExpandedIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  }, []);

  useEffect(() => {
    fetchServers()
      .then(setServers)
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));

    const connection = createServerHubConnection();
    connectionRef.current = connection;

    connection.on("ServerUpdated", upsertServer);
    connection.on("ServerOffline", markOffline);

    connection.onreconnected(async () => {
      try {
        await connection.invoke("JoinWebClients");
        const fresh = await fetchServers();
        setServers(fresh);
      } catch {
        /* best-effort refresh */
      }
    });

    let retryTimeout: ReturnType<typeof setTimeout>;
    connection.onclose(() => {
      const retry = () => {
        connection
          .start()
          .then(() => connection.invoke("JoinWebClients"))
          .then(() => fetchServers())
          .then(setServers)
          .catch(() => {
            retryTimeout = setTimeout(retry, 30_000);
          });
      };
      retryTimeout = setTimeout(retry, 5_000);
    });

    connection
      .start()
      .then(() => connection.invoke("JoinWebClients"))
      .catch(() => {
        /* hub not available yet, REST data still works */
      });

    return () => {
      clearTimeout(retryTimeout);
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

        {servers.map((server) => {
          const [primaryHost, ...extraHosts] = server.hosts ?? [];
          const hasExtra = extraHosts.length > 0;
          const isExpanded = expandedIds.has(server.id);

          return (
            <div
              key={server.id}
              className={styles.card}
              data-expandable={hasExtra}
              data-expanded={isExpanded}
              onClick={hasExtra ? () => toggleExpanded(server.id) : undefined}
              role={hasExtra ? "button" : undefined}
              tabIndex={hasExtra ? 0 : undefined}
              onKeyDown={
                hasExtra
                  ? (e) => {
                      if (e.key === "Enter" || e.key === " ") {
                        e.preventDefault();
                        toggleExpanded(server.id);
                      }
                    }
                  : undefined
              }
            >
              <div className={styles.cardHeader}>
                <div className={styles.cardTitle}>
                  <span
                    className={styles.status}
                    data-online={server.isOnline}
                  />
                  <span className={styles.name} title={server.name}>
                    {server.name}
                  </span>
                </div>
                <span className={styles.users}>
                  <GoPeople size={13} />
                  {server.userCount}
                </span>
              </div>

              {server.description && (
                <p className={styles.description}>{server.description}</p>
              )}

              {server.tags && server.tags.length > 0 && (
                <div className={styles.tags}>
                  {server.tags.map((tag) => {
                    const c = tagColor(tag);
                    return (
                      <span
                        key={tag}
                        className={styles.tag}
                        title={tag}
                        style={{
                          background: c.bg,
                          borderColor: c.border,
                          color: c.text,
                        }}
                      >
                        {tag}
                      </span>
                    );
                  })}
                </div>
              )}

              <div className={styles.cardFooter}>
                {primaryHost && (
                  <span className={styles.host} title={primaryHost}>
                    {primaryHost}
                  </span>
                )}
                {hasExtra && (
                  <GoChevronDown
                    className={styles.expandIcon}
                    size={14}
                    aria-hidden
                  />
                )}
              </div>

              {isExpanded && hasExtra && (
                <div className={styles.expandedSection}>
                  <div className={styles.expandedLabel}>Other hosts</div>
                  <ul className={styles.hostList}>
                    {extraHosts.map((h) => (
                      <li key={h} className={styles.host} title={h}>
                        {h}
                      </li>
                    ))}
                  </ul>
                </div>
              )}
            </div>
          );
        })}
      </div>
    </div>
  );
}
