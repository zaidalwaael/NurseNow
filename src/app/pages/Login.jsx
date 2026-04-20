import { useState ,useEffect} from "react";
import { useNavigate } from "react-router";
import { Lock, Mail, ArrowRight } from "lucide-react";
import { loginAdmin } from "../../services/authService";

export default function Login() {
  const navigate = useNavigate();
  
  useEffect(() => { 
    const token = localStorage.getItem("token")
    if (token)
      navigate("/")
  },[navigate])


  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (event) => {
    event.preventDefault();

    if (!email.trim() || !password.trim()) {
      setError("Please enter both email and password.");
      return;
    }

    try {
      setLoading(true);
      setError("");

      const data = await loginAdmin(email, password);

      localStorage.setItem("token", data.token);
      localStorage.setItem(
        "adminUser",
        JSON.stringify({
          fullName: data.fullName,
          email: data.email,
          roles: data.roles,
        })
      );

      navigate("/");
    } catch (err) {
      setError(err.message || "Login failed.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-[#F8FAFC] flex items-center justify-center px-4 py-10">
      <div className="w-full max-w-md bg-white rounded-3xl shadow-xl border border-gray-200 p-8">
        <div className="mb-8 text-center">
          <h1 className="text-3xl font-semibold text-gray-900">Sign in</h1>
          <p className="text-sm text-gray-500 mt-2">
            Access the Nurse Home Admin Dashboard.
          </p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-6">
          {error && (
            <div className="rounded-2xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-600">
              {error}
            </div>
          )}

          <div>
            <label className="block text-sm font-medium text-gray-700">
              Email address
            </label>
            <div className="mt-2 relative">
              <Mail className="absolute left-3 top-3 text-gray-400 w-4 h-4" />
              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className="w-full rounded-2xl border border-gray-300 bg-white px-10 py-3 text-sm text-gray-900 outline-none focus:border-[#1F7A8C] focus:ring-2 focus:ring-[#1F7A8C]/20"
                placeholder="admin@nursehome.com"
              />
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700">
              Password
            </label>
            <div className="mt-2 relative">
              <Lock className="absolute left-3 top-3 text-gray-400 w-4 h-4" />
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className="w-full rounded-2xl border border-gray-300 bg-white px-10 py-3 text-sm text-gray-900 outline-none focus:border-[#1F7A8C] focus:ring-2 focus:ring-[#1F7A8C]/20"
                placeholder="Enter your password"
              />
            </div>
          </div>

          <button
            type="submit"
            disabled={loading}
            className="w-full inline-flex items-center justify-center gap-2 rounded-2xl bg-[#1F7A8C] px-4 py-3 text-sm font-medium text-white hover:bg-[#18626F] transition-colors disabled:opacity-70"
          >
            {loading ? "Signing in..." : "Sign in"}
            {!loading && <ArrowRight className="w-4 h-4" />}
          </button>
        </form>

        <div className="mt-6 text-center text-sm text-gray-500">
          Need an account? Contact your administrator.
        </div>
      </div>
    </div>
  );
}