import { Link } from "react-router-dom";
import { GoTerminal, GoCommentDiscussion, GoServer } from "react-icons/go";
import { FiGithub } from "react-icons/fi";
import styles from "./Home.module.css";

export default function Home() {
  return (
    <div className={styles.container}>
      <div className={styles.hero}>
        <img
          src="https://cdn.voidcube.cloud/assets/hue_icon.svg"
          alt="EchoHub logo"
          className={styles.heroLogo}
        />
        <h1 className={styles.title}>EchoHub</h1>
        <p className={styles.subtitle}>
          IRC-inspired chat, reimagined for the terminal.
        </p>
        <p className={styles.description}>
          Connect to servers, join channels, and chat — all from your CLI.
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
            Built for the command line. No browser needed.
          </p>
        </div>
        <div className={styles.feature}>
          <div className={styles.featureIcon}>
            <GoCommentDiscussion size={20} />
          </div>
          <h3 className={styles.featureTitle}>IRC Protocol</h3>
          <p className={styles.featureDesc}>
            Familiar IRC concepts — servers, channels, direct messages.
          </p>
        </div>
        <div className={styles.feature}>
          <div className={styles.featureIcon}>
            <GoServer size={20} />
          </div>
          <h3 className={styles.featureTitle}>Self-Hosted</h3>
          <p className={styles.featureDesc}>
            Run your own server. Own your conversations.
          </p>
        </div>
      </div>
    </div>
  );
}
