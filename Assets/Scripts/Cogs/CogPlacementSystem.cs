using UnityEngine;
using UnityEngine.InputSystem;
using CogsTowerDefense.Grid;

namespace CogsTowerDefense.Cogs
{
    /// <summary>
    /// Système de placement de rouages sur la grille hexagonale
    /// </summary>
    public class CogPlacementSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HexGrid hexGrid;
        [SerializeField] private CogChain cogChain;
        [SerializeField] private Camera mainCamera;

        [Header("Placement Settings")]
        [SerializeField] private CogData cogToPlace; // Le rouage actuellement sélectionné
        [SerializeField] private LayerMask placementLayerMask;

        [Header("Visual Feedback")]
        [SerializeField] private GameObject previewPrefab;
        [SerializeField] private Color validPlacementColor = new Color(0f, 1f, 0f, 0.5f);
        [SerializeField] private Color invalidPlacementColor = new Color(1f, 0f, 0f, 0.5f);

        // État interne
        private GameObject currentPreview;
        private HexCell hoveredCell;
        private bool isPlacementMode = false;
        private bool canPlace = false;

        private void Awake()
        {
            if (hexGrid == null)
            {
                hexGrid = FindFirstObjectByType<HexGrid>();
            }

            if (cogChain == null)
            {
                cogChain = FindFirstObjectByType<CogChain>();
            }

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }

        private void Update()
        {
            if (!isPlacementMode) return;

            HandlePlacementInput();
        }

        /// <summary>
        /// Gère les inputs pour le placement
        /// </summary>
        private void HandlePlacementInput()
        {
            if (mainCamera == null || hexGrid == null || Mouse.current == null)
                return;

            // Récupère la position de la souris
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0));
            worldPos.z = 0;

            // Récupère la cellule sous la souris
            HexCell cell = hexGrid.GetCellAtWorldPosition(worldPos);

            // Update le preview
            UpdatePreview(cell);

            // Placement au clic
            if (Mouse.current.leftButton.wasPressedThisFrame && canPlace)
            {
                PlaceCog(hoveredCell);
            }

            // Annulation au clic droit
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                CancelPlacement();
            }
        }

        /// <summary>
        /// Met à jour le preview de placement
        /// </summary>
        private void UpdatePreview(HexCell cell)
        {
            if (cell == hoveredCell) return;

            hoveredCell = cell;

            if (cell == null)
            {
                HidePreview();
                return;
            }

            // Vérifie si on peut placer ici
            canPlace = CanPlaceAt(cell);

            // Crée ou met à jour le preview
            if (currentPreview == null && cogToPlace != null && cogToPlace.Prefab != null)
            {
                currentPreview = Instantiate(cogToPlace.Prefab, transform);

                // Désactive les composants fonctionnels du preview
                Cog cogComponent = currentPreview.GetComponent<Cog>();
                if (cogComponent != null)
                {
                    cogComponent.enabled = false;
                }
            }

            if (currentPreview != null)
            {
                currentPreview.transform.position = cell.WorldPosition;

                // Change la couleur selon la validité
                SetPreviewColor(canPlace ? validPlacementColor : invalidPlacementColor);
            }
        }

        /// <summary>
        /// Cache le preview
        /// </summary>
        private void HidePreview()
        {
            if (currentPreview != null)
            {
                currentPreview.SetActive(false);
            }
        }

        /// <summary>
        /// Change la couleur du preview
        /// </summary>
        private void SetPreviewColor(Color color)
        {
            if (currentPreview == null) return;

            SpriteRenderer[] renderers = currentPreview.GetComponentsInChildren<SpriteRenderer>();
            foreach (var renderer in renderers)
            {
                renderer.color = color;
            }
        }

        /// <summary>
        /// Vérifie si on peut placer un rouage à cette cellule
        /// </summary>
        private bool CanPlaceAt(HexCell cell)
        {
            if (cell == null || cogToPlace == null) return false;

            // Vérifie que la cellule est libre
            if (cell.IsOccupied) return false;

            // Vérifie que la cellule est placeable
            if (!cell.IsPlaceable) return false;

            // TODO: Vérifier que le joueur a assez d'or

            // Vérifie avec le système de chaîne
            if (cogChain != null)
            {
                // Crée un rouage temporaire pour vérifier
                Cog tempCog = new GameObject("TempCog").AddComponent<Cog>();
                // TODO: Configurer le tempCog selon cogToPlace

                bool canPlace = cogChain.CanPlaceCog(cell.Coordinates, tempCog);

                Destroy(tempCog.gameObject);
                return canPlace;
            }

            return true;
        }

        /// <summary>
        /// Place le rouage à la position donnée
        /// </summary>
        private void PlaceCog(HexCell cell)
        {
            if (cell == null || cogToPlace == null || cogToPlace.Prefab == null)
                return;

            // TODO: Vérifier et dépenser l'or

            // Instancie le rouage
            GameObject cogObj = Instantiate(cogToPlace.Prefab, cell.WorldPosition, Quaternion.identity);
            Cog cog = cogObj.GetComponent<Cog>();

            if (cog != null)
            {
                // Enregistre dans la grille
                hexGrid.PlaceObject(cell.Coordinates, cogObj);

                // Enregistre dans la chaîne
                if (cogChain != null)
                {
                    cogChain.RegisterCog(cog, cell.Coordinates);
                }

                Debug.Log($"Placed {cogToPlace.CogName} at {cell.Coordinates}");
            }

            // Réinitialise pour le prochain placement
            canPlace = false;
        }

        /// <summary>
        /// Active le mode placement avec un type de rouage
        /// </summary>
        public void StartPlacement(CogData cogData)
        {
            cogToPlace = cogData;
            isPlacementMode = true;

            Debug.Log($"Started placement mode: {cogData.CogName}");
        }

        /// <summary>
        /// Annule le mode placement
        /// </summary>
        public void CancelPlacement()
        {
            isPlacementMode = false;
            cogToPlace = null;

            if (currentPreview != null)
            {
                Destroy(currentPreview);
                currentPreview = null;
            }

            Debug.Log("Cancelled placement mode");
        }

        /// <summary>
        /// Change le type de rouage à placer
        /// </summary>
        public void SetCogToPlace(CogData cogData)
        {
            cogToPlace = cogData;

            // Détruit l'ancien preview
            if (currentPreview != null)
            {
                Destroy(currentPreview);
                currentPreview = null;
            }
        }

        /// <summary>
        /// Vérifie si on est en mode placement
        /// </summary>
        public bool IsPlacementMode()
        {
            return isPlacementMode;
        }
    }
}
