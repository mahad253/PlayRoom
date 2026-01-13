document.getElementById("isPrivate").addEventListener("change", function () {
    const passwordGroup = document.getElementById("passwordGroup");
    passwordGroup.style.display = this.checked ? "block" : "none";
});
document.getElementById("btnCreateLobby").addEventListener("click", async () => {
    const hostName = document.getElementById("hostName").value.trim();
    const isPrivate = document.getElementById("isPrivate").checked;
    const password = document.getElementById("password").value;

    const errorDiv = document.getElementById("createLobbyError");
    errorDiv.style.display = "none";
    errorDiv.textContent = "";

    if (!hostName) {
    errorDiv.textContent = "Veuillez saisir un pseudo.";
    errorDiv.style.display = "block";
    return;
}

// tu peux garder le pseudo en localStorage pour le réutiliser
localStorage.setItem("nickname", hostName);

try {
    const response = await fetch("/api/lobby", {
    method: "POST",
    headers: {
    "Content-Type": "application/json"
},
body: JSON.stringify({
    hostName: hostName,
    isPrivate: isPrivate,
    password: isPrivate ? password : null
})
});

if (!response.ok) {
    const err = await response.json().catch(() => null);
    errorDiv.textContent = err?.message || "Erreur lors de la création du lobby.";
    errorDiv.style.display = "block";
    return;
}

const lobby = await response.json();
    // on part du principe que le contrôleur renvoie au moins { code: "...", id: ... }

    // redirection vers la page de lobby avec le code dans l’URL
    window.location.href = `/Lobby/Room?code=${encodeURIComponent(lobby.code)}`;
} catch (e) {
    console.error(e);
    errorDiv.textContent = "Impossible de contacter le serveur.";
    errorDiv.style.display = "block";
}
});
