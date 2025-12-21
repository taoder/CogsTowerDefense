using UnityEngine;

namespace ClogTowerDefense.Grid
{
    /// <summary>
    /// Représente une cellule individuelle de la grille hexagonale
    /// Contient les informations sur l'occupation et les objets placés
    /// </summary>
    public class HexCell
    {
        public HexCoordinates Coordinates { get; private set; }
        public Vector3 WorldPosition { get; private set; }

        // État de la cellule
        public bool IsOccupied { get; private set; }
        public bool IsWalkable { get; set; }
        public bool IsPlaceable { get; set; }

        // Objet occupant la cellule (Cog, Engine, etc.)
        public GameObject OccupyingObject { get; private set; }

        // Référence vers la grille parente
        private HexGrid parentGrid;

        public HexCell(HexCoordinates coordinates, HexGrid parent, float hexSize = IsometricUtils.HEX_SIZE)
        {
            Coordinates = coordinates;
            parentGrid = parent;
            WorldPosition = IsometricUtils.HexToWorldPosition(coordinates, hexSize);

            IsOccupied = false;
            IsWalkable = true;
            IsPlaceable = true;
        }

        /// <summary>
        /// Place un objet dans cette cellule
        /// </summary>
        public bool PlaceObject(GameObject obj)
        {
            if (IsOccupied || !IsPlaceable)
            {
                Debug.LogWarning($"Cannot place object at {Coordinates}: Cell is occupied or not placeable");
                return false;
            }

            OccupyingObject = obj;
            IsOccupied = true;
            return true;
        }

        /// <summary>
        /// Retire l'objet de cette cellule
        /// </summary>
        public void ClearObject()
        {
            OccupyingObject = null;
            IsOccupied = false;
        }

        /// <summary>
        /// Retourne les cellules voisines
        /// </summary>
        public HexCell[] GetNeighborCells()
        {
            HexCoordinates[] neighborCoords = Coordinates.GetNeighbors();
            HexCell[] neighbors = new HexCell[6];

            for (int i = 0; i < 6; i++)
            {
                neighbors[i] = parentGrid.GetCell(neighborCoords[i]);
            }

            return neighbors;
        }

        /// <summary>
        /// Retourne une cellule voisine dans une direction spécifique (0-5)
        /// </summary>
        public HexCell GetNeighborCell(int direction)
        {
            HexCoordinates neighborCoord = Coordinates.GetNeighbor(direction);
            return parentGrid.GetCell(neighborCoord);
        }

        /// <summary>
        /// Calcule la distance vers une autre cellule
        /// </summary>
        public int DistanceTo(HexCell other)
        {
            return Coordinates.DistanceTo(other.Coordinates);
        }

        /// <summary>
        /// Vérifie si cette cellule est adjacente à une autre
        /// </summary>
        public bool IsAdjacentTo(HexCell other)
        {
            return DistanceTo(other) == 1;
        }

        public override string ToString()
        {
            return $"HexCell {Coordinates} - Occupied: {IsOccupied}";
        }
    }
}
