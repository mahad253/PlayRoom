using System;
using System.Collections.Generic;

public class LabyrinthGenerator
{
    // Méthode principale pour générer le labyrinthe
    public bool[,] GenerateLabyrinth(int rows, int cols)
    {
        // Initialisation de l'adjacence (tableau de booléens représentant la connectivité)
        bool[,] adjacency = new bool[rows * cols, rows * cols];

        // Liste des cellules visitées
        HashSet<int> visitedCells = new HashSet<int> { 1 };

        // Taille du labyrinthe
        int[] siz = { rows, cols };

        // Mélange des cellules
        List<int> cellOrder = new List<int>();
        for (int i = 0; i < rows * cols; i++)
        {
            cellOrder.Add(i);
        }
        ShuffleArray(cellOrder);

        // Génération du labyrinthe
        for (int cellCounter = 0; cellCounter < rows * cols; cellCounter++)
        {
            int currentCell = cellOrder[cellCounter];
            List<int> cellsInPath = new List<int> { currentCell };

            while (!visitedCells.Contains(currentCell))
            {
                int currentRow = currentCell / rows;
                int currentCol = currentCell % cols;
                List<int> candidates = new List<int>();

                // Vérification des voisins (haut, bas, gauche, droite)
                if (currentCol > 0) // Gauche
                {
                    int leftCell = currentCell - 1;
                    if (!cellsInPath.Contains(leftCell))
                        candidates.Add(leftCell);
                }
                if (currentCol < cols - 1) // Droite
                {
                    int rightCell = currentCell + 1;
                    if (!cellsInPath.Contains(rightCell))
                        candidates.Add(rightCell);
                }
                if (currentRow > 0) // Haut
                {
                    int aboveCell = currentCell - cols;
                    if (!cellsInPath.Contains(aboveCell))
                        candidates.Add(aboveCell);
                }
                if (currentRow < rows - 1) // Bas
                {
                    int belowCell = currentCell + cols;
                    if (!cellsInPath.Contains(belowCell))
                        candidates.Add(belowCell);
                }

                // Sélection du prochain voisin
                int nextCell = -1;
                if (candidates.Count > 0)
                {
                    ShuffleArray(candidates);
                    foreach (var candidate in candidates)
                    {
                        if (visitedCells.Contains(candidate))
                        {
                            if (new Random().NextDouble() < 0.05)
                            {
                                nextCell = candidate;
                                break;
                            }
                        }
                        else
                        {
                            nextCell = candidate;
                            break;
                        }
                    }
                    if (nextCell >= 0)
                    {
                        cellsInPath.Add(nextCell);
                        adjacency[nextCell, currentCell] = true;
                        adjacency[currentCell, nextCell] = true;
                        currentCell = nextCell;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            // Ajout des cellules du chemin à celles visitées
            foreach (var cell in cellsInPath)
            {
                visitedCells.Add(cell);
            }
        }

        // Vérification de la connectivité des cellules
        HashSet<int> visibleCells = CellsInCluster(adjacency, 1, rows, cols);
        for (int i = 0; i < rows * cols; i++)
        {
            if (!visibleCells.Contains(i))
            {
                adjacency = OpenCluster(adjacency, visibleCells, rows, cols);
                visibleCells = CellsInCluster(adjacency, i, rows, cols);
            }
        }

        return adjacency;
    }

    // Méthode pour obtenir les cellules visibles (connectées)
    private static HashSet<int> CellsInCluster(bool[,] adjacency, int startCell, int rows, int cols)
    {
        HashSet<int> cluster = new HashSet<int> { startCell };
        Stack<int> stack = new Stack<int>();
        stack.Push(startCell);

        while (stack.Count > 0)
        {
            int currentCell = stack.Pop();
            int currentRow = currentCell / rows;
            int currentCol = currentCell % cols;

            // Vérification des voisins (haut, bas, gauche, droite)
            foreach (var neighbor in GetNeighbors(currentCell, rows, cols))
            {
                if (adjacency[currentCell, neighbor] && !cluster.Contains(neighbor))
                {
                    cluster.Add(neighbor);
                    stack.Push(neighbor);
                }
            }
        }

        return cluster;
    }

    // Méthode pour obtenir les voisins d'une cellule
    private static List<int> GetNeighbors(int currentCell, int rows, int cols)
    {
        List<int> neighbors = new List<int>();
        int currentRow = currentCell / rows;
        int currentCol = currentCell % cols;

        if (currentCol > 0) neighbors.Add(currentCell - 1); // Gauche
        if (currentCol < cols - 1) neighbors.Add(currentCell + 1); // Droite
        if (currentRow > 0) neighbors.Add(currentCell - cols); // Haut
        if (currentRow < rows - 1) neighbors.Add(currentCell + cols); // Bas

        return neighbors;
    }

    // Méthode pour ouvrir un cluster et connecter les cellules isolées
    private static bool[,] OpenCluster(bool[,] adjacency, HashSet<int> clusterCells, int rows, int cols)
    {
        List<int> clusterCellsList = new List<int>(clusterCells);
        ShuffleArray(clusterCellsList); // Mélange aléatoire des cellules du cluster

        foreach (var currentCell in clusterCellsList)
        {
            int currentRow = currentCell / rows;
            int currentCol = currentCell % cols;
            List<int> candidates = new List<int>();

            // Vérification des voisins
            if (currentCol > 0) // Cellule à gauche
            {
                int leftCell = currentCell - 1;
                if (!clusterCells.Contains(leftCell))
                    candidates.Add(leftCell);
            }
            if (currentCol < cols - 1) // Cellule à droite
            {
                int rightCell = currentCell + 1;
                if (!clusterCells.Contains(rightCell))
                    candidates.Add(rightCell);
            }
            if (currentRow > 0) // Cellule au-dessus
            {
                int aboveCell = currentCell - cols;
                if (!clusterCells.Contains(aboveCell))
                    candidates.Add(aboveCell);
            }
            if (currentRow < rows - 1) // Cellule en-dessous
            {
                int belowCell = currentCell + cols;
                if (!clusterCells.Contains(belowCell))
                    candidates.Add(belowCell);
            }

            // Sélection d'un candidat parmi les voisins
            if (candidates.Count > 0)
            {
                ShuffleArray(candidates); // Mélange aléatoire des candidats

                // Vérification si le premier candidat est une cellule sur le bord
                if (IsEdgeCell(candidates[0], rows, cols))
                {
                    adjacency[candidates[0], currentCell] = true;
                    adjacency[currentCell, candidates[0]] = true;
                    break;
                }
                else if (candidates.Count > 1)
                {
                    adjacency[candidates[0], currentCell] = true;
                    adjacency[currentCell, candidates[0]] = true;
                    break;
                }
            }
        }

        return adjacency;
    }

    // Méthode pour vérifier si une cellule est située sur le bord du labyrinthe
    private static bool IsEdgeCell(int cell, int rows, int cols)
    {
        int cellRow = cell / rows;
        int cellCol = cell % cols;
        return cellRow == 0 || cellRow == rows - 1 || cellCol == 0 || cellCol == cols - 1;
    }

    // Méthode pour mélanger un tableau
    private static void ShuffleArray<T>(List<T> list)
    {
        Random rand = new Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rand.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
