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

export async function fetchComplaints(search = "", status = "All") {
  const query = new URLSearchParams();

  if (search) query.append("search", search);
  if (status && status !== "All") query.append("status", status);

  const response = await fetch(
    `${API_BASE_URL}/admin/complaints?${query.toString()}`,
    {
      method: "GET",
      headers: getAuthHeaders(),
    }
  );

  return handleResponse(response);
}

export async function fetchComplaintDetails(id) {
  const response = await fetch(`${API_BASE_URL}/admin/complaints/${id}`, {
    method: "GET",
    headers: getAuthHeaders(),
  });

  return handleResponse(response);
}

export async function respondToComplaint(id, responseText) {
  const response = await fetch(`${API_BASE_URL}/admin/complaints/${id}/respond`, {
    method: "PUT",
    headers: getAuthHeaders(),
    body: JSON.stringify({ response: responseText }),
  });

  return handleResponse(response);
}

export async function resolveComplaint(id) {
  const response = await fetch(`${API_BASE_URL}/admin/complaints/${id}/resolve`, {
    method: "PUT",
    headers: getAuthHeaders(),
  });

  return handleResponse(response);
}