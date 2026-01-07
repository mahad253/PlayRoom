document.getElementById('joinButton').addEventListener('click', function () {
    // Récupérer le champ du pseudo
    var pseudoField = document.getElementById('Pseudo');
    var pseudoValue = pseudoField.value.trim();
    
    // Récupérer le conteneur de l'erreur
    var pseudoError = document.getElementById('pseudoError');
    
    // Réinitialiser l'affichage de l'erreur (masquer les messages d'erreur au départ)
    pseudoError.classList.add("d-none");
    
    // Vérifier si le champ du pseudo est vide
    if (pseudoValue === '') {
        pseudoError.textContent = 'Veuillez saisir un pseudo avant de rejoindre la partie.';
        pseudoError.classList.remove("d-none"); // Afficher l'erreur
    } else if (pseudoValue.length < 5) {
        // Vérifier si le pseudo a moins de 5 caractères
        pseudoError.textContent = 'Le pseudo doit contenir au moins 5 caractères.';
        pseudoError.classList.remove("d-none"); // Afficher l'erreur
    } else if (!/^[a-zA-Z]/.test(pseudoValue)) {
        // Vérifier si le pseudo commence par une lettre
        pseudoError.textContent = 'Le pseudo doit commencer par une lettre.';
        pseudoError.classList.remove("d-none"); // Afficher l'erreur
    } else {
        // Si le pseudo est valide, soumettre le formulaire
        document.getElementById('joinForm').submit();
    }
});


document.addEventListener("DOMContentLoaded", () => {
    // Récupérer les données injectées dans la vue
    const gameInfo = document.getElementById("game-info");
    const gameId = gameInfo.getAttribute("data-game-id");
    const sessionToken = gameInfo.getAttribute("data-session-token");

    console.log("Game ID récupéré :", gameId);
    console.log("Session Token récupéré :", sessionToken);

    // Initialiser la connexion SignalR
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/petitbacHub")
        .configureLogging(signalR.LogLevel.Information)
        .build();

    // Gestion de l'événement SignalR "PlayerStatusUpdated"
    connection.on("PlayerStatusUpdated", (playerPseudo, status) => {
        console.log(`[SignalR] Événement reçu : Joueur=${playerPseudo}, Statut=${status}`);

        const playerList = document.getElementById("player-list");
        if (playerList) {
            // Rechercher une ligne existante pour ce joueur
            let playerRow = [...playerList.rows].find(row => row.cells[0]?.innerText === playerPseudo);

            if (playerRow) {
                // Mettre à jour le statut si le joueur existe déjà
                playerRow.cells[1].innerText = status;
            } else {
                // Ajouter une nouvelle ligne pour le joueur
                const newRow = playerList.insertRow();
                const nameCell = newRow.insertCell(0);
                const statusCell = newRow.insertCell(1);
                nameCell.innerText = playerPseudo;
                statusCell.innerText = status;
            }
        } else {
            console.warn("Table 'player-list' introuvable.");
        }
    });

})