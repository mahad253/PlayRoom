// ===============================
// CONNEXION SIGNALR
// ===============================
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/morpionHub")
    .build();

// ===============================
// ELEMENTS DOM
// ===============================
const cells = document.querySelectorAll(".cell");
const status = document.getElementById("status");
const restartBtn = document.getElementById("restartBtn");

// ===============================
// RECEPTION ETAT DU JEU
// ===============================
connection.on("UpdateGame", game => {

    // Mise à jour du plateau
    game.board.forEach((value, index) => {
        cells[index].textContent = value;
        cells[index].classList.toggle("disabled", value !== "");
    });

    // Etat de la partie
    if (game.finished) {
        status.textContent =
            game.winner === "Draw"
                ? "Match nul 🤝"
                : `Victoire de ${game.winner} 🎉`;

        restartBtn.style.display = "inline-block";
    } else {
        status.textContent = `Tour du joueur ${game.currentPlayer}`;
        restartBtn.style.display = "none";
    }
});

// ===============================
// CLICK SUR UNE CASE
// ===============================
cells.forEach(cell => {
    cell.addEventListener("click", () => {
        const index = parseInt(cell.dataset.index);

        connection.invoke("PlayMove", index)
            .catch(err => console.error("Erreur SignalR :", err));
    });
});

// ===============================
// REJOUER
// ===============================
restartBtn.addEventListener("click", () => {
    connection.invoke("ResetGame")
        .catch(err => console.error("Erreur reset :", err));
});

// ===============================
// CONNEXION AU SERVEUR
// ===============================
connection.start()
    .then(() => {
        status.textContent = "En attente d’un autre joueur...";
    })
    .catch(err => {
        console.error(err);
        status.textContent = "Erreur de connexion au serveur";
    });
