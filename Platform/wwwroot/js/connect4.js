(() => {
  const statusEl = document.getElementById("status");
  const boardEl = document.getElementById("board");

  const lobbyId =
    window.CONNECT4_LOBBY ||
    document.getElementById("lobbyId")?.textContent?.trim();

  function setStatus(text, type = "info") {
    statusEl.className = `alert alert-${type} py-2`;
    statusEl.textContent = text;
  }

  if (!lobbyId) {
    setStatus("LobbyId manquant (erreur de page).", "danger");
    return;
  }

  const connection = new signalR.HubConnectionBuilder()
    .withUrl("/connect4Hub")
    .withAutomaticReconnect()
    .build();

  // Helpers pour supporter Status/status, Board/board, etc.
  const get = (obj, pascal, camel) => obj?.[pascal] ?? obj?.[camel];

  let lastState = null;

  function updateStatus(state) {
    const status = String(get(state, "Status", "status") ?? "");

    if (status === "WaitingPlayers") {
      setStatus("En attente d’un 2e joueur… (ouvre le lien dans un autre navigateur/onglet)", "warning");
      return;
    }

    if (status === "InProgress") {
      const turnVal = get(state, "Turn", "turn");
      const turn = turnVal === 1 ? "Rouge" : "Jaune";
      setStatus(`Partie en cours — Tour: ${turn}`, "info");
      return;
    }

    const isDraw = !!get(state, "IsDraw", "isDraw");
    if (isDraw) {
      setStatus("Match nul ✨", "secondary");
      return;
    }

    const winner = get(state, "Winner", "winner");
    if (winner === 1) {
      setStatus("Victoire: Rouge 🎉", "success");
      return;
    }
    if (winner === 2) {
      setStatus("Victoire: Jaune 🎉", "success");
      return;
    }

    setStatus("Partie terminée.", "secondary");
  }

  function renderBoard(state) {
    boardEl.innerHTML = "";

    const board = get(state, "Board", "board");
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
          // Le serveur valide tout (tour/colonne/état) donc c'est ok de tenter
          connection
            .invoke("PlayMove", lobbyId, c)
            .catch(err => setStatus(err?.toString?.() ?? String(err), "danger"));
        });

        boardEl.appendChild(cell);
      }
    }
  }

  connection.on("GameStateUpdated", (state) => {
    console.log("STATE:", state);
    lastState = state;
    updateStatus(state);
    renderBoard(state);
  });

  connection.on("Error", (msg) => setStatus(msg, "danger"));

  connection.onclose((err) => {
    console.error("SignalR closed:", err);
    setStatus("Connexion SignalR fermée (voir console).", "danger");
  });

  async function start() {
    try {
      await connection.start();
      setStatus("Connecté. Join lobby…", "info");
      await connection.invoke("JoinLobby", lobbyId);
    } catch (e) {
      console.error(e);
      setStatus(e?.toString?.() ?? String(e), "danger");
    }
  }

  start();
})();
