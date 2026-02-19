import { useState, useCallback } from "react";
import { Link, useLocation } from "react-router-dom";
import { GoHome, GoServer } from "react-icons/go";
import { HiOutlineBars3, HiOutlineXMark } from "react-icons/hi2";
import styles from "./SideMenu.module.css";

export default function SideMenu() {
  const [open, setOpen] = useState(false);
  const location = useLocation();

  const openMenu = useCallback(() => setOpen(true), []);
  const close = useCallback(() => setOpen(false), []);

  return (
    <>
      <button
        className={styles.burger}
        onClick={openMenu}
        aria-label="Open menu"
      >
        <HiOutlineBars3 size={22} />
      </button>

      <div
        className={`${styles.backdrop} ${open ? styles.backdropVisible : ""}`}
        onClick={close}
      />

      <nav className={`${styles.panel} ${open ? styles.panelOpen : ""}`}>
        <div className={styles.header}>
          <Link to="/" className={styles.logo} onClick={close}>
            <img
              src="https://cdn.voidcube.cloud/assets/hue_icon.svg"
              alt="EchoHub"
              className={styles.logoIcon}
            />
            <span>EchoHub</span>
          </Link>
          <button
            className={styles.closeBtn}
            onClick={close}
            aria-label="Close menu"
          >
            <HiOutlineXMark size={20} />
          </button>
        </div>

        <div className={styles.links}>
          <Link
            to="/"
            className={`${styles.link} ${location.pathname === "/" ? styles.active : ""}`}
            onClick={close}
          >
            <GoHome size={18} />
            <span>Home</span>
          </Link>
          <Link
            to="/servers"
            className={`${styles.link} ${location.pathname === "/servers" ? styles.active : ""}`}
            onClick={close}
          >
            <GoServer size={18} />
            <span>Servers</span>
          </Link>
        </div>
      </nav>
    </>
  );
}
