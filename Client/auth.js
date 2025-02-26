
document.addEventListener("DOMContentLoaded", async () => {
  const currentPage = window.location.pathname;

  // Se siamo sulla pagina di login…
  if (currentPage.includes("login.html")) {
    if (await isAuthenticated()) {
      // Se l'utente è già autenticato, reindirizza alla pagina salvata o a una di default
      const redirectPage = sessionStorage.getItem("redirectPage") || "/Client/page1.html";
      sessionStorage.removeItem("redirectPage");
      // Imposta l'URL della finestra corrente al valore di redirectPage,
      // causando il caricamento della pagina corrispondente.
      window.location.href = redirectPage;
    }
    return; // Evita di eseguire il controllo delle pagine protette in login.html
  }

  // Se siamo su una pagina protetta e l'utente NON è autenticato…
  if (!await isAuthenticated()) {
    // Salva la pagina corrente per reindirizzare dopo il login
    sessionStorage.setItem("redirectPage", currentPage);
    window.location.href = "/Client/login.html";
  }
});

// Funzione per controllare se l'utente è autenticato
async function isAuthenticated() {
  const token = LocalStorage.getItem("jwt");
  if (!token) {
      return false;
  }

  try {
      const response = await fetch("http://localhost:5000/api/auth/check", {
          method: "GET",
          headers: { "Authorization": `Bearer ${token}` } // Invia il token nell'header
      });

      return response.ok;
  } catch (error) {
      console.error("Errore nel controllo autenticazione:", error);
      return false;
  }
}


