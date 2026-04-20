import { createBrowserRouter } from "react-router";
import ProtectedRoute from "./components/ProtectedRoute";
import { Layout } from "./components/Layout";
import Dashboard from "./pages/Dashboard";
import NurseVerification from "./pages/NurseVerification";
import UsersManagement from "./pages/UsersManagement";
import ServiceRequests from "./pages/ServiceRequests";
import PaymentTransactions from "./pages/PaymentTransactions";
import ComplaintsSupport from "./pages/ComplaintsSupport";
import NotificationsAnnouncements from "./pages/NotificationsAnnouncements";
import AllNotifications from "./pages/AllNotifications";
import Reports from "./pages/Reports";
import AdminManagement from "./pages/AdminManagement";
import Settings from "./pages/Settings";
import ViewProfile from "./pages/ViewProfile";
import EditProfile from "./pages/EditProfile";
import Login from "./pages/Login";

export const router = createBrowserRouter([
  {
    path: "/login",
    Component: Login,
  },
  {
    path: "/",
    element: (
      <ProtectedRoute>
        <Layout />
      </ProtectedRoute>
    ),
    children: [
      { index: true, Component: Dashboard },
      { path: "nurse-verification", Component: NurseVerification },
      { path: "users", Component: UsersManagement },
      { path: "service-requests", Component: ServiceRequests },
      { path: "payment-transactions", Component: PaymentTransactions },
      { path: "complaints", Component: ComplaintsSupport },
      { path: "notifications", Component: NotificationsAnnouncements },
      { path: "notifications/all", Component: AllNotifications },
      { path: "reports", Component: Reports },
      { path: "admin-management", Component: AdminManagement },
      { path: "settings", Component: Settings },
      { path: "profile", Component: ViewProfile },
      { path: "profile/edit", Component: EditProfile },
    ],
  },
]);