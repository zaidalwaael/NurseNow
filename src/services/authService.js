const API_BASE_URL = "https://localhost:7053/api";

async function handleResponse(response) {
  if (!response.ok) {
    let errorMessage = "Login failed";

    try {
      const errorData = await response.json();
      errorMessage =
        errorData.message ||
        errorData.title ||
        "Invalid email or password";
    } catch {
      // ignore
    }

    throw new Error(errorMessage);
  }

  return response.json();
}

export async function loginAdmin(email, password) {
  const response = await fetch(`${API_BASE_URL}/auth/login`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      email,
      password,
    }),
  });

  return handleResponse(response);
}