using System.Collections.Generic;
using UnityEngine;

namespace CogsTowerDefense.Grid
{
    /// <summary>
    /// Gestionnaire principal de la grille hexagonale isométrique
    /// Gère la création, l'accès et la visualisation de la grille
    /// </summary>
    public class HexGrid : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private int gridWidth = 15;
        [SerializeField] private int gridHeight = 10;
        [SerializeField] private float hexSize = 1.0f;
        [SerializeField] private bool pointyTopOrientation = false;

        [Header("Visual Settings")]
        [SerializeField] private bool showGrid = true;
        [SerializeField] private Color gridColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        [SerializeField] private Color occupiedColor = new Color(1f, 0f, 0f, 0.7f);
        [SerializeField] private Color placeableColor = new Color(0f, 1f, 0f, 0.3f);
        [SerializeField] private Color hoverColor = new Color(1f, 1f, 0f, 0.5f);

        // Stockage de la grille
        private Dictionary<HexCoordinates, HexCell> cells = new Dictionary<HexCoordinates, HexCell>();

        // Cellule actuellement survolée (pour le placement)
        private HexCell hoveredCell;

        public int Width => gridWidth;
        public int Height => gridHeight;
        public float HexSize => hexSize;

        private void Awake()
        {
            InitializeGrid();
        }

        /// <summary>
        /// Initialise toutes les cellules de la grille
        /// </summary>
        private void InitializeGrid()
        {
            cells.Clear();

            // Utilise un système offset pour créer une grille rectangulaire d'hexagones
            for (int row = 0; row < gridHeight; row++)
            {
                for (int col = 0; col < gridWidth; col++)
                {
                    HexCoordinates coords = HexCoordinates.FromOffset(col, row);
                    HexCell cell = new HexCell(coords, this, hexSize);
                    cells[coords] = cell;
                }
            }

            Debug.Log($"Hex Grid initialized: {cells.Count} cells ({gridWidth}x{gridHeight})");
        }

        /// <summary>
        /// Récupère une cellule à partir de coordonnées hexagonales
        /// </summary>
        public HexCell GetCell(HexCoordinates coords)
        {
            cells.TryGetValue(coords, out HexCell cell);
            return cell;
        }

        /// <summary>
        /// Récupère une cellule à partir de coordonnées offset
        /// </summary>
        public HexCell GetCellFromOffset(int col, int row)
        {
            HexCoordinates coords = HexCoordinates.FromOffset(col, row);
            return GetCell(coords);
        }

        /// <summary>
        /// Récupère la cellule la plus proche d'une position monde
        /// </summary>
        public HexCell GetCellAtWorldPosition(Vector3 worldPos)
        {
            HexCoordinates coords = IsometricUtils.WorldToHex(worldPos, hexSize);
            return GetCell(coords);
        }

        /// <summary>
        /// Vérifie si une cellule existe dans la grille
        /// </summary>
        public bool ContainsCell(HexCoordinates coords)
        {
            return cells.ContainsKey(coords);
        }

        /// <summary>
        /// Place un objet à des coordonnées spécifiques
        /// </summary>
        public bool PlaceObject(HexCoordinates coords, GameObject obj)
        {
            HexCell cell = GetCell(coords);
            if (cell == null)
            {
                Debug.LogWarning($"No cell found at coordinates {coords}");
                return false;
            }

            return cell.PlaceObject(obj);
        }

        /// <summary>
        /// Place un objet à une position monde
        /// </summary>
        public bool PlaceObjectAtWorldPosition(Vector3 worldPos, GameObject obj)
        {
            HexCell cell = GetCellAtWorldPosition(worldPos);
            if (cell == null) return false;

            return cell.PlaceObject(obj);
        }

        /// <summary>
        /// Retire un objet de la grille
        /// </summary>
        public void RemoveObject(HexCoordinates coords)
        {
            HexCell cell = GetCell(coords);
            cell?.ClearObject();
        }

        /// <summary>
        /// Retourne toutes les cellules de la grille
        /// </summary>
        public IEnumerable<HexCell> GetAllCells()
        {
            return cells.Values;
        }

        /// <summary>
        /// Retourne toutes les cellules dans un rayon donné autour d'un point
        /// </summary>
        public List<HexCell> GetCellsInRange(HexCoordinates center, int range)
        {
            List<HexCell> result = new List<HexCell>();

            for (int q = -range; q <= range; q++)
            {
                for (int r = Mathf.Max(-range, -q - range); r <= Mathf.Min(range, -q + range); r++)
                {
                    HexCoordinates coords = new HexCoordinates(center.Q + q, center.R + r);
                    HexCell cell = GetCell(coords);
                    if (cell != null)
                    {
                        result.Add(cell);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Trouve un chemin entre deux cellules (A* simplifié)
        /// </summary>
        public List<HexCell> FindPath(HexCell start, HexCell end)
        {
            // TODO: Implémenter A* pathfinding
            // Pour l'instant, retourne null
            return null;
        }

        /// <summary>
        /// Met à jour la cellule survolée (appelé par système de placement)
        /// </summary>
        public void SetHoveredCell(HexCell cell)
        {
            hoveredCell = cell;
        }

        /// <summary>
        /// Dessine la grille dans la scène (Gizmos)
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!showGrid) return;

            // Si la grille n'est pas initialisée, l'initialiser temporairement pour la preview
            if (cells.Count == 0)
            {
                InitializeGrid();
            }

            foreach (var cell in cells.Values)
            {
                Color cellColor = gridColor;

                // Coloration selon l'état de la cellule
                if (cell.IsOccupied)
                {
                    cellColor = occupiedColor;
                }
                else if (cell == hoveredCell)
                {
                    cellColor = hoverColor;
                }
                else if (cell.IsPlaceable)
                {
                    cellColor = placeableColor;
                }

                IsometricUtils.DrawHexGizmo(cell.Coordinates, cellColor, hexSize, pointyTopOrientation);

                // Dessine les coordonnées au centre (optionnel, pour debug)
                #if UNITY_EDITOR
                if (Application.isPlaying)
                {
                    UnityEditor.Handles.Label(
                        cell.WorldPosition,
                        $"{cell.Coordinates.Q},{cell.Coordinates.R}",
                        new GUIStyle() { normal = new GUIStyleState() { textColor = Color.white } }
                    );
                }
                #endif
            }
        }

        /// <summary>
        /// Réinitialise la grille (utile pour l'éditeur)
        /// </summary>
        [ContextMenu("Reinitialize Grid")]
        public void ReinitializeGrid()
        {
            InitializeGrid();
        }
    }
}
