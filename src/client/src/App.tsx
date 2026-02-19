import { BrowserRouter, Routes, Route } from "react-router-dom";
import StarField from "./components/StarField";
import SideMenu from "./components/SideMenu";
import Home from "./pages/Home";
import Servers from "./pages/Servers";

export default function App() {
  return (
    <BrowserRouter>
      <StarField />
      <SideMenu />
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/servers" element={<Servers />} />
      </Routes>
    </BrowserRouter>
  );
}
