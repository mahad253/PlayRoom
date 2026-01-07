document.addEventListener("DOMContentLoaded", () => {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/petitbacHub")
        .build();
 // Bouton copier
 copyButton.addEventListener('click', function () {
    const linkField = document.getElementById('playerLink');
    const copyAlert = document.getElementById('copyAlert');

    navigator.clipboard.writeText(linkField.value)
        .then(() => {
            copyAlert.textContent = 'Lien copié dans le presse-papier !';
            copyAlert.style.display = 'block';
            setTimeout(() => copyAlert.style.display = 'none', 3000);
        })
        .catch(err => {
            copyAlert.textContent = 'Erreur lors de la copie : ' + err;
            copyAlert.style.display = 'block';
            setTimeout(() => copyAlert.style.display = 'none', 3000);
        });
});

    const gameInfo = document.getElementById("game-info");
    const gameId = gameInfo.getAttribute("data-game-id");
    const sessionToken = gameInfo.getAttribute("data-session-token");

    connection.start()
        .then(() => connection.invoke("JoinGame", gameId, sessionToken))
        .catch(err => console.error("Erreur de connexion SignalR :", err));

    connection.on("PlayerStatusUpdated", (playerPseudo, status) => {
        const playerList = document.getElementById("player-list");
        let playerRow = [...playerList.rows].find(row => row.cells[0]?.innerText === playerPseudo);

        if (!playerRow) {
            const newRow = playerList.insertRow();
            newRow.innerHTML = `
                <td>${playerPseudo}</td>
                <td>${status}</td>
                <td>
                    <button class="btn btn-primary btn-sm" 
                            onclick="openCorrectionModal('${playerPseudo}')"
                            ${status !== "Terminé" ? "disabled" : ""}>
                        Corriger
                    </button>
                </td>`;
        } else {
            playerRow.cells[1].innerText = status;
            const button = playerRow.cells[2].querySelector("button");
            button.disabled = status !== "Terminé";
        }
    });
});

function openCorrectionModal(playerPseudo) {
    const gameInfo = document.getElementById("game-info");
    const gameId = gameInfo.getAttribute("data-game-id");

    const modal = document.getElementById("modalCorrection");
    modal.dataset.playerPseudo = playerPseudo;
    modal.dataset.gameId = gameId;

    // Charger les réponses via l'API
    fetch(`/api/petitbac/getPlayerAnswersByPseudo?playerPseudo=${encodeURIComponent(playerPseudo)}`)
        .then(response => {
            if (!response.ok) {
                throw new Error("Erreur lors de la récupération des réponses du joueur.");
            }
            return response.json();
        })
        .then(data => {
            const container = document.getElementById("answers-container");
            container.innerHTML = "";

            for (const [letter, categories] of Object.entries(data.responses)) {
                for (const [category, answer] of Object.entries(categories)) {
                    container.innerHTML += `
                        <div class="mb-3">
                            <label>${category} (${letter})</label>
                            <input type="text" class="form-control" value="${answer}" readonly />
                            <div class="form-check">
                                <input class="form-check-input correct-checkbox" type="checkbox" />
                                <label class="form-check-label">Correct</label>
                            </div>
                        </div>`;
                }
            }

            // Afficher le modal
            const modalInstance = new bootstrap.Modal(modal);
            modalInstance.show();
        })
        .catch(err => {
            console.error("Erreur lors du chargement des réponses :", err);
            const container = document.getElementById("answers-container");
            container.innerHTML = `<p class="text-danger">Erreur : Impossible de charger les réponses.</p>`;
        });
}

// Gestion de la soumission des corrections
document.getElementById("submitCorrection").addEventListener("click", () => {
    const modal = document.getElementById("modalCorrection");
    const playerPseudo = modal.dataset.playerPseudo;
    const gameId = modal.dataset.gameId;

    const checkboxes = document.querySelectorAll(".correct-checkbox");
    const correctCount = [...checkboxes].filter(cb => cb.checked).length;
    const total = checkboxes.length;
    const scorePercentage = Math.round((correctCount / total) * 100);

    // Afficher le score dans une alerte ou une notification
    alert(`Score calculé : ${scorePercentage}%`);

    // Mettre à jour le score dans la table des joueurs
    const playerList = document.getElementById("player-list");
    const playerRow = [...playerList.rows].find(row => row.cells[0]?.innerText === playerPseudo);

    if (playerRow) {
        const scoreCell = playerRow.querySelector(".player-score");
        if (scoreCell) {
            scoreCell.innerText = `${scorePercentage}%`; // Mettre à jour la cellule du score
        } else {
            const newCell = playerRow.insertCell(-1);
            newCell.classList.add("player-score");
            newCell.innerText = `${scorePercentage}%`;
        }}

    // Soumettre le score via fetch
    const requestData = {
        PlayerPseudo: playerPseudo,
        GameId: gameId,
        Score: scorePercentage
    };

    fetch(`/PetitBac/SubmitScore`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(requestData)
    })
        .then(response => {
            if (!response.ok) {
                return response.json().then(errData => { throw new Error(errData.message || "Erreur lors de la soumission du score."); });
            }
            return response.json();
        })
        .then(data => {
            console.log("Score soumis avec succès :", data.message);
        })
        .catch(err => {
            console.error("Erreur lors de la soumission du score :", err.message);
        });
});
