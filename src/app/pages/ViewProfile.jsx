import { Link } from "react-router";
import { User, Mail, Phone, MapPin, CalendarDays, Shield } from "lucide-react";

export default function ViewProfile() {
  const profile = {
    fullName: "Admin User",
    email: "admin@nursehome.com",
    role: "Administrator",
    phone: "+1 (555) 123-4567",
    location: "Nurse Home HQ, Dallas, TX",
    joined: "January 12, 2025",
    status: "Active",
  };

  return (
    <div className="space-y-6">
      <div className="bg-white rounded-lg shadow p-6">
        <div className="flex flex-col md:flex-row items-start md:items-center justify-between gap-4">
          <div>
            <h2 className="text-gray-800 text-2xl font-semibold">
              Profile Overview
            </h2>
            <p className="text-sm text-gray-500 mt-1">
              Review your account information and access profile settings.
            </p>
          </div>
          <Link
            to="/profile/edit"
            className="inline-flex items-center gap-2 px-5 py-3 rounded-lg bg-[#1F7A8C] text-white hover:bg-[#18626F] transition-colors"
          >
            <User className="w-4 h-4" />
            Edit Profile
          </Link>
        </div>
      </div>

      <div className="grid gap-6 lg:grid-cols-[1.4fr_1fr]">
        <div className="bg-white rounded-lg shadow p-6">
          <div className="flex items-center gap-4 mb-6">
            <div className="w-16 h-16 rounded-full bg-[#1F7A8C] text-white flex items-center justify-center text-xl font-semibold">
              AU
            </div>
            <div>
              <h3 className="text-lg text-gray-800">{profile.fullName}</h3>
              <p className="text-sm text-gray-500">{profile.role}</p>
            </div>
          </div>

          <div className="space-y-4">
            <div className="flex items-center gap-3 text-gray-600">
              <Mail className="w-4 h-4" />
              <div>
                <p className="text-sm font-medium text-gray-800">Email</p>
                <p className="text-sm text-gray-500">{profile.email}</p>
              </div>
            </div>
            <div className="flex items-center gap-3 text-gray-600">
              <Phone className="w-4 h-4" />
              <div>
                <p className="text-sm font-medium text-gray-800">Phone</p>
                <p className="text-sm text-gray-500">{profile.phone}</p>
              </div>
            </div>
            <div className="flex items-center gap-3 text-gray-600">
              <MapPin className="w-4 h-4" />
              <div>
                <p className="text-sm font-medium text-gray-800">Location</p>
                <p className="text-sm text-gray-500">{profile.location}</p>
              </div>
            </div>
            <div className="flex items-center gap-3 text-gray-600">
              <CalendarDays className="w-4 h-4" />
              <div>
                <p className="text-sm font-medium text-gray-800">Joined</p>
                <p className="text-sm text-gray-500">{profile.joined}</p>
              </div>
            </div>
            <div className="flex items-center gap-3 text-gray-600">
              <Shield className="w-4 h-4" />
              <div>
                <p className="text-sm font-medium text-gray-800">Status</p>
                <p className="text-sm text-gray-500">{profile.status}</p>
              </div>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-6">
          <h3 className="text-lg text-gray-800 mb-4">About</h3>
          <p className="text-sm text-gray-600 leading-relaxed">
            This profile represents the administrator account for the Nurse Home
            system. Use the edit page to keep contact and account details up to
            date.
          </p>

          <div className="mt-6 space-y-4">
            <div className="rounded-lg border border-gray-200 p-4 bg-gray-50">
              <p className="text-xs text-gray-500 uppercase tracking-wide">
                Admin Area
              </p>
              <p className="text-sm text-gray-800 mt-2">
                Full access to nurse verification, user management, reports, and
                system settings.
              </p>
            </div>
            <div className="rounded-lg border border-gray-200 p-4 bg-gray-50">
              <p className="text-xs text-gray-500 uppercase tracking-wide">
                Security
              </p>
              <p className="text-sm text-gray-800 mt-2">
                We recommend updating your password regularly and enabling any
                available multi-factor authentication.
              </p>
            </div>
          </div>
        </div>
      </div>

      <div className="bg-white rounded-lg shadow p-6">
        <h3 className="text-lg text-gray-800 mb-4">Recent Activity</h3>
        <div className="space-y-3">
          <div className="flex items-center justify-between gap-4 border border-gray-200 rounded-lg p-4">
            <div>
              <p className="text-sm text-gray-800">Updated user permissions</p>
              <p className="text-xs text-gray-500 mt-1">Today at 11:20 AM</p>
            </div>
            <span className="text-xs text-green-600 bg-green-50 rounded-full px-2 py-1">
              Completed
            </span>
          </div>
          <div className="flex items-center justify-between gap-4 border border-gray-200 rounded-lg p-4">
            <div>
              <p className="text-sm text-gray-800">
                Reviewed new nurse registration
              </p>
              <p className="text-xs text-gray-500 mt-1">Yesterday at 4:05 PM</p>
            </div>
            <span className="text-xs text-blue-600 bg-blue-50 rounded-full px-2 py-1">
              In Review
            </span>
          </div>
          <div className="flex items-center justify-between gap-4 border border-gray-200 rounded-lg p-4">
            <div>
              <p className="text-sm text-gray-800">
                Saved account profile updates
              </p>
              <p className="text-xs text-gray-500 mt-1">March 31, 2026</p>
            </div>
            <span className="text-xs text-gray-600 bg-gray-100 rounded-full px-2 py-1">
              Info
            </span>
          </div>
        </div>
      </div>
    </div>
  );
}
