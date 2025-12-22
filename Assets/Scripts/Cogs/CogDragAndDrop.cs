using UnityEngine;
using UnityEngine.InputSystem;
using CogsTowerDefense.UI;

namespace CogsTowerDefense.Cogs
{
    /// <summary>
    /// Système de drag & drop pour placer et déplacer les rouages
    /// Preview circulaire avec code couleur (vert/rouge)
    /// </summary>
    public class CogDragAndDrop : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CogChain cogChain;
        [SerializeField] private Camera mainCamera;

        [Header("Drag Settings")]
        [SerializeField] private float dragStartThreshold = 0.1f; // Distance minimum pour commencer un drag

        [Header("Visual Feedback")]
        [SerializeField] private Color validPlacementColor = new Color(0f, 1f, 0f, 0.5f);
        [SerializeField] private Color invalidPlacementColor = new Color(1f, 0f, 0f, 0.5f);
        [SerializeField] private Color deleteHighlightColor = new Color(1f, 0.5f, 0f, 0.7f); // Orange pour suppression

        // État du drag
        private enum DragState { None, NewCog, ExistingCog }
        private DragState currentDragState = DragState.None;

        // Pour nouveau rouage
        private CogData cogToPlace;
        private GameObject draggedCogPreview;
        private float draggedCogRadius;
        private int draggedCogPowerRequired;

        // Pour rouage existant
        private Cog draggedExistingCog;
        private Vector2 draggedCogOriginalPosition;

        // Preview circulaire
        private LineRenderer previewCircle;
        private Vector2 currentMouseWorldPos;
        private Vector2 dragStartPos;
        private bool canPlace = false;

        // Pour la suppression
        private Cog cogToDelete;
        private LineRenderer deleteHighlight;

        // Pour le drag depuis le stockage
        private bool dragFromStorage = false;
        private int storageSlotIndex = -1;
        private CogStorageZone sourceStorage = null;

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
            if (Mouse.current == null) return;

            UpdateMousePosition();

            // Gère le drag en cours
            if (currentDragState != DragState.None)
            {
                UpdateDrag();

                // Fin du drag
                if (Mouse.current.leftButton.wasReleasedThisFrame)
                {
                    EndDrag();
                }

                // Annulation avec clic droit
                if (Mouse.current.rightButton.wasPressedThisFrame)
                {
                    CancelDrag();
                }
            }
            else
            {
                // Détecte le début d'un drag sur un rouage existant
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    TryStartDragExistingCog();
                }

                // Suppression avec touche Delete ou Backspace
                if (Keyboard.current != null &&
                    (Keyboard.current.deleteKey.wasPressedThisFrame ||
                     Keyboard.current.backspaceKey.wasPressedThisFrame))
                {
                    TryDeleteCogAtMouse();
                }

                // Highlight du rouage sous la souris pour suppression
                UpdateDeleteHighlight();
            }
        }

        /// <summary>
        /// Met à jour la position de la souris en world space
        /// </summary>
        private void UpdateMousePosition()
        {
            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0));
            currentMouseWorldPos = new Vector2(worldPos.x, worldPos.y);
        }

        /// <summary>
        /// Démarre le drag d'un nouveau rouage depuis un bouton UI
        /// </summary>
        public void StartDragNewCog(CogData cogData)
        {
            if (cogData == null) return;

            cogToPlace = cogData;
            currentDragState = DragState.NewCog;
            dragStartPos = currentMouseWorldPos;

            // Calcule les propriétés du rouage
            // Utilise le rayon avec dents pour le preview
            draggedCogRadius = GetToothRadiusForSize(cogData.Size);
            draggedCogPowerRequired = cogData.GetPowerRequired(1);

            // Crée le preview visuel (optionnel, on peut juste utiliser le cercle)
            CreatePreviewCircle();

            Debug.Log($"Started dragging new cog: {cogData.CogName}");
        }

        /// <summary>
        /// Démarre le drag d'un rouage depuis la zone de stockage
        /// </summary>
        public void StartDragFromStorage(CogData cogData, int slotIndex, CogStorageZone storage)
        {
            if (cogData == null || storage == null) return;

            // Marque que ce drag vient du stockage
            dragFromStorage = true;
            storageSlotIndex = slotIndex;
            sourceStorage = storage;

            // Utilise la même logique que StartDragNewCog
            cogToPlace = cogData;
            currentDragState = DragState.NewCog;
            dragStartPos = currentMouseWorldPos;

            // Calcule les propriétés du rouage
            draggedCogRadius = GetToothRadiusForSize(cogData.Size);
            draggedCogPowerRequired = cogData.GetPowerRequired(1);

            // Crée le preview visuel
            CreatePreviewCircle();

            Debug.Log($"Started dragging cog from storage slot {slotIndex}: {cogData.CogName}");
        }

        /// <summary>
        /// Tente de commencer à déplacer un rouage existant
        /// </summary>
        private void TryStartDragExistingCog()
        {
            // Raycaste pour trouver un rouage sous la souris
            Cog cogUnderMouse = GetCogAtPosition(currentMouseWorldPos);

            if (cogUnderMouse != null)
            {
                // Commence le drag
                currentDragState = DragState.ExistingCog;
                draggedExistingCog = cogUnderMouse;
                draggedCogOriginalPosition = cogUnderMouse.Position;
                dragStartPos = currentMouseWorldPos;

                // Désenregistre temporairement du cogChain
                if (cogChain != null)
                {
                    cogChain.UnregisterCog(draggedExistingCog);
                }

                // Propriétés du rouage
                // Utilise le rayon avec dents pour le preview
                draggedCogRadius = draggedExistingCog.GetToothRadius();
                draggedCogPowerRequired = draggedExistingCog.PowerRequired;

                CreatePreviewCircle();

                Debug.Log($"Started dragging existing cog at {draggedCogOriginalPosition}");
            }
        }

        /// <summary>
        /// Trouve le rouage à une position donnée
        /// Utilise le rayon avec dents pour une détection précise
        /// </summary>
        private Cog GetCogAtPosition(Vector2 position)
        {
            // Cherche tous les rouages dans la scène
            Cog[] allCogs = FindObjectsByType<Cog>(FindObjectsSortMode.None);

            foreach (var cog in allCogs)
            {
                float distance = Vector2.Distance(position, cog.Position);
                // Utilise le rayon avec dents pour cliquer sur toute la roue
                float clickRadius = cog.GetToothRadius();
                if (distance <= clickRadius)
                {
                    return cog;
                }
            }

            return null;
        }

        /// <summary>
        /// Met à jour le drag en cours
        /// </summary>
        private void UpdateDrag()
        {
            // Met à jour la position du rouage dragué
            if (currentDragState == DragState.ExistingCog && draggedExistingCog != null)
            {
                draggedExistingCog.transform.position = currentMouseWorldPos;
            }

            // Vérifie si le placement est valide
            canPlace = CanPlaceAt(currentMouseWorldPos, draggedCogRadius, draggedCogPowerRequired);

            // Met à jour le preview
            UpdatePreviewCircle(currentMouseWorldPos, draggedCogRadius, canPlace);
        }

        /// <summary>
        /// Termine le drag
        /// </summary>
        private void EndDrag()
        {
            if (currentDragState == DragState.NewCog && canPlace)
            {
                // Place le nouveau rouage
                PlaceNewCog();
            }
            else if (currentDragState == DragState.ExistingCog && canPlace)
            {
                // Replace le rouage existant
                ReplaceExistingCog();
            }
            else
            {
                // Placement invalide, annule
                CancelDrag();
                return;
            }

            // Nettoie
            CleanupDrag();
        }

        /// <summary>
        /// Annule le drag
        /// </summary>
        private void CancelDrag()
        {
            if (currentDragState == DragState.ExistingCog && draggedExistingCog != null)
            {
                // Remet le rouage à sa position d'origine
                draggedExistingCog.transform.position = draggedCogOriginalPosition;

                // Réenregistre dans le cogChain
                if (cogChain != null)
                {
                    cogChain.RegisterCog(draggedExistingCog);
                }

                Debug.Log($"Cancelled drag, cog returned to {draggedCogOriginalPosition}");
            }

            CleanupDrag();
        }

        /// <summary>
        /// Place un nouveau rouage
        /// </summary>
        private void PlaceNewCog()
        {
            if (cogToPlace == null || cogToPlace.Prefab == null) return;

            // TODO: Vérifier et dépenser l'or

            // Instancie le rouage
            GameObject cogObj = Instantiate(cogToPlace.Prefab, currentMouseWorldPos, Quaternion.identity);
            Cog cog = cogObj.GetComponent<Cog>();

            if (cog != null && cogChain != null)
            {
                cogChain.RegisterCog(cog);
                Debug.Log($"Placed new {cogToPlace.CogName} at {currentMouseWorldPos}");

                // Si le rouage vient du stockage, le retire du stockage
                if (dragFromStorage && sourceStorage != null && storageSlotIndex >= 0)
                {
                    sourceStorage.RemoveCog(storageSlotIndex);
                    Debug.Log($"Removed cog from storage slot {storageSlotIndex}");
                }
            }
        }

        /// <summary>
        /// Replace un rouage existant
        /// </summary>
        private void ReplaceExistingCog()
        {
            if (draggedExistingCog == null) return;

            // Le rouage est déjà à la bonne position (on l'a déplacé pendant le drag)
            // Il suffit de le réenregistrer
            if (cogChain != null)
            {
                cogChain.RegisterCog(draggedExistingCog);
                Debug.Log($"Moved cog from {draggedCogOriginalPosition} to {currentMouseWorldPos}");
            }
        }

        /// <summary>
        /// Nettoie après un drag
        /// </summary>
        private void CleanupDrag()
        {
            currentDragState = DragState.None;
            cogToPlace = null;
            draggedExistingCog = null;

            // Réinitialise les flags de stockage
            dragFromStorage = false;
            storageSlotIndex = -1;
            sourceStorage = null;

            if (previewCircle != null)
            {
                Destroy(previewCircle.gameObject);
                previewCircle = null;
            }

            if (draggedCogPreview != null)
            {
                Destroy(draggedCogPreview);
                draggedCogPreview = null;
            }

            // Nettoie aussi le highlight de suppression
            CleanupDeleteHighlight();
        }

        /// <summary>
        /// Crée le cercle de preview
        /// </summary>
        private void CreatePreviewCircle()
        {
            GameObject circleObj = new GameObject("DragPreviewCircle");
            circleObj.transform.SetParent(transform);

            previewCircle = circleObj.AddComponent<LineRenderer>();
            previewCircle.useWorldSpace = true;
            previewCircle.loop = true;
            previewCircle.widthMultiplier = 0.05f;
            previewCircle.positionCount = 64;
            previewCircle.material = new Material(Shader.Find("Sprites/Default"));
            previewCircle.sortingOrder = 100;
        }

        /// <summary>
        /// Met à jour le cercle de preview
        /// </summary>
        private void UpdatePreviewCircle(Vector2 position, float radius, bool isValid)
        {
            if (previewCircle == null) return;

            // Met à jour la couleur
            Color color = isValid ? validPlacementColor : invalidPlacementColor;
            previewCircle.startColor = color;
            previewCircle.endColor = color;

            // Met à jour les positions
            int segments = previewCircle.positionCount;
            for (int i = 0; i < segments; i++)
            {
                float angle = (float)i / segments * 2f * Mathf.PI;
                Vector3 pos = new Vector3(
                    position.x + Mathf.Cos(angle) * radius,
                    position.y + Mathf.Sin(angle) * radius,
                    0f
                );
                previewCircle.SetPosition(i, pos);
            }

            previewCircle.enabled = true;
        }

        /// <summary>
        /// Vérifie si on peut placer à cette position
        /// </summary>
        private bool CanPlaceAt(Vector2 position, float radius, int powerRequired)
        {
            if (cogChain == null) return false;

            // Si on déplace un rouage existant, on ignore la collision avec lui-même
            // (déjà géré car on l'a désenregistré)

            return cogChain.CanPlaceCog(position, radius, powerRequired);
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
        /// Retourne le rayon avec dents pour une taille de rouage
        /// </summary>
        private float GetToothRadiusForSize(CogSize size)
        {
            float baseRadius = GetRadiusForSize(size);
            float scale = size switch
            {
                CogSize.Small => 1.0f,
                CogSize.Medium => 2.0f,
                CogSize.Large => 3.6f,
                _ => 1.0f
            };

            float spriteBaseToothHeight = 8f / 64f; // 0.125 unités
            float visualBaseRadius = baseRadius * 1.4f; // baseRadiusRatio = 0.7
            return visualBaseRadius + (spriteBaseToothHeight * scale);
        }

        /// <summary>
        /// Tente de supprimer le rouage sous la souris
        /// </summary>
        private void TryDeleteCogAtMouse()
        {
            if (currentDragState != DragState.None) return; // Pas pendant un drag

            Cog cogUnderMouse = GetCogAtPosition(currentMouseWorldPos);

            if (cogUnderMouse != null)
            {
                DeleteCog(cogUnderMouse);
            }
        }

        /// <summary>
        /// Supprime un rouage
        /// </summary>
        private void DeleteCog(Cog cog)
        {
            if (cog == null) return;

            // Désenregistre du cogChain
            if (cogChain != null)
            {
                cogChain.UnregisterCog(cog);
            }

            // TODO: Rembourser une partie de l'or

            Debug.Log($"Deleted cog at {cog.Position}");

            // Nettoie le highlight si c'était ce rouage
            if (cogToDelete == cog)
            {
                CleanupDeleteHighlight();
            }

            Destroy(cog.gameObject);
        }

        /// <summary>
        /// Met à jour le highlight de suppression
        /// </summary>
        private void UpdateDeleteHighlight()
        {
            if (currentDragState != DragState.None)
            {
                CleanupDeleteHighlight();
                return;
            }

            // Trouve le rouage sous la souris
            Cog cogUnderMouse = GetCogAtPosition(currentMouseWorldPos);

            if (cogUnderMouse != cogToDelete)
            {
                // Le rouage a changé
                CleanupDeleteHighlight();
                cogToDelete = cogUnderMouse;

                if (cogToDelete != null)
                {
                    CreateDeleteHighlight(cogToDelete);
                }
            }
            else if (cogToDelete != null && deleteHighlight != null)
            {
                // Met à jour le highlight existant
                UpdateDeleteHighlightCircle(cogToDelete.Position, cogToDelete.GetToothRadius());
            }
        }

        /// <summary>
        /// Crée le highlight de suppression
        /// </summary>
        private void CreateDeleteHighlight(Cog cog)
        {
            GameObject highlightObj = new GameObject("DeleteHighlight");
            highlightObj.transform.SetParent(transform);

            deleteHighlight = highlightObj.AddComponent<LineRenderer>();
            deleteHighlight.useWorldSpace = true;
            deleteHighlight.loop = true;
            deleteHighlight.widthMultiplier = 0.08f; // Plus épais que le preview de drag
            deleteHighlight.positionCount = 64;
            deleteHighlight.material = new Material(Shader.Find("Sprites/Default"));
            deleteHighlight.sortingOrder = 99;

            UpdateDeleteHighlightCircle(cog.Position, cog.GetToothRadius());
        }

        /// <summary>
        /// Met à jour le cercle de highlight de suppression
        /// </summary>
        private void UpdateDeleteHighlightCircle(Vector2 position, float radius)
        {
            if (deleteHighlight == null) return;

            // Couleur orange
            deleteHighlight.startColor = deleteHighlightColor;
            deleteHighlight.endColor = deleteHighlightColor;

            // Met à jour les positions
            int segments = deleteHighlight.positionCount;
            for (int i = 0; i < segments; i++)
            {
                float angle = (float)i / segments * 2f * Mathf.PI;
                Vector3 pos = new Vector3(
                    position.x + Mathf.Cos(angle) * radius,
                    position.y + Mathf.Sin(angle) * radius,
                    0f
                );
                deleteHighlight.SetPosition(i, pos);
            }

            deleteHighlight.enabled = true;
        }

        /// <summary>
        /// Nettoie le highlight de suppression
        /// </summary>
        private void CleanupDeleteHighlight()
        {
            if (deleteHighlight != null)
            {
                Destroy(deleteHighlight.gameObject);
                deleteHighlight = null;
            }
            cogToDelete = null;
        }

        /// <summary>
        /// Active le mode placement (pour compatibilité avec l'ancien système)
        /// </summary>
        public void StartPlacement(CogData cogData)
        {
            StartDragNewCog(cogData);
        }

        /// <summary>
        /// Annule le mode placement
        /// </summary>
        public void CancelPlacement()
        {
            CancelDrag();
        }
    }
}
