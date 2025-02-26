// login.js
document.addEventListener("DOMContentLoaded", () => {
    document.getElementById("loginForm").addEventListener("submit", async (event) => {
      event.preventDefault();
      await handleLogin();
    });
  });
  
  async function handleLogin() {
    const username = document.getElementById("username").value;
    const password = document.getElementById("password").value;
    
    try {
      const response = await fetch("http://localhost:5000/api/auth/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ username, password }),
        credentials: "include" // Per includere i cookie HttpOnly
      });
      
      if (response.ok) {
        alert("Login riuscito!");
        // Reindirizza all'ultima pagina salvata oppure a una pagina di default
        const redirectPage = sessionStorage.getItem("redirectPage") || "page1.html";
        sessionStorage.removeItem("redirectPage");
        window.location.href = redirectPage;
      } else {
        showError("Credenziali errate!");
      }
    } catch (error) {
      console.error("Errore di connessione:", error);
      showError("Errore nel server.");
    }
  }
  
  function showError(message) {
    document.getElementById("errorMessage").textContent = message;
  }
  