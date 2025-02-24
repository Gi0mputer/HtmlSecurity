
document.addEventListener("DOMContentLoaded", function () {
    const form = document.querySelector("form");  // Seleziona il form di login
    const loginButton = form.querySelector("button");

    // Evento di submit del form
    form.addEventListener("submit", async function (event) {
        event.preventDefault(); // Evita il comportamento predefinito

        // Ottieni i valori di username e password dai campi input
        const username = document.getElementById("username").value;
        const password = document.getElementById("password").value;

        try {
            const response = await fetch("/login", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ username, password }),
                credentials: "include" // Invia automaticamente i cookie (compresi gli HttpOnly)
            });

            if (response.ok) {
                alert("Login riuscito!");
                window.location.href = "/protected"; // Reindirizza alla pagina protetta
            } else {
                alert("Credenziali errate. Riprova.");
            }
        } catch (error) {
            console.error("Errore durante il login:", error);
            alert("Errore di connessione al server.");
        }
    });

    // **Controllo dell'autenticazione al caricamento della pagina**
    async function checkAuth() {
        try {
            const response = await fetch("/check-auth", {
                method: "GET",
                credentials: "include" // Invia i cookie per verificare l'autenticazione
            });

            if (response.ok) {
                console.log("Utente autenticato");
                window.location.href = "/protected"; // Se autenticato, reindirizza subito
            } else {
                console.log("Utente non autenticato");
            }
        } catch (error) {
            console.error("Errore nel controllo dell'autenticazione:", error);
        }
    }

    checkAuth(); // Controlla l'autenticazione all'avvio della pagina

    // **Logout**
    async function logout() {
        try {
            await fetch("/logout", {
                method: "POST",
                credentials: "include" // Invia i cookie per invalidare la sessione
            });

            alert("Logout effettuato.");
            window.location.href = "/login"; // Torna alla pagina di login
        } catch (error) {
            console.error("Errore nel logout:", error);
        }
    }

    // Se vuoi un pulsante di logout, crealo dinamicamente (es. su dashboard)
    const logoutButton = document.createElement("button");
    logoutButton.textContent = "Logout";
    logoutButton.onclick = logout;
    document.body.appendChild(logoutButton);
});

