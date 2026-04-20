import { useEffect, useRef } from "react";
import { useNavigate } from "react-router";
import { User, Settings, LogOut } from "lucide-react";

export function ProfileDropdown({ isOpen, onClose }) {
  const navigate = useNavigate();
  const dropdownRef = useRef(null);

  useEffect(() => {
    function handleClickOutside(event) {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target)) {
        onClose();
      }
    }

    if (isOpen) {
      document.addEventListener("mousedown", handleClickOutside);
    }

    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, [isOpen, onClose]);

  if (!isOpen) return null;

  const handleViewProfile = () => {
    navigate("/profile");
    onClose();
  };

  const handleEditProfile = () => {
    navigate("/profile/edit");
    onClose();
  };

  const handleAccountSettings = () => {
    navigate("/settings");
    onClose();
  };

  const handleLogout = () => {
      localStorage.removeItem("token")
      localStorage.removeItem("adminUser")
      navigate("/login");
      onClose();
  };

  return (
    <div
      ref={dropdownRef}
      className="absolute top-14 right-0 w-56 bg-white rounded-lg shadow-lg border border-gray-200 z-50"
    >
      {/* Profile Info */}
      <div className="p-4 border-b border-gray-200">
        <p className="text-sm text-gray-800">Admin User</p>
        <p className="text-xs text-gray-500 mt-1">admin@nursehome.com</p>
      </div>

      {/* Menu Items */}
      <div className="py-2">
        <button
          onClick={handleViewProfile}
          className="w-full px-4 py-2 text-left text-sm text-gray-700 hover:bg-gray-50 transition-colors flex items-center gap-3"
        >
          <User className="w-4 h-4 text-gray-600" />
          View Profile
        </button>

        <button
          onClick={handleEditProfile}
          className="w-full px-4 py-2 text-left text-sm text-gray-700 hover:bg-gray-50 transition-colors flex items-center gap-3"
        >
          <User className="w-4 h-4 text-gray-600" />
          Edit Profile
        </button>

        <button
          onClick={handleAccountSettings}
          className="w-full px-4 py-2 text-left text-sm text-gray-700 hover:bg-gray-50 transition-colors flex items-center gap-3"
        >
          <Settings className="w-4 h-4 text-gray-600" />
          Account Settings
        </button>
      </div>

      {/* Logout */}
      <div className="border-t border-gray-200 py-2">
        <button
          onClick={handleLogout}
          className="w-full px-4 py-2 text-left text-sm text-red-600 hover:bg-red-50 transition-colors flex items-center gap-3"
        >
          <LogOut className="w-4 h-4" />
          Logout
        </button>
      </div>
    </div>
  );
}
