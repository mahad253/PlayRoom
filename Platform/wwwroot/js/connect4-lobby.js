(() => {
  const lobbyId = window.C4_LOBBY_ID || document.getElementById("lobbyId")?.textContent?.trim();
  let pseudo = window.C4_PSEUDO || "";

  const statusEl = document.getElementById("status");
  const boardEl = document.getElementById("board");

  const playersListEl = document.getElementById("playersList");
  const startBtn = document.getElementById("startBtn");

  const chatBox = document.getElementById("chatBox");
  const chatInput = document.getElementById("chatInput");
  const chatSendBtn = document.getElementById("chatSendBtn");

  const copyLinkBtn = document.getElementById("copyLinkBtn");
  const shareLink = document.getElementById("shareLink");

  let isHost = false;
  let canStart = false;
  let lastState = null;

  function setStatus(text, type = "info") {
    statusEl.className = `alert alert-${type} py-2 mb-0`;
    statusEl.textContent = text;
  }

  function ensurePseudo() {
    pseudo = (pseudo || "").trim();
    if (pseudo.length >= 2) return;

    const p = prompt("Choisis un pseudo (2-20 caractères) :");
    if (!p) return;
    pseudo = p.trim().slice(0, 20);
  }

  function appendChatLine(html) {
    // remove placeholder
    const first = chatBox.firstElementChild;
    if (first && first.classList.contains("text-muted")) chatBox.innerHTML = "";

    const div = document.createElement("div");
    div.className = "mb-1";
    div.innerHTML = html;
    chatBox.appendChild(div);
    chatBox.scrollTop = chatBox.scrollHeight;
  }

  function renderPlayers(players) {
    playersListEl.innerHTML = "";
    for (const p of players) {
      const li = document.createElement("li");
      li.className = "list-group-item d-flex justify-content-between align-items-center";

      const left = document.createElement("div");
      left.textContent = p.pseudo;

      if (p.isHost) {
        const badge = document.createElement("span");
        badge.className = "badge bg-dark ms-2";
        badge.textContent = "Host";
        left.appendChild(badge);
      }

      li.appendChild(left);
      playersListEl.appendChild(li);
    }
  }

  function canPlay(state) {
    if (!state) return false;
    // support camelCase/PascalCase
    const status = state.status ?? state.Status;
    return status === "InProgress";
  }

  function updateStatusFromState(state) {
    const status = state.status ?? state.Status;
    const turn = state.turn ?? state.Turn;
    const isDraw = state.isDraw ?? state.IsDraw;
    const winner = state.winner ?? state.Winner;

    if (status === "WaitingPlayers") {
      setStatus("En attente d’un 2e joueur…", "warning");
      return;
    }
    if (status === "InProgress") {
      setStatus(`Partie en cours — Tour: ${turn === 1 ? "Rouge" : "Jaune"}`, "info");
      return;
    }
    if (status === "Finished") {
      if (isDraw) setStatus("Match nul ✨", "secondary");
      else if (winner === 1) setStatus("Victoire: Rouge 🎉", "success");
      else if (winner === 2) setStatus("Victoire: Jaune 🎉", "success");
      else setStatus("Partie terminée.", "secondary");
      return;
    }
    setStatus("Statut inconnu.", "secondary");
  }

  function renderBoard(state) {
    boardEl.innerHTML = "";

    const board = state.board ?? state.Board;
    if (!board || !board[0]) return;

    for (let r = 0; r < 6; r++) {
      for (let c = 0; c < 7; c++) {
        const cell = document.createElement("div");
        cell.className = "c4-cell";
        cell.dataset.col = c;

        const v = board[r][c];
        if (v === 1) cell.classList.add("red");
        if (v === 2) cell.classList.add("yellow");

        cell.addEventListener("click", () => {
          if (!canPlay(lastState)) return;
          connection.invoke("PlayMove", lobbyId, c)
            .catch(err => setStatus(err?.toString?.() ?? String(err), "danger"));
        });

        boardEl.appendChild(cell);
      }
    }
  }

  const connection = new signalR.HubConnectionBuilder()
    .withUrl("/connect4Hub")
    .withAutomaticReconnect()
    .build();

  // --- Events
  connection.on("LobbyUpdated", (payload) => {
    const players = payload.players ?? payload.Players ?? [];
    renderPlayers(players);

    // host / canStart
    isHost = players.some(p => (p.pseudo === pseudo) && (p.isHost === true));
    canStart = !!(payload.canStart ?? payload.CanStart);

    // Start button visible seulement si host et canStart
    if (isHost && canStart) startBtn.classList.remove("d-none");
    else startBtn.classList.add("d-none");

    // Status
    if (!canStart && (players.length < 2)) setStatus("En attente d’un 2e joueur…", "warning");
    else if (canStart && isHost) setStatus("2 joueurs présents — tu peux démarrer ✅", "success");
    else if (canStart && !isHost) setStatus("2 joueurs présents — en attente du host…", "info");
  });

  connection.on("GameStateUpdated", (state) => {
    lastState = state;
    updateStatusFromState(state);
    renderBoard(state);

    // Une fois la partie commencée, on cache start
    startBtn.classList.add("d-none");
  });

  connection.on("ChatMessage", (msg) => {
    const p = msg.pseudo ?? msg.Pseudo ?? "???";
    const m = msg.message ?? msg.Message ?? "";
    appendChatLine(`<span class="text-info fw-bold">${escapeHtml(p)}</span> : ${escapeHtml(m)}`);
  });

  connection.on("ChatSystem", (msg) => {
    const m = msg.message ?? msg.Message ?? "";
    appendChatLine(`<span class="text-muted">[Système]</span> ${escapeHtml(m)}`);
  });

  connection.on("Error", (msg) => {
    setStatus(msg, "danger");
  });

  connection.onclose((err) => {
    console.error(err);
    setStatus("Connexion fermée (voir console).", "danger");
  });

  // --- Actions
  startBtn.addEventListener("click", () => {
    connection.invoke("StartGame", lobbyId)
      .catch(err => setStatus(err?.toString?.() ?? String(err), "danger"));
  });

  function sendChat() {
    const text = (chatInput.value || "").trim();
    if (!text) return;
    chatInput.value = "";
    connection.invoke("SendChatMessage", lobbyId, pseudo, text).catch(console.error);
  }

  chatSendBtn.addEventListener("click", sendChat);
  chatInput.addEventListener("keydown", (e) => {
    if (e.key === "Enter") sendChat();
  });

  copyLinkBtn?.addEventListener("click", async () => {
    try {
      await navigator.clipboard.writeText(shareLink.value);
      copyLinkBtn.textContent = "Copié ✅";
      setTimeout(() => (copyLinkBtn.textContent = "Copier"), 1200);
    } catch {
      // fallback
      shareLink.select();
      document.execCommand("copy");
    }
  });

  // Utils
  function escapeHtml(s) {
    return String(s)
      .replaceAll("&", "&amp;")
      .replaceAll("<", "&lt;")
      .replaceAll(">", "&gt;")
      .replaceAll('"', "&quot;")
      .replaceAll("'", "&#039;");
  }

  async function start() {
    ensurePseudo();
    if (!pseudo || pseudo.length < 2) {
      setStatus("Pseudo requis pour rejoindre le lobby.", "danger");
      return;
    }

    try {
      await connection.start();
      setStatus("Connecté. Rejoindre le lobby…", "info");
      await connection.invoke("JoinLobby", lobbyId, pseudo);
    } catch (e) {
      console.error(e);
      setStatus(e?.toString?.() ?? String(e), "danger");
    }
  }

  start();
})();
