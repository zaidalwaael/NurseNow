import { useState } from "react";
import { Link } from "react-router";
import { Save, ArrowLeft, Mail, Lock, Phone, MapPin, User } from "lucide-react";

const initialProfile = {
  fullName: "Admin User",
  email: "admin@nursehome.com",
  phone: "+1 (555) 123-4567",
  role: "Administrator",
  location: "Nurse Home HQ, Dallas, TX",
  password: "",
};

export default function EditProfile() {
  const [formData, setFormData] = useState(initialProfile);

  const handleChange = (field) => (event) => {
    setFormData((current) => ({ ...current, [field]: event.target.value }));
  };

  const handleSubmit = (event) => {
    event.preventDefault();
    alert("Profile updated successfully!");
  };

  return (
    <div className="space-y-6">
      <div className="bg-white rounded-lg shadow p-6 flex flex-col md:flex-row items-start md:items-center justify-between gap-4">
        <div>
          <h2 className="text-gray-800 text-2xl font-semibold">Edit Profile</h2>
          <p className="text-sm text-gray-500 mt-1">
            Update your account details and save changes to your profile.
          </p>
        </div>

        <Link
          to="/profile"
          className="inline-flex items-center gap-2 px-4 py-2 rounded-lg border border-gray-200 text-gray-700 hover:bg-gray-50 transition-colors"
        >
          <ArrowLeft className="w-4 h-4" />
          Back to Profile
        </Link>
      </div>

      <form
        onSubmit={handleSubmit}
        className="bg-white rounded-lg shadow p-6 space-y-6"
      >
        <div className="grid gap-6 md:grid-cols-2">
          <label className="block text-sm text-gray-700">
            Full Name
            <input
              type="text"
              value={formData.fullName}
              onChange={handleChange("fullName")}
              className="mt-2 w-full rounded-lg border border-gray-300 px-3 py-2 outline-none focus:border-[#1F7A8C] focus:ring-2 focus:ring-[#1F7A8C]/20"
            />
          </label>

          <label className="block text-sm text-gray-700">
            Email Address
            <div className="mt-2 relative">
              <Mail className="absolute left-3 top-3 w-4 h-4 text-gray-400" />
              <input
                type="email"
                value={formData.email}
                onChange={handleChange("email")}
                className="w-full rounded-lg border border-gray-300 px-10 py-2 outline-none focus:border-[#1F7A8C] focus:ring-2 focus:ring-[#1F7A8C]/20"
              />
            </div>
          </label>

          <label className="block text-sm text-gray-700">
            Phone Number
            <div className="mt-2 relative">
              <Phone className="absolute left-3 top-3 w-4 h-4 text-gray-400" />
              <input
                type="text"
                value={formData.phone}
                onChange={handleChange("phone")}
                className="w-full rounded-lg border border-gray-300 px-10 py-2 outline-none focus:border-[#1F7A8C] focus:ring-2 focus:ring-[#1F7A8C]/20"
              />
            </div>
          </label>

          <label className="block text-sm text-gray-700">
            Role
            <div className="mt-2 relative">
              <User className="absolute left-3 top-3 w-4 h-4 text-gray-400" />
              <input
                type="text"
                value={formData.role}
                onChange={handleChange("role")}
                className="w-full rounded-lg border border-gray-300 px-10 py-2 outline-none focus:border-[#1F7A8C] focus:ring-2 focus:ring-[#1F7A8C]/20"
              />
            </div>
          </label>
        </div>

        <div className="grid gap-6 md:grid-cols-2">
          <label className="block text-sm text-gray-700">
            Location
            <div className="mt-2 relative">
              <MapPin className="absolute left-3 top-3 w-4 h-4 text-gray-400" />
              <input
                type="text"
                value={formData.location}
                onChange={handleChange("location")}
                className="w-full rounded-lg border border-gray-300 px-10 py-2 outline-none focus:border-[#1F7A8C] focus:ring-2 focus:ring-[#1F7A8C]/20"
              />
            </div>
          </label>

          <label className="block text-sm text-gray-700">
            New Password
            <div className="mt-2 relative">
              <Lock className="absolute left-3 top-3 w-4 h-4 text-gray-400" />
              <input
                type="password"
                value={formData.password}
                onChange={handleChange("password")}
                placeholder="Leave blank to keep current password"
                className="w-full rounded-lg border border-gray-300 px-10 py-2 outline-none focus:border-[#1F7A8C] focus:ring-2 focus:ring-[#1F7A8C]/20"
              />
            </div>
          </label>
        </div>

        <div className="bg-gray-50 rounded-lg border border-gray-200 p-4">
          <div className="flex items-start gap-3">
            <div className="w-10 h-10 rounded-full bg-[#1F7A8C] text-white flex items-center justify-center">
              <Save className="w-5 h-5" />
            </div>
            <div>
              <p className="text-sm font-medium text-gray-800">
                Profile update
              </p>
              <p className="text-sm text-gray-500 mt-1">
                Save your updated contact and account details.
              </p>
            </div>
          </div>
        </div>

        <div className="flex justify-end gap-3">
          <Link
            to="/profile"
            className="px-5 py-3 rounded-lg border border-gray-200 text-gray-700 hover:bg-gray-50 transition-colors"
          >
            Cancel
          </Link>
          <button
            type="submit"
            className="inline-flex items-center gap-2 px-6 py-3 rounded-lg bg-[#1F7A8C] text-white hover:bg-[#18626F] transition-colors"
          >
            <Save className="w-4 h-4" />
            Save Changes
          </button>
        </div>
      </form>
    </div>
  );
}
