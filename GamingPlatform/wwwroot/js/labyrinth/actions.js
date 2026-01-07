var connection = new signalR.HubConnectionBuilder().withUrl("/labyrinthHub").build();

 //Set up SignalR event listeners
connection.on("ReceivePlayerMovement", function (playerId, newCell) {
    if (playerId === 1) {
        player1Cell = newCell;
    } else if (playerId === 2) {
        player2Cell = newCell;
    }
    render();
});

connection.on("EndGame", function (message) {
    gameMessages.innerHTML = `<b><em>Game Over!</em></b> ${message}`;
});

// Start the SignalR connection
connection.start().catch(function (err) {
    return console.error("Error with SignalR:", err.toString());
});

// retrieve labyrinth
var nRows = 25;
var nCols = 25;



// Initialize array to count number of times each player stepped in each location
var floorTiles = Array(nRows * nCols).fill(0);

// Set player initial positions
var player1Cell = 0; // Start player 1 in the top left corner
var player2Cell = nCols - 1; // Start player 2 in the top right corner
floorTiles[player1Cell] = 1;
floorTiles[player2Cell] = 2;

// Define the destination cell at the bottom middle
var destinationCell = (nRows - 1) * nCols + Math.floor(nCols / 2);

// How big to draw the cells
var CELL_SIZE = 21;

// Set up keyboard event listener and keyCodes
window.addEventListener("keydown", keyboardHandler, false);
var UP = 38, DOWN = 40, RIGHT = 39, LEFT = 37; // Player 1 controls (arrow keys)
var W = 87, S = 83, D = 68, A = 65; // Player 2 controls (WASD keys)

// Set up HTML5 canvas
var canvas = document.querySelector("canvas");
var drawingSurface = canvas.getContext("2d");
canvas.width = nCols * CELL_SIZE;
canvas.height = nRows * CELL_SIZE;

// Set up paragraph to show game messages
var gameMessages = document.querySelector("#gameMessages");
gameMessages.innerHTML = "Utilise les flèches pour jouer.<br><strong>Course de labyrinthe</strong>: Atteignez la sortie vert foncée.";

// Set up timer
var timeLeft = 90;
var gameRunning = false;

// Draw starting situation
render();


// Helper function to handle movement for each player
function handlePlayerMovement(keyCode, playerCell, upKey, downKey, leftKey, rightKey, playerMark, updatePlayerCell) {
    let playerRow = Math.floor(playerCell / nRows);
    let playerCol = playerCell % nCols;
    let nextCell = -1;

    switch (keyCode) {
        case upKey:
            if (playerRow > 0) nextCell = playerCell - nCols;
            break;
        case downKey:
            if (playerRow < nRows - 1) nextCell = playerCell + nCols;
            break;
        case leftKey:
            if (playerCol > 0) nextCell = playerCell - 1;
            break;
        case rightKey:
            if (playerCol < nCols - 1) nextCell = playerCell + 1;
            break;
    }

    if (nextCell >= 0 && labyrinth[playerCell][nextCell]) {
        updatePlayerCell(nextCell);
        floorTiles[nextCell] = playerMark; // Mark cell with player number
    }
}

// End the game
function endGame() {
    window.removeEventListener("keydown", keyboardHandler, false);
    gameMessages.innerHTML = "<b><em>Game Over!</em></b> Les deux joueurs ont terminé le labyrinthe !";
}

// Draws the labyrinth walls and heatmap of player steps
function render() {
    if (!gameReady) return; // Ne rien faire tant que le labyrinthe n'est pas prêt

    drawingSurface.clearRect(0, 0, canvas.width, canvas.height);
    for (var cell = 0; cell < nRows * nCols; cell++) {
        var cellRow = Math.floor(cell / nRows);
        var cellCol = cell % nCols;

        if (cell == player1Cell)
            drawingSurface.fillStyle = "rgba(0, 0, 255, 0.7)"; // Player 1 color (Blue)
        else if (cell == player2Cell)
            drawingSurface.fillStyle = "rgba(255, 0, 0, 0.7)"; // Player 2 color (Red)
        else if (cell == destinationCell)
            drawingSurface.fillStyle = "rgba(0, 255, 0, 0.8)"; // Goal color (Green)
        else if (floorTiles[cell] === 1) {
            drawingSurface.fillStyle = "rgba(0, 0, 255, 0.3)"; // Path color for Player 1 (light blue)
        }
        else if (floorTiles[cell] === 2) {
            drawingSurface.fillStyle = "rgba(255, 0, 0, 0.3)"; // Path color for Player 2 (light red)
        } else
            drawingSurface.fillStyle = "rgba(255, 255, 255, 1)"; // Default empty tile

        drawingSurface.fillRect(cellCol * CELL_SIZE, cellRow * CELL_SIZE, CELL_SIZE, CELL_SIZE);
    }

    // Draw walls over heatmap
    drawingSurface.lineWidth = 2;
    for (var cell = 0; cell < nRows * nCols; cell++) {
        var cellRow = Math.floor(cell / nRows);
        var cellCol = cell % nCols;
        if (cellRow < nRows - 1 && !labyrinth[cell][cell + nCols]) {
            drawingSurface.beginPath();
            drawingSurface.moveTo(cellCol * CELL_SIZE, (cellRow + 1) * CELL_SIZE);
            drawingSurface.lineTo((cellCol + 1) * CELL_SIZE, (cellRow + 1) * CELL_SIZE);
            drawingSurface.stroke();
        }
        if (cellCol < nCols - 1 && !labyrinth[cell][cell + 1]) {
            drawingSurface.beginPath();
            drawingSurface.moveTo((cellCol + 1) * CELL_SIZE, cellRow * CELL_SIZE);
            drawingSurface.lineTo((cellCol + 1) * CELL_SIZE, (cellRow + 1) * CELL_SIZE);
            drawingSurface.stroke();
        }
    }
}