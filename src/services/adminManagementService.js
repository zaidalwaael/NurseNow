const API_BASE_URL = "https://localhost:7053/api";

function getAuthHeaders(includeJson = true) {
  const token = localStorage.getItem("token");

  return {
    ...(includeJson ? { "Content-Type": "application/json" } : {}),
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };
}

async function handleResponse(response) {
  if (!response.ok) {
    let errorMessage = "Request failed";

    try {
      const errorData = await response.json();

      if (Array.isArray(errorData)) {
        errorMessage = errorData.map((e) => e.description || e.code).join(", ");
      } else {
        errorMessage = errorData.message || errorMessage;
      }
    } catch {
      // ignore
    }

    throw new Error(errorMessage);
  }

  return response.json();
}

export async function fetchAdmins() {
  const response = await fetch(`${API_BASE_URL}/admin/admin-management`, {
    method: "GET",
    headers: getAuthHeaders(false),
  });

  return handleResponse(response);
}

export async function fetchAdminDetails(id) {
  const response = await fetch(`${API_BASE_URL}/admin/admin-management/${id}`, {
    method: "GET",
    headers: getAuthHeaders(false),
  });

  return handleResponse(response);
}

export async function createAdmin(payload) {
  const response = await fetch(`${API_BASE_URL}/admin/admin-management`, {
    method: "POST",
    headers: getAuthHeaders(true),
    body: JSON.stringify(payload),
  });

  return handleResponse(response);
}

export async function updateAdmin(id, payload) {
  const response = await fetch(`${API_BASE_URL}/admin/admin-management/${id}`, {
    method: "PUT",
    headers: getAuthHeaders(true),
    body: JSON.stringify(payload),
  });

  return handleResponse(response);
}

export async function deleteAdmin(id) {
  const response = await fetch(`${API_BASE_URL}/admin/admin-management/${id}`, {
    method: "DELETE",
    headers: getAuthHeaders(false),
  });

  return handleResponse(response);
}

export async function fetchRecentAdminActions() {
  const response = await fetch(
    `${API_BASE_URL}/admin/admin-management/recent-actions`,
    {
      method: "GET",
      headers: getAuthHeaders(false),
    }
  );

  return handleResponse(response);
}