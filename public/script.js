document.getElementById("loginForm").addEventListener("submit", async function(event) {
    event.preventDefault();

    const username = document.getElementById("username").value;
    const password = document.getElementById("password").value;

    const response = await fetch("http://localhost:3000/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ username, password })
    });

    const result = await response.json();
    
    if (result.success) {
        sessionStorage.setItem("token", result.token);
        window.location.href = "dashboard.html";
    } else {
        document.getElementById("errorMessage").textContent = "Credenziali errate!";
    }
});
