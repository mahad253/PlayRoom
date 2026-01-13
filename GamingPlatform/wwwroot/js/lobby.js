const lobbyCode = new URLSearchParams(window.location.search).get("code");
const nickname = localStorage.getItem("nickname") || "Player";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/lobbyHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

// événements reçus du hub
connection.on("PlayerJoined", (player) => {
    console.log("PlayerJoined", player);
    // TODO: mettre à jour la liste des joueurs dans le DOM
});

connection.on("GameStarted", () => {
    console.log("GameStarted");
    // TODO: rediriger vers la page SpeedTyping
    // window.location.href = `/SpeedTyping?code=${lobbyCode}`;
});

connection.on("ReceiveMessage", (message) => {
    console.log("ReceiveMessage", message);
    // TODO: ajouter le message au chat
});

async function start() {
    try {
        await connection.start();
        console.log("SignalR connected.");

        await connection.invoke("JoinLobby", lobbyCode, nickname);
    } catch (err) {
        console.error(err);
        setTimeout(start, 5000);
    }
}

connection.onclose(start);
start();
