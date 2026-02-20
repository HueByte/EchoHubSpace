import { lazy, Suspense } from "react";
import { Outlet } from "react-router-dom";
import SideMenu from "../components/SideMenu";
import styles from "./DefaultLayout.module.css";

const StarField = lazy(() => import("../components/StarField"));

export default function DefaultLayout() {
  return (
    <>
      <Suspense fallback={null}>
        <StarField />
      </Suspense>
      <SideMenu />
      <main className={styles.content}>
        <Suspense fallback={null}>
          <Outlet />
        </Suspense>
      </main>
    </>
  );
}
