using UnityEngine;
using UnityEngine.InputSystem;

namespace CogsTowerDefense.Cogs
{
    /// <summary>
    /// Système de placement de rouages avec placement libre
    /// Les rouages doivent se toucher physiquement pour se connecter
    /// </summary>
    public class CogPlacementSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CogChain cogChain;
        [SerializeField] private Camera mainCamera;

        [Header("Placement Settings")]
        [SerializeField] private CogData cogToPlace; // Le rouage actuellement sélectionné

        [Header("Visual Feedback")]
        [SerializeField] private Color validPlacementColor = new Color(0f, 1f, 0f, 0.5f);
        [SerializeField] private Color invalidPlacementColor = new Color(1f, 0f, 0f, 0.5f);

        // État interne
        private GameObject currentPreview;
        private Vector2 currentMouseWorldPos;
        private bool isPlacementMode = false;
        private bool canPlace = false;
        private LineRenderer previewCircle;

        private void Awake()
        {
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
            if (mainCamera == null || Mouse.current == null)
                return;

            // Récupère la position de la souris
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0));
            currentMouseWorldPos = new Vector2(worldPos.x, worldPos.y);

            // Update le preview
            UpdatePreview();

            // Placement au clic
            if (Mouse.current.leftButton.wasPressedThisFrame && canPlace)
            {
                PlaceCog();
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
        private void UpdatePreview()
        {
            if (cogToPlace == null)
            {
                HidePreview();
                return;
            }

            // Vérifie si on peut placer ici
            float cogRadius = GetRadiusForSize(cogToPlace.Size);
            canPlace = CanPlaceAt(currentMouseWorldPos, cogRadius, cogToPlace.GetPowerRequired(1));

            // Crée ou met à jour le cercle de preview
            if (previewCircle == null)
            {
                CreatePreviewCircle();
            }

            if (previewCircle != null)
            {
                previewCircle.transform.position = currentMouseWorldPos;

                // Met à jour le rayon du cercle
                UpdatePreviewCircleRadius(cogRadius);

                // Change la couleur selon la validité
                Color color = canPlace ? validPlacementColor : invalidPlacementColor;
                previewCircle.startColor = color;
                previewCircle.endColor = color;

                previewCircle.enabled = true;
            }
        }

        /// <summary>
        /// Crée le cercle de preview
        /// </summary>
        private void CreatePreviewCircle()
        {
            GameObject circleObj = new GameObject("PlacementPreviewCircle");
            circleObj.transform.SetParent(transform);

            previewCircle = circleObj.AddComponent<LineRenderer>();
            previewCircle.useWorldSpace = false;
            previewCircle.loop = true;
            previewCircle.widthMultiplier = 0.05f;
            previewCircle.positionCount = 64; // Nombre de segments pour un cercle lisse
            previewCircle.material = new Material(Shader.Find("Sprites/Default"));
            previewCircle.sortingOrder = 100;
        }

        /// <summary>
        /// Met à jour le rayon du cercle de preview
        /// </summary>
        private void UpdatePreviewCircleRadius(float radius)
        {
            if (previewCircle == null) return;

            int segments = previewCircle.positionCount;
            for (int i = 0; i < segments; i++)
            {
                float angle = (float)i / segments * 2f * Mathf.PI;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius,
                    0f
                );
                previewCircle.SetPosition(i, pos);
            }
        }

        /// <summary>
        /// Cache le preview
        /// </summary>
        private void HidePreview()
        {
            if (previewCircle != null)
            {
                previewCircle.enabled = false;
            }
        }

        /// <summary>
        /// Retourne le rayon pour une taille de rouage
        /// </summary>
        private float GetRadiusForSize(CogSize size)
        {
            return size switch
            {
                CogSize.Small => 0.5f,
                CogSize.Medium => 1.0f,
                CogSize.Large => 1.8f,
                _ => 0.5f
            };
        }

        /// <summary>
        /// Vérifie si on peut placer un rouage à cette position
        /// </summary>
        private bool CanPlaceAt(Vector2 position, float radius, int powerRequired)
        {
            if (cogToPlace == null) return false;

            // TODO: Vérifier que le joueur a assez d'or

            // Vérifie avec le système de chaîne
            if (cogChain != null)
            {
                return cogChain.CanPlaceCog(position, radius, powerRequired);
            }

            return true;
        }

        /// <summary>
        /// Place le rouage à la position donnée
        /// </summary>
        private void PlaceCog()
        {
            if (cogToPlace == null || cogToPlace.Prefab == null)
                return;

            // TODO: Vérifier et dépenser l'or

            // Instancie le rouage
            GameObject cogObj = Instantiate(cogToPlace.Prefab, currentMouseWorldPos, Quaternion.identity);
            Cog cog = cogObj.GetComponent<Cog>();

            if (cog != null)
            {
                // Enregistre dans la chaîne
                if (cogChain != null)
                {
                    cogChain.RegisterCog(cog);
                }

                Debug.Log($"Placed {cogToPlace.CogName} at {currentMouseWorldPos}");
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

            if (previewCircle != null)
            {
                Destroy(previewCircle.gameObject);
                previewCircle = null;
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
            if (previewCircle != null)
            {
                Destroy(previewCircle.gameObject);
                previewCircle = null;
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
