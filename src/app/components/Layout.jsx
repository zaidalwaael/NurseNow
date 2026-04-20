import { useState } from "react";
import { Outlet, Link, useLocation } from "react-router";
import {
  LayoutDashboard,
  UserCheck,
  Users,
  ClipboardList,
  Wallet,
  MessageSquare,
  Bell,
  BarChart3,
  Shield,
  Settings,
  LogOut,
} from "lucide-react";
import { NotificationDropdown } from "./NotificationDropdown";
import { ProfileDropdown } from "./ProfileDropdown";
import { useNavigate } from "react-router";
// Navigation items for Nurse Home Admin Dashboard


const navItems = [
  { path: "/", label: "Dashboard", icon: LayoutDashboard },
  { path: "/nurse-verification", label: "Nurse Verification", icon: UserCheck },
  { path: "/users", label: "Users Management", icon: Users },
  { path: "/service-requests", label: "Service Requests", icon: ClipboardList },
  {
    path: "/payment-transactions",
    label: "Payment & Transactions",
    icon: Wallet,
  },
  { path: "/complaints", label: "Complaints & Support", icon: MessageSquare },
  {
    path: "/notifications",
    label: "Notifications & Announcements",
    icon: Bell,
  },
  {
    path: "/notifications/all",
    label: "All Notifications",
    icon: Bell,
  },
  { path: "/reports", label: "Reports & Analytics", icon: BarChart3 },
  { path: "/admin-management", label: "Admin Management", icon: Shield },
  { path: "/settings", label: "Settings", icon: Settings },
];

export function Layout() {
  const location = useLocation();
  const navigate = useNavigate();
  const [isNotificationOpen, setIsNotificationOpen] = useState(false);
  const [isProfileOpen, setIsProfileOpen] = useState(false);

  const routeTitleMap = {
    "/profile": "View Profile",
    "/profile/edit": "Edit Profile",
    "/notifications/all": "All Notifications",
  };

  const pageTitle =
    navItems.find((item) => item.path === location.pathname)?.label ||
    routeTitleMap[location.pathname] ||
    "Dashboard";

  return (
    <div className="flex h-screen bg-gray-50">
      {/* Sidebar */}
      <aside className="w-64 bg-[#1F7A8C] text-white flex flex-col fixed h-full">
        <div className="p-6 border-b border-[#18626F]">
          <h1 className="text-xl text-white">Nurse Home</h1>
          <p className="text-xs text-white/80 mt-1">Admin Dashboard</p>
        </div>

        <nav className="flex-1 min-h-0 overflow-y-auto smooth-scrollbar sidebar-scrollbar py-4">
          {navItems.map((item) => {
            const Icon = item.icon;
            const isActive = location.pathname === item.path;
            return (
              <Link
                key={item.path}
                to={item.path}
                className={`flex items-center gap-3 px-6 py-3 transition-colors ${
                  isActive
                    ? "bg-white/10 border-l-4 border-white text-white"
                    : "text-white/90 hover:bg-white/5"
                }`}
              >
                <Icon className="w-5 h-5" />
                <span className="text-sm">{item.label}</span>
              </Link>
            );
          })}
        </nav>

        <div className="p-6 border-t border-[#18626F]">
          <button className="flex items-center gap-3 text-white/90 hover:text-white transition-colors w-full" onClick={()=>{localStorage.removeItem("token")
      localStorage.removeItem("adminUser")
      navigate("/login");}}>
            <LogOut className="w-5 h-5" />
            <span className="text-sm">Logout</span>
          </button>
        </div>
      </aside>

      {/* Main Content */}
      <div className="flex-1 ml-64 flex flex-col">
        {/* Header */}
        <header className="bg-white border-b border-gray-200 px-8 py-4 flex items-center justify-between">
          <h2 className="text-gray-800">{pageTitle}</h2>

          <div className="flex items-center gap-6">
            {/* Notification Bell */}
            <div className="relative">
              <button
                onClick={() => {
                  setIsNotificationOpen(!isNotificationOpen);
                  setIsProfileOpen(false);
                }}
                className="relative p-2 hover:bg-gray-100 rounded-lg transition-colors"
              >
                <Bell className="w-5 h-5 text-gray-600" />
                <span className="absolute top-1 right-1 w-2 h-2 bg-red-500 rounded-full"></span>
              </button>
              <NotificationDropdown
                isOpen={isNotificationOpen}
                onClose={() => setIsNotificationOpen(false)}
              />
            </div>

            {/* Admin Profile */}
            <div className="relative">
              <button
                onClick={() => {
                  setIsProfileOpen(!isProfileOpen);
                  setIsNotificationOpen(false);
                }}
                className="flex items-center gap-3 hover:bg-gray-100 rounded-lg p-2 transition-colors"
              >
                <div className="text-right">
                  <p className="text-sm text-gray-800">Admin User</p>
                  <p className="text-xs text-gray-500">Admin</p>
                </div>
                <div className="w-10 h-10 bg-[#1F7A8C] text-white rounded-full flex items-center justify-center">
                  <span className="text-sm">AU</span>
                </div>
              </button>
              <ProfileDropdown
                isOpen={isProfileOpen}
                onClose={() => setIsProfileOpen(false)}
              />
            </div>
          </div>
        </header>

        {/* Page Content */}
        <main className="flex-1 overflow-y-auto p-8 smooth-scrollbar">
          <Outlet />
        </main>

        {/* Footer */}
        <footer className="bg-white border-t border-gray-200 px-8 py-3 text-center text-sm text-gray-500">
          Nurse Home Admin Dashboard v1.0.0 • Internal Admin System
        </footer>
      </div>
    </div>
  );
}
