(() => {
  const lobbyId =
    (window.CONNECT4_LOBBY || "").trim() ||
    document.getElementById("lobbyId")?.textContent?.trim();

  const pseudo = (window.CONNECT4_PSEUDO || "").trim();

  const statusEl = document.getElementById("status");
  const boardEl = document.getElementById("board");

  const playersListEl = document.getElementById("playersList");
  const startBtn = document.getElementById("startBtn");
  const leaveBtn = document.getElementById("leaveBtn");

  const chatBox = document.getElementById("chatMessages"); // ✅ bon id
  const chatInput = document.getElementById("chatInput");
  const chatSendBtn = document.getElementById("chatSendBtn");

  const copyLinkBtn = document.getElementById("copyLinkBtn");
  const copyToast = document.getElementById("copyToast");
  const restartBtn = document.getElementById("restartBtn");


  let isHost = false;
  let canStart = false;
  let lastState = null;

  function setStatus(text, type = "info") {
    // ✅ status est un badge, pas une alert
    const map = {
      info: "bg-info text-dark",
      warning: "bg-warning text-dark",
      success: "bg-success",
      danger: "bg-danger",
      secondary: "bg-secondary",
    };
    statusEl.className = `badge ${map[type] ?? "bg-info text-dark"} px-3 py-2`;
    statusEl.textContent = text;
  }

  // ✅ pas de prompt : si pseudo absent => retour Index
  if (!pseudo || pseudo.length < 2) {
    window.location.href = "/Connect4/Index";
    return;
  }

  function escapeHtml(s) {
    return String(s)
      .replaceAll("&", "&amp;")
      .replaceAll("<", "&lt;")
      .replaceAll(">", "&gt;")
      .replaceAll('"', "&quot;")
      .replaceAll("'", "&#039;");
  }

  function appendChatLine(html) {
    if (!chatBox) return;
    const div = document.createElement("div");
    div.className = "mb-1";
    div.innerHTML = html;
    chatBox.appendChild(div);
    chatBox.scrollTop = chatBox.scrollHeight;
  }

  function renderPlayers(players) {
    playersListEl.innerHTML = "";
    for (const p of players) {
      const row = document.createElement("div");
      row.className = "d-flex justify-content-between align-items-center border rounded-3 px-3 py-2";

      const left = document.createElement("div");
      left.className = "fw-semibold";
      left.textContent = p.pseudo;

      if (p.isHost) {
        const badge = document.createElement("span");
        badge.className = "badge bg-dark text-white ms-2";
        badge.textContent = "Host";
        left.appendChild(badge);
      }

      row.appendChild(left);
      playersListEl.appendChild(row);
    }
  }

  function canPlay(state) {
    if (!state) return false;
    const status = state.status ?? state.Status;
    return status === "InProgress";
  }

  function updateStatusFromState(state) {
    const status = state.status ?? state.Status;
    const turn = state.turn ?? state.Turn;
    const isDraw = state.isDraw ?? state.IsDraw;
    const winner = state.winner ?? state.Winner;

    if (status === "WaitingPlayers") return setStatus("En attente d’un 2e joueur…", "warning");
    if (status === "InProgress") return setStatus(`Tour: ${turn === 1 ? "Rouge" : "Jaune"}`, "info");
    if (status === "Finished") {
      if (isDraw) return setStatus("Match nul ✨", "secondary");
      if (winner === 1) return setStatus("Victoire: Rouge 🎉", "success");
      if (winner === 2) return setStatus("Victoire: Jaune 🎉", "success");
      return setStatus("Partie terminée.", "secondary");
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

 connection.on("LobbyUpdated", (payload) => {
  const players = payload.players ?? payload.Players ?? [];
  renderPlayers(players);

  // Déterminer host et possibilité de démarrer
  isHost = players.some(p => p.pseudo === pseudo && p.isHost === true);
  canStart = !!(payload.canStart ?? payload.CanStart);

  // ▶️ Bouton Démarrer
  startBtn.disabled = !(isHost && canStart);

  if (isHost && canStart) {
    startBtn.classList.remove("d-none");
  } else {
    startBtn.classList.add("d-none");
  }

  // 🔄 Si on peut (re)démarrer, on cache Rejouer
  if (canStart) {
    restartBtn?.classList.add("d-none");
  }

  // 🟦 Status texte
  if (!canStart && players.length < 2) {
    setStatus("En attente d’un 2e joueur…", "warning");
  } else if (canStart && isHost) {
    setStatus("2 joueurs présents — tu peux démarrer ✅", "success");
  } else if (canStart && !isHost) {
    setStatus("2 joueurs présents — en attente du host…", "info");
  }
});


  connection.on("GameStateUpdated", (state) => {
  lastState = state;
  updateStatusFromState(state);
  renderBoard(state);

  const status = state.status ?? state.Status;

  if (status === "InProgress") {
    startBtn.classList.add("d-none");
    restartBtn?.classList.add("d-none");
  }

  if (status === "Finished") {
    startBtn.classList.add("d-none");
    restartBtn?.classList.remove("d-none"); // ✅ apparaît à la fin
  }

  if (status === "WaitingPlayers") {
    // on laisse LobbyUpdated décider si Start est visible pour le host
    restartBtn?.classList.add("d-none");
  }
});


  // ✅ Chat events (doit matcher le serveur)
  connection.on("ChatMessage", (msg) => {
    const p = msg.pseudo ?? msg.Pseudo ?? "???";
    const m = msg.message ?? msg.Message ?? "";
    appendChatLine(`<span class="text-info fw-bold">${escapeHtml(p)}</span> : ${escapeHtml(m)}`);
  });



  connection.on("ChatSystem", (msg) => {
    const m = msg.message ?? msg.Message ?? "";
    appendChatLine(`<span class="text-muted">[Système]</span> ${escapeHtml(m)}`);
  });

  connection.on("Error", (msg) => setStatus(msg, "danger"));

  startBtn.addEventListener("click", () => {
    connection.invoke("StartGame", lobbyId)
      .catch(err => setStatus(err?.toString?.() ?? String(err), "danger"));
  });
   restartBtn?.addEventListener("click", () => {
  connection.invoke("RestartGame", lobbyId)
    .catch(err => setStatus(err?.toString?.() ?? String(err), "danger"));
});


  function sendChat() {
    const text = (chatInput.value || "").trim();
    if (!text) return;
    chatInput.value = "";
    connection.invoke("SendChatMessage", lobbyId, pseudo, text)
      .catch(err => console.error(err));
  }

  chatSendBtn.addEventListener("click", sendChat);
  chatInput.addEventListener("keydown", (e) => {
    if (e.key === "Enter") sendChat();
  });

  // ✅ Copier lien + toast
 copyLinkBtn?.addEventListener("click", async () => {
  // Construire un lien JOIN propre (sans pseudo)
  const joinUrl = `${window.location.origin}/Connect4/Lobby/${encodeURIComponent(lobbyId)}`;


  try {
    await navigator.clipboard.writeText(joinUrl);

    const toast = document.getElementById("copyToast");
    toast?.classList.add("show");
    setTimeout(() => toast?.classList.remove("show"), 1500);
  } catch {
    prompt("Copie ce lien :", joinUrl);
  }
});



  leaveBtn?.addEventListener("click", () => {
    window.location.href = "/";
  });

  (async function start() {
    if (!lobbyId) {
      setStatus("LobbyId manquant.", "danger");
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
  })();
})();
