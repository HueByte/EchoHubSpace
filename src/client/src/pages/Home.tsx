import { Link } from "react-router-dom";
import { GoTerminal, GoCommentDiscussion, GoServer } from "react-icons/go";
import { FiGithub } from "react-icons/fi";
import EchoHubLogo from "../assets/EchoHubLogo";
import styles from "./Home.module.css";

export default function Home() {
  return (
    <div className={styles.container}>
      <div className={styles.hero}>
        <EchoHubLogo className={styles.heroLogo} />
        <h1 className={styles.title}>EchoHub</h1>
        <p className={styles.subtitle}>
          No tracking. No subscriptions. No 'enhanced AI features'. Just chat.
        </p>
        <p className={styles.description}>
          Connect to servers, join channels, and chat — from TUI or your
          favorite IRC client.
        </p>
        <div className={styles.actions}>
          <Link to="/servers" className={styles.primaryBtn}>
            Browse Servers
          </Link>
          <a
            href="https://github.com/HueByte/EchoHub"
            target="_blank"
            rel="noopener noreferrer"
            className={styles.secondaryBtn}
          >
            <FiGithub size={14} />
            <span>Get Started</span>
          </a>
        </div>
      </div>

      <div className={styles.features}>
        <div className={styles.feature}>
          <div className={styles.featureIcon}>
            <GoTerminal size={20} />
          </div>
          <h3 className={styles.featureTitle}>Terminal Native</h3>
          <p className={styles.featureDesc}>
            Built for the command line. No browser, no Electron — just your
            terminal.
          </p>
        </div>
        <div className={styles.feature}>
          <div className={styles.featureIcon}>
            <GoCommentDiscussion size={20} />
          </div>
          <h3 className={styles.featureTitle}>Native IRC Support</h3>
          <p className={styles.featureDesc}>
            Connect with irssi, WeeChat, or any IRC client alongside TUI users.
          </p>
        </div>
        <div className={styles.feature}>
          <div className={styles.featureIcon}>
            <GoServer size={20} />
          </div>
          <h3 className={styles.featureTitle}>Self-Hosted</h3>
          <p className={styles.featureDesc}>
            Run your own server with E2E encryption. No tracking, no third
            parties — own your conversations.
          </p>
        </div>
      </div>
    </div>
  );
}
