using UnityEngine;
using UnityEngine.InputSystem;

namespace CogsTowerDefense.Grid
{
    /// <summary>
    /// Script de test pour la grille hexagonale
    /// Permet de cliquer sur la grille et placer des objets de test
    /// Utilise le nouveau Input System (UnityEngine.InputSystem)
    /// </summary>
    public class HexGridTester : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HexGrid hexGrid;
        [SerializeField] private HexGridRenderer gridRenderer;

        [Header("Test Object")]
        [SerializeField] private GameObject testObjectPrefab;
        [SerializeField] private bool spawnOnClick = true;

        private Camera mainCamera;
        private HexCell currentHoveredCell;

        private void Start()
        {
            mainCamera = Camera.main;

            if (hexGrid == null)
            {
                hexGrid = FindObjectOfType<HexGrid>();
            }

            if (gridRenderer == null)
            {
                gridRenderer = FindObjectOfType<HexGridRenderer>();
            }

            // Crée un prefab de test simple si non assigné
            if (testObjectPrefab == null)
            {
                testObjectPrefab = CreateDefaultTestObject();
            }
        }

        private void Update()
        {
            HandleMouseInput();
        }

        /// <summary>
        /// Gère les inputs souris pour le hover et le placement
        /// Utilise le nouveau Input System
        /// </summary>
        private void HandleMouseInput()
        {
            if (mainCamera == null || hexGrid == null) return;

            // Vérification que la souris est disponible
            if (Mouse.current == null) return;

            // Convertit la position souris en position monde
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0));
            mouseWorldPos.z = 0; // Assure que Z est 0 pour la 2D

            // Récupère la cellule sous la souris
            HexCell hoveredCell = hexGrid.GetCellAtWorldPosition(mouseWorldPos);

            // Update le highlight
            if (hoveredCell != currentHoveredCell)
            {
                currentHoveredCell = hoveredCell;
                hexGrid.SetHoveredCell(hoveredCell);

                if (gridRenderer != null)
                {
                    if (hoveredCell != null)
                    {
                        Color highlightColor = hoveredCell.IsPlaceable && !hoveredCell.IsOccupied
                            ? Color.green
                            : Color.red;
                        gridRenderer.HighlightCell(hoveredCell, highlightColor);
                    }
                    else
                    {
                        gridRenderer.ClearHighlight();
                    }
                }
            }

            // Placement au clic gauche
            if (spawnOnClick && Mouse.current.leftButton.wasPressedThisFrame)
            {
                PlaceTestObject(hoveredCell);
            }

            // Suppression au clic droit
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                RemoveTestObject(hoveredCell);
            }
        }

        /// <summary>
        /// Place un objet de test sur une cellule
        /// </summary>
        private void PlaceTestObject(HexCell cell)
        {
            if (cell == null || cell.IsOccupied || !cell.IsPlaceable)
            {
                Debug.Log($"Cannot place object: cell {(cell == null ? "null" : cell.Coordinates.ToString())}");
                return;
            }

            GameObject obj = Instantiate(testObjectPrefab, cell.WorldPosition, Quaternion.identity);
            obj.name = $"TestObject_{cell.Coordinates}";

            if (hexGrid.PlaceObject(cell.Coordinates, obj))
            {
                Debug.Log($"Placed object at {cell.Coordinates}");
            }
            else
            {
                Destroy(obj);
                Debug.LogWarning($"Failed to place object at {cell.Coordinates}");
            }
        }

        /// <summary>
        /// Retire un objet de test d'une cellule
        /// </summary>
        private void RemoveTestObject(HexCell cell)
        {
            if (cell == null || !cell.IsOccupied) return;

            GameObject obj = cell.OccupyingObject;
            hexGrid.RemoveObject(cell.Coordinates);

            if (obj != null)
            {
                Destroy(obj);
                Debug.Log($"Removed object from {cell.Coordinates}");
            }
        }

        /// <summary>
        /// Crée un prefab de test simple
        /// </summary>
        private GameObject CreateDefaultTestObject()
        {
            GameObject obj = new GameObject("TestObject");

            // Ajoute un sprite renderer avec un carré coloré
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSquareSprite(Color.cyan);
            sr.sortingLayerName = "Cogs";
            sr.sortingOrder = 10;

            // Ajoute un tag pour identification
            obj.tag = "Untagged";

            return obj;
        }

        /// <summary>
        /// Crée un sprite carré simple
        /// </summary>
        private Sprite CreateSquareSprite(Color color)
        {
            Texture2D tex = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            tex.SetPixels(pixels);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
        }

        /// <summary>
        /// Debug : Affiche les voisins de la cellule survolée
        /// </summary>
        [ContextMenu("Debug Show Neighbors")]
        public void DebugShowNeighbors()
        {
            if (currentHoveredCell == null)
            {
                Debug.Log("No cell hovered");
                return;
            }

            HexCell[] neighbors = currentHoveredCell.GetNeighborCells();
            Debug.Log($"Neighbors of {currentHoveredCell.Coordinates}:");

            for (int i = 0; i < neighbors.Length; i++)
            {
                if (neighbors[i] != null)
                {
                    Debug.Log($"  Direction {i}: {neighbors[i].Coordinates}");
                }
            }
        }

        private void OnDrawGizmos()
        {
            // Dessine une ligne depuis la cellule survolée vers ses voisins (debug)
            if (currentHoveredCell != null && Application.isPlaying)
            {
                HexCell[] neighbors = currentHoveredCell.GetNeighborCells();
                foreach (var neighbor in neighbors)
                {
                    if (neighbor != null)
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawLine(currentHoveredCell.WorldPosition, neighbor.WorldPosition);
                    }
                }
            }
        }
    }
}
