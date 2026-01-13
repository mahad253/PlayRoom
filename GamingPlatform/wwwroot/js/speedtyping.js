// Variables
let demoText = "Ceci est un test de speed typing en local. Tapez le texte le plus rapidement possible sans erreur. La vitesse et la précision sont essentielles pour obtenir un bon score!";

let gameState = {
    isRunning: false,
    timeLeft: 60,
    timerInterval: null,
    currentDifficulty: 'easy',
    difficulties: {
        easy: 60,
        medium: 45,
        hard: 30
    },
    progress: 0,
};

// DOM Elements
const timerEl = document.getElementById('timer');
const textToTypeEl = document.getElementById('textToType');
const userInputEl = document.getElementById('userInput');
const wpmEl = document.getElementById('wpm');
const accuracyEl = document.getElementById('accuracy');
const scoreEl = document.getElementById('score');
const progressBarEl = document.getElementById('progressBar');
const progressTextEl = document.getElementById('progressText');
const btnStart = document.getElementById('btnStart');
const btnReset = document.getElementById('btnReset');
const btnLeaderboard = document.getElementById('btnLeaderboard');
const messageInput = document.getElementById('messageInput');
const btnSend = document.getElementById('btnSend');
const messagesBox = document.querySelector('.messages-box');
const resultsModal = document.getElementById('resultsModal');
const difficultyButtons = document.querySelectorAll(".diff-btn");




// Difficulty Buttons
let currentDifficulty = 1; // 1 = facile, 2 = moyen, 3 = difficile
difficultyButtons.forEach(btn => {
    btn.addEventListener("click", () => {
        difficultyButtons.forEach(b => b.classList.remove("active"));
        btn.classList.add("active");
        const diff = btn.dataset.diff; // easy/medium/hard
        if (diff === "easy") {
            currentDifficulty = 1;
            gameState.timeLeft = gameState.difficulties.easy
           
        }
        if (diff === "medium") {
            currentDifficulty = 2;
            gameState.timeLeft = gameState.difficulties.medium;
        }
        if (diff === "hard") {
            currentDifficulty = 3;
            gameState.timeLeft = gameState.difficulties.hard;
        }

        timerEl.textContent = gameState.timeLeft;
    });
});

// Start Game
btnStart.addEventListener("click", async () => {
    console.log("le jeux commence");
    // Appel API pour récupérer une phrase
    try {
        const response = await fetch(`/api/SpeedTyping/random?difficulty=${currentDifficulty}&language=FR`);
        if (!response.ok) {
            alert("Impossible de récupérer une phrase.");
            return;
        }

        const data = await response.json();

        // Injecter le texte dans l'UI
        textToTypeEl.innerText = data.text;
        userInputEl.value = "";
        userInputEl.disabled = false;
        userInputEl.focus();
        demoText = data.text;

        // TODO: ici tu peux démarrer le timer, reset progression, etc.
        startGame();
    } catch (e) {
        console.error(e);
        alert("Erreur lors du chargement de la phrase.");
    }
});

function startGame() {
    gameState.isRunning = true;
    gameState.timeLeft = gameState.difficulties[gameState.currentDifficulty];
    userInputEl.value = '';
    userInputEl.focus();
    btnStart.disabled = true;

    timerEl.textContent = gameState.timeLeft;
    renderText();

    gameState.timerInterval = setInterval(() => {
        gameState.timeLeft--;
        updateTimer();

        if (gameState.timeLeft <= 0 || gameState.progress === 100) {
            endGame();
        }
    }, 1000);
}

function updateTimer() {
    timerEl.textContent = gameState.timeLeft;
    if (gameState.timeLeft <= 10) {
        timerEl.classList.add('low');
    } else {
        timerEl.classList.remove('low');
    }
}

function renderText() {
    const typed = userInputEl.value;
    const chars = demoText.split('').map((char, i) => {
        let className = 'char upcoming';
        if (i < typed.length) {
            className = typed[i] === char ? 'char correct' : 'char incorrect';
        } else if (i === typed.length) {
            className = 'char current';
        }
        return `<span class="${className}">${char}</span>`;
    });
    textToTypeEl.innerHTML = chars.join('');
}

userInputEl.addEventListener('input', (e) => {
    console.log("bonjour");
    if (!gameState.isRunning) return;

    const typed = userInputEl.value;
    const target = demoText;


    // Render text with styling
    renderText();

    // Calculate progress
    const progress = Math.min((typed.length / target.length) * 100, 100);
    gameState.progress = progress;
    progressBarEl.style.width = progress + '%';
    progressTextEl.textContent = Math.floor(progress) + '%';

    // Calculate accuracy
    let correctChars = 0;
    for (let i = 0; i < Math.min(typed.length, target.length); i++) {
        if (typed[i] === target[i]) correctChars++;
    }
    const accuracy = typed.length === 0 ? 0 : (correctChars / typed.length) * 100;
    accuracyEl.textContent = Math.floor(accuracy) + '%';

    // Calculate WPM
    const minutesElapsed = (gameState.difficulties[gameState.currentDifficulty] - gameState.timeLeft) / 60 || 1/60;
    const words = typed.trim().split(/\s+/).filter(w => w).length;
    const wpm = Math.round(words / minutesElapsed);
    wpmEl.textContent = isFinite(wpm) ? wpm : 0;

    // Calculate Score
    const score = Math.round((wpm || 0) * (accuracy / 100));
    scoreEl.textContent = isFinite(score) ? score : 0;
});

function endGame() {
    clearInterval(gameState.timerInterval);
    gameState.isRunning = false;
    gameState.progress = 0;
    userInputEl.disabled = true;
    btnStart.disabled = false;

    // Show results modal
    document.getElementById('modalWpm').textContent = wpmEl.textContent;
    document.getElementById('modalAccuracy').textContent = accuracyEl.textContent;
    document.getElementById('modalScore').textContent = scoreEl.textContent;
    document.getElementById('modalChars').textContent = userInputEl.value.length;
    resultsModal.classList.add('show');
}

// Reset
btnReset.addEventListener('click', () => {
    clearInterval(gameState.timerInterval);
    gameState.isRunning = false;
    userInputEl.value = '';
    userInputEl.disabled = false;
    gameState.timeLeft = gameState.difficulties[gameState.currentDifficulty];
    timerEl.textContent = gameState.timeLeft;
    timerEl.classList.remove('low');
    wpmEl.textContent = '0';
    accuracyEl.textContent = '0%';
    scoreEl.textContent = '0';
    progressBarEl.style.width = '0%';
    progressTextEl.textContent = '0%';
    textToTypeEl.innerHTML = '<span class="char upcoming">Cliquez sur "Lancer la Partie" pour commencer...</span>';
    btnStart.disabled = false;
});

// Chat
btnSend.addEventListener('click', sendMessage);
messageInput.addEventListener('keypress', (e) => {
    if (e.key === 'Enter') sendMessage();
});

function sendMessage() {
    const text = messageInput.value.trim();
    if (!text) return;

    const msgDiv = document.createElement('div');
    msgDiv.className = 'message';
    msgDiv.innerHTML = `
                <div class="message-avatar">Y</div>
                <div class="message-content">
                    <div class="message-author">Vous</div>
                    <div class="message-text">${text}</div>
                </div>
            `;
    messagesBox.appendChild(msgDiv);
    messagesBox.scrollTop = messagesBox.scrollHeight;
    messageInput.value = '';
}

// Leaderboard
btnLeaderboard.addEventListener('click', () => {
    alert('🏆 Classement\n\n1. Alpha - 85 WPM (98% précision)\n2. Beta - 72 WPM (96% précision)\n3. Charlie - 68 WPM (94% précision)');
});