
document.addEventListener("DOMContentLoaded", function () {
const form = document.getElementById("configureGameForm");
const pseudoInput = document.getElementById("CreatorPseudo");
const categoriesContainer = document.getElementById("categoriesContainer");
const categoryCheckboxes = document.querySelectorAll(".category-checkbox");
const pseudoError = document.getElementById("pseudoError");
const categoryError = document.getElementById("categoryError");

form.addEventListener("submit", function (e) {
let isValid = true;

// Validation du pseudo
const pseudoValue = pseudoInput.value.trim();
if (pseudoValue === "") {
    pseudoError.classList.remove("d-none");
    pseudoError.textContent = "Le pseudo est obligatoire.";
    isValid = false;
} else if (pseudoValue.length < 5) {
    pseudoError.classList.remove("d-none");
    pseudoError.textContent = "Le pseudo doit contenir au moins 5 caractères.";
    isValid = false;
  } else if (!/^[a-zA-Z]+$/.test(pseudoValue)) {
// Vérifier si le pseudo contient uniquement des lettres
    pseudoError.textContent = 'Le pseudo ne peut contenir que des lettres de l\'alphabet.';
    pseudoError.classList.remove("d-none"); // Afficher l'erreur
    isValid = false;
} else {
    pseudoError.classList.add("d-none");
}



//Validation des lettres 
document.getElementById("configureGameForm").addEventListener("submit", function (event) {
    // Récupérer toutes les cases à cocher
    const letterCheckboxes = document.querySelectorAll("input[name='SelectedLetters']");
    const errorElement = document.getElementById("lettersError");

    // Vérifier si au moins une case est cochée
    const isChecked = Array.from(letterCheckboxes).some(checkbox => checkbox.checked);

    if (!isChecked) {
        // Afficher le message d'erreur et empêcher la soumission
        errorElement.classList.remove("d-none");
        errorElement.innerText = "Vous devez sélectionner au moins une lettre.";
        event.preventDefault();
    } else {
        // Masquer le message d'erreur si la validation est correcte
        errorElement.classList.add("d-none");
    }
});
   
// Validation des catégories
const selectedCategories = Array.from(categoryCheckboxes).filter(checkbox => checkbox.checked);
if (selectedCategories.length < 3) {
    categoryError.classList.remove("d-none");
    isValid = false;
} else {
    categoryError.classList.add("d-none");
}

// Empêche l'envoi si le formulaire n'est pas valide
if (!isValid) {
    e.preventDefault();
}
});
});

