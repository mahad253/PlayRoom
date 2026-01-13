// layout-nav.js

const navPlayerInfo = document.getElementById("navPlayerInfo");
const btnCreateLobby = document.getElementById("btnCreateLobby");
const btnViewLobbies = document.getElementById("btnViewLobbies");
const btnPlayerConnect = document.getElementById("btnPlayerConnect");
const btnPlayerDisconnect = document.getElementById("btnPlayerDisconnect");

function getPlatformPlayer() {
    const stored = localStorage.getItem("platformPlayer");
    if (!stored) return null;
    try {
        const p = JSON.parse(stored);
        if (p && p.pseudo) return p;
    } catch (e) {
        console.error("Erreur lecture platformPlayer", e);
    }
    return null;
}

function updateNavbarPlayer() {
    const player = getPlatformPlayer();
    if (!navPlayerInfo) return;

    const statusSpan = navPlayerInfo.querySelector(".nav-player-status");

    if (player) {
        statusSpan.textContent = `Connecté : ${player.pseudo}`;
        if (btnPlayerConnect) btnPlayerConnect.style.display = "none";
        if (btnPlayerDisconnect) btnPlayerDisconnect.style.display = "inline-block";
    } else {
        statusSpan.textContent = "Invité";
        if (btnPlayerConnect) btnPlayerConnect.style.display = "inline-block";
        if (btnPlayerDisconnect) btnPlayerDisconnect.style.display = "none";
    }
}

document.addEventListener("DOMContentLoaded", () => {
    updateNavbarPlayer();

    if (btnPlayerConnect) {
        btnPlayerConnect.addEventListener("click", () => {
            // vers ta page de profil joueur
            window.location.href = "/Home/Player";
        });
    }

    if (btnPlayerDisconnect) {
        btnPlayerDisconnect.addEventListener("click", () => {
            localStorage.removeItem("platformPlayer");
            updateNavbarPlayer();
        });
    }

    if (btnCreateLobby) {
        btnCreateLobby.addEventListener("click", () => {
            const player = getPlatformPlayer();
            if (!player) {
                alert("Définis ton pseudo avant de créer un lobby.");
                window.location.href = "/Home/Player";
                return;
            }
            // À adapter : route de création de lobby (form ou direct)
            window.location.href = "/Lobby/Create";
        });
    }

    if (btnViewLobbies) {
        btnViewLobbies.addEventListener("click", () => {
            // Page qui liste tous les lobbys
            window.location.href = "/Lobby/List";
        });
    }
});
