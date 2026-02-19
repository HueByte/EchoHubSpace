import { Link, useLocation } from "react-router-dom";
import { GoHome, GoServer } from "react-icons/go";
import styles from "./Navbar.module.css";

export default function Navbar() {
  const location = useLocation();

  return (
    <nav className={styles.nav}>
      <Link to="/" className={styles.logo}>
        <img
          src="https://cdn.voidcube.cloud/assets/hue_icon.svg"
          alt="EchoHub"
          className={styles.logoIcon}
        />
        <span>EchoHub</span>
      </Link>
      <div className={styles.links}>
        <Link
          to="/"
          className={`${styles.link} ${location.pathname === "/" ? styles.active : ""}`}
        >
          <GoHome size={16} />
          <span>Home</span>
        </Link>
        <Link
          to="/servers"
          className={`${styles.link} ${location.pathname === "/servers" ? styles.active : ""}`}
        >
          <GoServer size={16} />
          <span>Servers</span>
        </Link>
      </div>
    </nav>
  );
}
