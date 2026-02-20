import { lazy } from "react";
import { BrowserRouter, Routes, Route } from "react-router-dom";
import DefaultLayout from "./layouts/DefaultLayout";

const Home = lazy(() => import("./pages/Home"));
const Servers = lazy(() => import("./pages/Servers"));

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<DefaultLayout />}>
          <Route path="/" element={<Home />} />
          <Route path="/servers" element={<Servers />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
