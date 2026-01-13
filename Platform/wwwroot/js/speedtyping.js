// ~/wwwroot/js/speedtyping_1.js

// =========================
// Etat du jeu
// =========================

const gameState = {
    isRunning: false,
    text: "",
    duration: 60,
    timeLeft: 60,
    timerId: null,
    totalTyped: 0,
    correctChars: 0,
    progress: 0
};

// =========================
// SignalR / Lobby
// =========================

let connection = null;
let lobbyId = null;
let pseudo = null;

// =========================
// DOM
// =========================

const timerEl = document.getElementById("timer");
const wpmEl = document.getElementById("wpm");
const accuracyEl = document.getElementById("accuracy");
const scoreEl = document.getElementById("score");
const textToTypeEl = document.getElementById("textToType");
const userInputEl = document.getElementById("userInput");
const progressBarEl = document.getElementById("progressBar");
const progressTextEl = document.getElementById("progressText");
const playerCountEl = document.getElementById("playerCount");
const playersPanel = document.getElementById("playersPanel");
const btnStart = document.getElementById("btnStart");
const btnReset = document.getElementById("btnReset");
const btnLeaderboard = document.getElementById("btnLeaderboard");
const resultsModal = document.getElementById("resultsModal");
const modalWpm = document.getElementById("modalWpm");
const modalAccuracy = document.getElementById("modalAccuracy");
const modalScore = document.getElementById("modalScore");
const modalChars = document.getElementById("modalChars");
const messagesBox = document.querySelector(".messages-box");
const messageInput = document.getElementById("messageInput");
const btnSend = document.getElementById("btnSend");
const root = document.getElementById("speedtyping-root");

// =========================
// Connexion SignalR
// =========================

async function initConnection() {
    if (connection) return;

    connection = new signalR.HubConnectionBuilder()
        .withUrl("/speedTypingHub")
        .withAutomaticReconnect()
        .build();

    registerHubEvents();

    await connection.start();
    console.log("Connected to SpeedTypingHub");
}

function registerHubEvents() {
    connection.on("LobbyUpdated", payload => {
        if (playerCountEl) {
            playerCountEl.textContent = payload.players.length.toString();
        }
        updatePlayersPanel(payload.players);
    });

    connection.on("ChatSystem", payload => {
        addMessage("System", payload.message);
    });

    connection.on("ChatMessage", payload => {
        addMessage(payload.pseudo, payload.message);
    });

    connection.on("GameStarted", payload => {
        gameState.isRunning = true;
        gameState.text = payload.textToType;
        gameState.duration = payload.duration || 60;
        gameState.timeLeft = gameState.duration;
        gameState.totalTyped = 0;
        gameState.correctChars = 0;
        gameState.progress = 0;

        resetLocalState(false);
        renderTextToType();
        startTimer();
        updatePlayersStats(payload.players);
    });

    connection.on("ProgressUpdated", payload => {
        updatePlayersStats(payload.players);

        if (payload.status === "Finished") {
            gameState.isRunning = false;
            openResultsModal();
        }
    });
}

// =========================
// Chat
// =========================

function addMessage(author, text) {
    if (!messagesBox) return;

    const wrapper = document.createElement("div");
    wrapper.className = "message";

    const avatar = document.createElement("div");
    avatar.className = "message-avatar";
    avatar.textContent = author[0]?.toUpperCase() || "?";

    const content = document.createElement("div");
    content.className = "message-content";

    const authorEl = document.createElement("div");
    authorEl.className = "message-author";
    authorEl.textContent = author;

    const textEl = document.createElement("div");
    textEl.className = "message-text";
    textEl.textContent = text;

    content.appendChild(authorEl);
    content.appendChild(textEl);

    wrapper.appendChild(avatar);
    wrapper.appendChild(content);

    messagesBox.appendChild(wrapper);
    messagesBox.scrollTop = messagesBox.scrollHeight;
}

// =========================
// Players
// =========================

function updatePlayersPanel(players) {
    if (!playersPanel) return;

    playersPanel.innerHTML = "";

    players.forEach(p => {
        const badge = document.createElement("div");
        badge.className = "player-badge";

        const avatar = document.createElement("div");
        avatar.className = "player-avatar";
        avatar.textContent = p.pseudo[0]?.toUpperCase() || "?";

        const info = document.createElement("div");
        info.className = "player-info";

        const name = document.createElement("div");
        name.className = "player-name";
        name.textContent = p.pseudo;

        const status = document.createElement("div");
        status.className = "player-status";
        status.textContent = "En ligne";

        info.appendChild(name);
        info.appendChild(status);

        const wpm = document.createElement("div");
        wpm.className = "player-wpm";
        wpm.textContent =
            (typeof p.wpm === "number" ? Math.round(p.wpm) : 0) + " WPM";

        badge.appendChild(avatar);
        badge.appendChild(info);
        badge.appendChild(wpm);

        playersPanel.appendChild(badge);
    });
}

// =========================
// Texte & stats
// =========================

function renderTextToType() {
    if (!textToTypeEl) return;

    textToTypeEl.innerHTML = "";

    for (let i = 0; i < gameState.text.length; i++) {
        const span = document.createElement("span");
        span.className = "char upcoming";
        span.textContent = gameState.text[i];
        textToTypeEl.appendChild(span);
    }
}

function updateLocalStats(wpm, accuracy) {
    if (wpmEl) wpmEl.textContent = Math.round(wpm).toString();
    if (accuracyEl) accuracyEl.textContent = Math.round(accuracy) + "%";

    const score = Math.floor(wpm * (accuracy / 100));
    if (scoreEl) scoreEl.textContent = score.toString();

    const progress =
        gameState.text.length > 0
            ? (gameState.correctChars / gameState.text.length) * 100
            : 0;

    gameState.progress = progress;

    if (progressBarEl) progressBarEl.style.width = progress + "%";
    if (progressTextEl) progressTextEl.textContent = Math.round(progress) + "%";
}

function updatePlayersStats(players) {
    if (!players) return;

    updatePlayersPanel(players);

    const me = players.find(p => p.pseudo === pseudo);
    if (me) {
        gameState.correctChars = me.correctChars;
        gameState.totalTyped = me.totalTyped;
        updateLocalStats(me.wpm, me.accuracy);
    }
}

// =========================
// Timer
// =========================

function startTimer() {
    if (!timerEl) return;

    if (gameState.timerId) clearInterval(gameState.timerId);

    gameState.timeLeft = gameState.duration;
    timerEl.textContent = gameState.timeLeft.toString();

    gameState.timerId = setInterval(() => {
        gameState.timeLeft--;
        if (gameState.timeLeft < 0) gameState.timeLeft = 0;

        timerEl.textContent = gameState.timeLeft.toString();

        if (gameState.timeLeft <= 0) {
            clearInterval(gameState.timerId);
            gameState.isRunning = false;
            openResultsModal();
        }
    }, 1000);
}

// =========================
// Saisie
// =========================

function resetLocalState(clearInput = true) {
    gameState.totalTyped = 0;
    gameState.correctChars = 0;
    gameState.progress = 0;

    if (clearInput && userInputEl) {
        userInputEl.value = "";
    }

    updateLocalStats(0, 0);
}

function onUserInputChange() {
    

    const typed = userInputEl.value;
    gameState.totalTyped = typed.length;
    gameState.correctChars = 0;

    const charsSpans = textToTypeEl
        ? Array.from(textToTypeEl.querySelectorAll(".char"))
        : [];

    for (let i = 0; i < gameState.text.length; i++) {
        const expected = gameState.text[i];
        const actual = typed[i] || "";

        if (!charsSpans[i]) continue;

        if (!actual) {
            charsSpans[i].className = "char upcoming";
        } else if (actual === expected) {
            charsSpans[i].className = "char correct";
            gameState.correctChars++;
        } else {
            charsSpans[i].className = "char wrong";
        }
    }

    const accuracy = gameState.totalTyped === 0
        ? 0
        : (gameState.correctChars / gameState.totalTyped) * 100;

    const elapsedSeconds =
        gameState.duration - parseInt(timerEl?.textContent || "0", 10);
    const seconds = elapsedSeconds <= 0 ? 1 : elapsedSeconds;

    const wpm = (gameState.correctChars / 5) * (60 / seconds);

    updateLocalStats(wpm, accuracy);

    if (connection && lobbyId) {
        connection.invoke(
            "UpdateProgress",
            lobbyId,
            gameState.totalTyped,
            gameState.correctChars
        ).catch(err => console.error(err));
    }
}

// =========================
// Résultats
// =========================

function openResultsModal() {
    if (!resultsModal) return;

    if (modalWpm) modalWpm.textContent = wpmEl ? wpmEl.textContent : "0";
    if (modalAccuracy) modalAccuracy.textContent =
        accuracyEl ? accuracyEl.textContent : "0%";
    if (modalScore) modalScore.textContent =
        scoreEl ? scoreEl.textContent : "0";
    if (modalChars) modalChars.textContent =
        gameState.correctChars.toString();

    resultsModal.classList.add("show");
}

// =========================
// DOM events
// =========================

function registerDomEvents() {
    if (btnStart) {
        btnStart.addEventListener("click", () => {
            if (!connection || !lobbyId) return;

            const activeDiff = document.querySelector(".diff-btn.active");
            const diff = activeDiff
                ? activeDiff.getAttribute("data-diff")
                : "easy";

            connection.invoke("StartGame", lobbyId, diff)
                .catch(err => console.error(err));
        });
    }

    if (btnReset) {
        btnReset.addEventListener("click", () => {
            resetLocalState(true);
        });
    }

    if (btnLeaderboard) {
        btnLeaderboard.addEventListener("click", () => {
            alert("Classement à implémenter");
        });
    }

    if (userInputEl) {
        userInputEl.addEventListener("input", onUserInputChange);
    }

    if (btnSend && messageInput) {
        btnSend.addEventListener("click", () => {
            const text = messageInput.value.trim();
            if (!text || !connection || !lobbyId) return;

            connection.invoke("SendChatMessage", lobbyId, pseudo, text)
                .catch(err => console.error(err));

            messageInput.value = "";
        });

        messageInput.addEventListener("keyup", e => {
            if (e.key === "Enter") {
                btnSend.click();
            }
        });
    }

    document.querySelectorAll(".diff-btn").forEach(btn => {
        btn.addEventListener("click", () => {
            document.querySelectorAll(".diff-btn")
                .forEach(b => b.classList.remove("active"));
            btn.classList.add("active");
        });
    });

    const btnCopyLink = document.getElementById("btnCopyLink");
    const lobbyUrlInput = document.getElementById("lobbyUrl");

    if (btnCopyLink && lobbyUrlInput) {
        btnCopyLink.addEventListener("click", async () => {
            try {
                await navigator.clipboard.writeText(lobbyUrlInput.value);
                btnCopyLink.textContent = "Copié !";
                setTimeout(() => btnCopyLink.textContent = "Copier", 1500);
            } catch {
                alert("Impossible de copier le lien, copiez-le manuellement.");
            }
        });
    }
}

// =========================
// Init
// =========================

document.addEventListener("DOMContentLoaded", async () => {
    if (!root) return;

    lobbyId = root.dataset.lobbyId;
    pseudo = root.dataset.pseudo || "";

    registerDomEvents();
    resetLocalState(true);

    try {
        await initConnection();
    } catch (err) {
        console.error("Erreur connexion SpeedTypingHub:", err);
        return;
    }

    console.log(pseudo, lobbyId);

    if (lobbyId && pseudo) {
        console.log("JoinLobby auto avec pseudo:", pseudo);
        await connection
            .invoke("JoinLobby", lobbyId, pseudo)
            .catch(err => console.error("JoinLobby error:", err));
    }
});


