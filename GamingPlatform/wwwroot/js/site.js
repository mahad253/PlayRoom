// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


// Pour les modal : Afin d'ajouter un champs dans le cas ou création de lobby privé
$(document).ready(function () {
    $('#privateLobby').change(function () {
        $('#passwordField').toggleClass('d-none', !this.checked);
    });
});

// Crée un nouveau lobby avec les données du formulaire
function createLobbyForm() {
    const lobbyName = document.getElementById('lobbyName').value;
    const isPrivate = document.getElementById('privateLobby').checked;
    const password = isPrivate ? document.getElementById('lobbyPassword').value : '';
    console.log(lobbyName, isPrivate);
    $.post('/Lobby/CreateLobby', { name: lobbyName, isPrivate, password })
        .done(response => {
            if (response.success) {
                window.location.href = `/SpeedTypingGame/Join?code=${response.lobbyCode}`;
            } else {
                alert("Erreur lors de la création du lobby.");
            }
        });
}

// créer un loby global
function createGlobalLobbyForm(){
    const lobbyName = document.getElementById('lobbyName').value;
    const game = document.getElementById('game').value;
    const isPrivate = document.getElementById('privateLobby').checked;
    const password = isPrivate ? document.getElementById('lobbyPassword').value : '';
    console.log(lobbyName, isPrivate);
    $.post('/Lobby/CreateLobby', { name: lobbyName, isPrivate, password })
        .done(response => {
            if (response.success) {
                window.location.href = `/SpeedTypingGame/Join?code=${response.lobbyCode}`;
            } else {
                alert("Erreur lors de la création du lobby.");
            }
        });
}


// join modal

function openJoinModal(isPrivate, lobbyCode) {
    $('#joinLobbyModal').modal('show');
    document.getElementById('lobbyCodeInput').value = lobbyCode;

    // Afficher ou masquer le champ de mot de passe selon le type de lobby
    document.getElementById('joinPasswordField').classList.toggle('d-none', !isPrivate);
}

function joinLobby() {
    const playerName = document.getElementById('playerName').value;
    const lobbyCode = document.getElementById('lobbyCodeInput').value;
    const password = document.getElementById('joinLobbyPassword').value;

    $.post('/Lobby/JoinLobby', { lobbyCode, playerName, password })
        .done(response => {
            if (response.success) {
                window.location.href = response.redirectUrl;
            } else {
                alert(response.message);
            }
        });
}


// Rejoint un lobby (public ou privé)
function joinLobby(isPrivate) {
    const playerName = document.getElementById('playerName').value;
    const password = isPrivate ? document.getElementById('lobbyPassword').value : '';

    $.post('/Lobby/JoinLobby', { lobbyCode: 'LOBBY_CODE', playerName, password })
        .done(response => {
            if (response.success) {
                window.location.href = response.redirectUrl;
            } else {
                alert(response.message);
            }
        });
}


// Actualiser la liste de lobby

function loadLobbies() {
    $.getJSON('/Lobby/GetAvailableLobbies', function (data) {
        const lobbyList = $('#lobbyList');
        lobbyList.empty();
        data.forEach(lobby => {
            const joinButton = lobby.isPrivate
                ? `<button class="btn btn-sm btn-outline-secondary" onclick="openJoinModal(true, '${lobby.code}')">Rejoindre</button>`
                : `<button class="btn btn-sm btn-outline-primary" onclick="openJoinModal(false, '${lobby.code}')">Rejoindre</button>`;

            lobbyList.append(`<li class="list-group-item d-flex justify-content-between align-items-center">
                            ${lobby.name} (${lobby.code})
                            ${joinButton}
                          </li>`);
        });
    });
}

// Charge les lobbies toutes les 10 secondes
//setInterval(loadLobbies, 10000);


// recuperer les top joueurs
function loadTopPlayers() {
    $.getJSON('/Game/GetTopPlayers', function (data) {
        const playerList = $('#topPlayersList');
        playerList.empty();
        data.forEach((player, index) => {
            playerList.append(`<li class="list-group-item d-flex align-items-center">
                            <h5>${index + 1}</h5>
                            <img src="https://picsum.photos/50?random=${index + 1}" class="rounded-circle me-3 mx-2" width="50" height="50">
                            <div class="flex-grow-1">
                                <h6 class="mb-0">${player.username}</h6>
                            </div>
                            <span class="badge bg-outline-primary rounded-pill">${player.points}</span>
                          </li>`);
        });
    });
}
// Actualiser l’historique toutes les 30 secondes
//setInterval(loadTopPlayers, 30000);


