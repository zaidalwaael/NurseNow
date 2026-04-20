const API_BASE_URL = "https://localhost:7053/api";

function getAuthHeaders() {
  const token = localStorage.getItem("token");

  return {
    "Content-Type": "application/json",
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };
}

async function handleResponse(response) {
  if (!response.ok) {
    let errorMessage = "Request failed";

    try {
      const errorData = await response.json();
      errorMessage = errorData.message || errorMessage;
    } catch {
      // ignore
    }

    throw new Error(errorMessage);
  }

  return response.json();
}

export async function fetchUsers(role = "Patient", status = "All", search = "") {
  const query = new URLSearchParams();

  if (role) query.append("role", role);
  if (status && status !== "All") query.append("status", status);
  if (search) query.append("search", search);

  const response = await fetch(`${API_BASE_URL}/admin/users?${query.toString()}`, {
    method: "GET",
    headers: getAuthHeaders(),
  });

  return handleResponse(response);
}

export async function fetchUserDetails(id) {
  const response = await fetch(`${API_BASE_URL}/admin/users/${id}`, {
    method: "GET",
    headers: getAuthHeaders(),
  });

  return handleResponse(response);
}

export async function toggleUserStatus(id) {
  const response = await fetch(`${API_BASE_URL}/admin/users/${id}/toggle-status`, {
    method: "PUT",
    headers: getAuthHeaders(),
  });

  return handleResponse(response);
}

export async function resetUserPassword(id, newPassword) {
  const response = await fetch(`${API_BASE_URL}/admin/users/${id}/reset-password`, {
    method: "PUT",
    headers: getAuthHeaders(),
    body: JSON.stringify({ newPassword }),
  });

  return handleResponse(response);
}