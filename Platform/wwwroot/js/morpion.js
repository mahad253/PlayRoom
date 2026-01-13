// ======================================================
// DATA
// ======================================================
const lobbyId = document.getElementById("lobbyId")?.value;
let currentPseudo = document.getElementById("pseudo")?.value || "";

// ======================================================
// UI ELEMENTS
// ======================================================
const cells = document.querySelectorAll(".cell");
const statusEl = document.getElementById("status");

const playersList = document.getElementById("playersList");
const startBtn = document.getElementById("startBtn");
const restartBtn = document.getElementById("restartBtn");
const lobbyStatus = document.getElementById("lobbyStatus");

const shareLink = document.getElementById("shareLink");
const copyBtn = document.getElementById("copyBtn");
const copyToast = document.getElementById("copyToast");

// HEADER USER NAME
const currentUserNameEl = document.getElementById("currentUserName");

// pseudo prompt
const pseudoPrompt = document.getElementById("pseudoPrompt");
const pseudoInput = document.getElementById("pseudoInput");
const confirmPseudoBtn = document.getElementById("confirmPseudoBtn");

// chat
const chatMessages = document.getElementById("chatMessages");
const chatInput = document.getElementById("chatInput");
const chatSendBtn = document.getElementById("chatSendBtn");

// ======================================================
// GAME STATE
// ======================================================
let mySymbol = null;
let isHost = false;
let toastTimer = null;

// ======================================================
// HEADER SYNC (IMPORTANT)
// ======================================================
function updateHeaderPseudo() {
    if (currentUserNameEl && currentPseudo) {
        currentUserNameEl.textContent = currentPseudo;
    }
}

// ======================================================
// TOAST
// ======================================================
function showToast(message) {
    if (!copyToast) return;

    copyToast.textContent = message;
    copyToast.classList.add("show");

    if (toastTimer) clearTimeout(toastTimer);
    toastTimer = setTimeout(() => {
        copyToast.classList.remove("show");
    }, 2200);
}

// ======================================================
// SHARE LINK
// ======================================================
if (shareLink && lobbyId) {
    const url = new URL(window.location.href);
    url.pathname = `/Morpion/Lobby/${lobbyId}`;
    url.search = "";

    const lobbyUrl = url.toString();
    shareLink.textContent = lobbyUrl;

    copyBtn?.addEventListener("click", () => {
        navigator.clipboard.writeText(lobbyUrl).then(() => {
            showToast("✅ Lien copié dans le presse-papiers");
        });
    });
}

// ======================================================
// SIGNALR
// ======================================================
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/morpionHub")
    .build();

// ======================================================
// HELPERS
// ======================================================
function renderBoard(board) {
    board.forEach((value, index) => {
        cells[index].textContent = value;
        cells[index].classList.toggle("disabled", value !== "");
    });
}

function setCellsEnabled(enabled) {
    cells.forEach(c => {
        if (enabled) {
            c.classList.remove("disabled");
        } else {
            c.classList.add("disabled");
        }
    });
}

function canPlay(currentPlayer) {
    return mySymbol && currentPlayer === mySymbol;
}

// ======================================================
// RENDER LOBBY
// ======================================================
function renderLobby(players, status) {
    playersList.innerHTML = "";
    isHost = false;

    players.forEach(p => {
        const li = document.createElement("li");
        li.className =
            "list-group-item d-flex justify-content-between align-items-center lobby-player-item";

        const nameSpan = document.createElement("span");
        nameSpan.textContent = p.pseudo;
        li.appendChild(nameSpan);

        if (p.isHost) {
            const badge = document.createElement("span");
            badge.className = "badge-host";
            badge.textContent = "HOST";
            li.appendChild(badge);

            if (p.pseudo === currentPseudo) {
                isHost = true;
            }
        }

        playersList.appendChild(li);
    });

    lobbyStatus.textContent = `Statut : ${status}`;

    if (isHost && status === "Waiting" && players.length === 2) {
        startBtn.classList.remove("d-none");
        startBtn.disabled = false;
    } else {
        startBtn.classList.add("d-none");
        startBtn.disabled = true;
    }

    if (!isHost) {
        restartBtn.classList.add("d-none");
    }
}

// ======================================================
// CHAT
// ======================================================
function appendChatMessage({ pseudo, message, system }) {
    if (!chatMessages) return;

    const row = document.createElement("div");
    row.className = system ? "chat-row system" : "chat-row";

    if (system) {
        row.textContent = message;
    } else {
        const author = document.createElement("span");
        author.className = "chat-author";
        author.textContent = pseudo + " : ";

        const txt = document.createElement("span");
        txt.className = "chat-text";
        txt.textContent = message;

        row.appendChild(author);
        row.appendChild(txt);
    }

    chatMessages.appendChild(row);
    chatMessages.scrollTop = chatMessages.scrollHeight;
}

function sendChat() {
    const msg = chatInput?.value?.trim();
    if (!msg) return;

    connection.invoke("SendChatMessage", lobbyId, currentPseudo, msg)
        .catch(() => showToast("❌ Erreur d’envoi du message"));

    chatInput.value = "";
    chatInput.focus();
}

// ======================================================
// SIGNALR EVENTS
// ======================================================
connection.on("LobbyUpdated", payload => {
    renderLobby(payload.players, payload.status);
    updateHeaderPseudo(); // ✅ sécurité

    if (payload.status === "Waiting") {
        statusEl.textContent = "En attente d’un autre joueur…";
        setCellsEnabled(false);
    }
});

connection.on("YouAre", payload => {
    mySymbol = payload.symbol;
});

connection.on("GameStarted", payload => {
    const g = payload.game;

    renderLobby(payload.players, payload.status);
    renderBoard(g.board);

    startBtn.classList.add("d-none");

    statusEl.textContent =
        `Tour du joueur ${g.currentPlayer} (${canPlay(g.currentPlayer) ? "à toi" : "à l'autre"})`;

    setCellsEnabled(canPlay(g.currentPlayer));
});

connection.on("UpdateGame", payload => {
    const g = payload.game;

    renderBoard(g.board);

    if (g.finished) {
        statusEl.textContent =
            g.winner === "Draw"
                ? "Match nul 🤝"
                : `Victoire de ${g.winner} 🎉`;

        setCellsEnabled(false);

        if (isHost) {
            restartBtn.classList.remove("d-none");
        }
    } else {
        statusEl.textContent =
            `Tour du joueur ${g.currentPlayer} (${canPlay(g.currentPlayer) ? "à toi" : "à l'autre"})`;

        restartBtn.classList.add("d-none");
        setCellsEnabled(canPlay(g.currentPlayer));
    }
});

// CHAT EVENTS
connection.on("ChatMessage", payload => {
    appendChatMessage({
        pseudo: payload.pseudo,
        message: payload.message,
        system: false
    });
});

connection.on("ChatSystem", payload => {
    appendChatMessage({
        message: payload.message,
        system: true
    });
});

// ======================================================
// UI ACTIONS
// ======================================================
cells.forEach(cell => {
    cell.addEventListener("click", () => {
        if (cell.classList.contains("disabled")) return;
        const index = parseInt(cell.dataset.index);
        connection.invoke("PlayMove", lobbyId, index);
    });
});

startBtn?.addEventListener("click", () => {
    if (!isHost) return;
    connection.invoke("StartGame", lobbyId);
});

restartBtn?.addEventListener("click", () => {
    if (!isHost) return;
    connection.invoke("ResetGame", lobbyId);
});

chatSendBtn?.addEventListener("click", sendChat);

chatInput?.addEventListener("keydown", e => {
    if (e.key === "Enter") {
        e.preventDefault();
        sendChat();
    }
});

// ======================================================
// START / JOIN
// ======================================================
connection.start()
    .then(() => {
        if (!currentPseudo) {
            pseudoPrompt.style.display = "flex";
            return;
        }

        updateHeaderPseudo(); // ✅ host
        connection.invoke("JoinLobby", lobbyId, currentPseudo);
    })
    .catch(() => {
        statusEl.textContent = "Erreur de connexion au serveur.";
    });

// ======================================================
// CONFIRM PSEUDO (NON-HOST)
// ======================================================
confirmPseudoBtn?.addEventListener("click", () => {
    const value = pseudoInput.value.trim();
    if (!value) return;

    currentPseudo = value;
    pseudoPrompt.style.display = "none";

    updateHeaderPseudo(); // ✅ non-host
    connection.invoke("JoinLobby", lobbyId, currentPseudo);
});
