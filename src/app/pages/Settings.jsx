import { useState } from "react";
import { Save, Shield, Bell, Database } from "lucide-react";

export default function Settings() {
  const [settings, setSettings] = useState({
    autoAssignNurse: true,
    requireDocumentVerification: true,
    emailNotifications: true,
    notifyNewNurse: true,
    notifyNewServiceRequest: true,
    notifyNewComplaint: true,
    sessionTimeout: 30,
    minimumPasswordLength: 8,
  });

  const handleToggle = (key) => {
    setSettings((prev) => ({
      ...prev,
      [key]: !prev[key],
    }));
  };

  const handleInputChange = (key, value) => {
    setSettings((prev) => ({
      ...prev,
      [key]: value,
    }));
  };

  const handleSave = () => {
    alert("Settings saved successfully!");
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="bg-white rounded-lg shadow p-6">
        <h2 className="text-gray-800 mb-1">System Settings</h2>
        <p className="text-sm text-gray-500">
          Configure platform settings and preferences
        </p>
      </div>

      {/* Platform Settings */}
      <div className="bg-white rounded-lg shadow p-6">
        <div className="flex items-center gap-3 mb-6">
          <div className="w-10 h-10 bg-[#1F7A8C]/10 rounded-lg flex items-center justify-center">
            <Database className="w-5 h-5 text-[#1F7A8C]" />
          </div>
          <h3 className="text-gray-800">Platform Settings</h3>
        </div>

        <div className="space-y-4">
          <div className="flex items-center justify-between p-4 border border-gray-200 rounded-lg">
            <div>
              <p className="text-sm text-gray-800">Automatic Nurse Assignment</p>
              <p className="text-xs text-gray-500 mt-1">
                Automatically assign nurses to service requests
              </p>
            </div>
            <label className="relative inline-flex items-center cursor-pointer">
              <input
                type="checkbox"
                className="sr-only peer"
                checked={settings.autoAssignNurse}
                onChange={() => handleToggle("autoAssignNurse")}
              />
              <div className="w-11 h-6 bg-gray-200 rounded-full peer peer-checked:bg-[#1F7A8C] peer-checked:after:translate-x-full after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border after:border-gray-300 after:rounded-full after:h-5 after:w-5 after:transition-all" />
            </label>
          </div>

          <div className="flex items-center justify-between p-4 border border-gray-200 rounded-lg">
            <div>
              <p className="text-sm text-gray-800">Require Document Verification</p>
              <p className="text-xs text-gray-500 mt-1">
                Nurses must upload documents for verification
              </p>
            </div>
            <label className="relative inline-flex items-center cursor-pointer">
              <input
                type="checkbox"
                className="sr-only peer"
                checked={settings.requireDocumentVerification}
                onChange={() => handleToggle("requireDocumentVerification")}
              />
              <div className="w-11 h-6 bg-gray-200 rounded-full peer peer-checked:bg-[#1F7A8C] peer-checked:after:translate-x-full after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border after:border-gray-300 after:rounded-full after:h-5 after:w-5 after:transition-all" />
            </label>
          </div>

          <div className="flex items-center justify-between p-4 border border-gray-200 rounded-lg">
            <div>
              <p className="text-sm text-gray-800">Email Notifications</p>
              <p className="text-xs text-gray-500 mt-1">
                Send email notifications to users
              </p>
            </div>
            <label className="relative inline-flex items-center cursor-pointer">
              <input
                type="checkbox"
                className="sr-only peer"
                checked={settings.emailNotifications}
                onChange={() => handleToggle("emailNotifications")}
              />
              <div className="w-11 h-6 bg-gray-200 rounded-full peer peer-checked:bg-[#1F7A8C] peer-checked:after:translate-x-full after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border after:border-gray-300 after:rounded-full after:h-5 after:w-5 after:transition-all" />
            </label>
          </div>
        </div>
      </div>

      {/* Notification Preferences */}
      <div className="bg-white rounded-lg shadow p-6">
        <div className="flex items-center gap-3 mb-6">
          <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
            <Bell className="w-5 h-5 text-blue-600" />
          </div>
          <h3 className="text-gray-800">Notification Preferences</h3>
        </div>

        <div className="space-y-4">
          <div className="flex items-center justify-between p-4 border border-gray-200 rounded-lg">
            <div>
              <p className="text-sm text-gray-800">New Nurse Registration</p>
              <p className="text-xs text-gray-500 mt-1">
                Notify when new nurses register
              </p>
            </div>
            <label className="relative inline-flex items-center cursor-pointer">
              <input
                type="checkbox"
                className="sr-only peer"
                checked={settings.notifyNewNurse}
                onChange={() => handleToggle("notifyNewNurse")}
              />
              <div className="w-11 h-6 bg-gray-200 rounded-full peer peer-checked:bg-[#1F7A8C] peer-checked:after:translate-x-full after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border after:border-gray-300 after:rounded-full after:h-5 after:w-5 after:transition-all" />
            </label>
          </div>

          <div className="flex items-center justify-between p-4 border border-gray-200 rounded-lg">
            <div>
              <p className="text-sm text-gray-800">New Service Request</p>
              <p className="text-xs text-gray-500 mt-1">
                Notify when service requests are submitted
              </p>
            </div>
            <label className="relative inline-flex items-center cursor-pointer">
              <input
                type="checkbox"
                className="sr-only peer"
                checked={settings.notifyNewServiceRequest}
                onChange={() => handleToggle("notifyNewServiceRequest")}
              />
              <div className="w-11 h-6 bg-gray-200 rounded-full peer peer-checked:bg-[#1F7A8C] peer-checked:after:translate-x-full after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border after:border-gray-300 after:rounded-full after:h-5 after:w-5 after:transition-all" />
            </label>
          </div>

          <div className="flex items-center justify-between p-4 border border-gray-200 rounded-lg">
            <div>
              <p className="text-sm text-gray-800">New Complaint</p>
              <p className="text-xs text-gray-500 mt-1">
                Notify when complaints are submitted
              </p>
            </div>
            <label className="relative inline-flex items-center cursor-pointer">
              <input
                type="checkbox"
                className="sr-only peer"
                checked={settings.notifyNewComplaint}
                onChange={() => handleToggle("notifyNewComplaint")}
              />
              <div className="w-11 h-6 bg-gray-200 rounded-full peer peer-checked:bg-[#1F7A8C] peer-checked:after:translate-x-full after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border after:border-gray-300 after:rounded-full after:h-5 after:w-5 after:transition-all" />
            </label>
          </div>
        </div>
      </div>

      {/* Security Settings */}
      <div className="bg-white rounded-lg shadow p-6">
        <div className="flex items-center gap-3 mb-6">
          <div className="w-10 h-10 bg-red-100 rounded-lg flex items-center justify-center">
            <Shield className="w-5 h-5 text-red-600" />
          </div>
          <h3 className="text-gray-800">Security Settings</h3>
        </div>

        <div className="space-y-4">
          <div>
            <label className="block text-sm text-gray-700 mb-2">
              Session Timeout (minutes)
            </label>
            <input
              type="number"
              value={settings.sessionTimeout}
              onChange={(e) =>
                handleInputChange("sessionTimeout", e.target.value)
              }
              className="w-full md:w-64 px-3 py-2 border border-gray-300 rounded-lg bg-white"
            />
          </div>

          <div>
            <label className="block text-sm text-gray-700 mb-2">
              Minimum Password Length
            </label>
            <input
              type="number"
              value={settings.minimumPasswordLength}
              onChange={(e) =>
                handleInputChange("minimumPasswordLength", e.target.value)
              }
              className="w-full md:w-64 px-3 py-2 border border-gray-300 rounded-lg bg-white"
            />
          </div>
        </div>
      </div>

      {/* Save Button */}
      <div className="flex justify-end">
        <button
          onClick={handleSave}
          className="px-6 py-3 bg-[#1F7A8C] text-white rounded-lg hover:bg-[#18626F] transition-colors flex items-center gap-2"
        >
          <Save className="w-4 h-4" />
          Save All Settings
        </button>
      </div>
    </div>
  );
}