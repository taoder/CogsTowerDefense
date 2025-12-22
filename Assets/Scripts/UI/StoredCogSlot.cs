using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using CogsTowerDefense.Cogs;
using CogsTowerDefense.Utils;

namespace CogsTowerDefense.UI
{
    /// <summary>
    /// Slot visuel dans la zone de stockage
    /// Affiche un rouage et permet de le drag & drop vers le terrain
    /// </summary>
    public class StoredCogSlot : MonoBehaviour, IPointerDownHandler
    {
        private int slotIndex;
        private CogStorageZone storageZone;
        private StoredCog storedCog;

        // Composants visuels
        private Image backgroundImage;
        private Image cogImage;
        private TextMeshProUGUI nameText;

        public void Initialize(int index, CogStorageZone zone)
        {
            slotIndex = index;
            storageZone = zone;

            CreateVisuals();
        }

        private void CreateVisuals()
        {
            // Background
            backgroundImage = gameObject.AddComponent<Image>();
            backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // Cog sprite (enfant)
            GameObject cogSpriteObj = new GameObject("CogSprite");
            cogSpriteObj.transform.SetParent(transform);

            RectTransform cogRect = cogSpriteObj.AddComponent<RectTransform>();
            cogRect.anchorMin = new Vector2(0.5f, 0.5f);
            cogRect.anchorMax = new Vector2(0.5f, 0.5f);
            cogRect.anchoredPosition = Vector2.zero;
            cogRect.sizeDelta = new Vector2(50f, 50f);

            cogImage = cogSpriteObj.AddComponent<Image>();
            cogImage.enabled = false;

            // Name text (enfant)
            GameObject textObj = new GameObject("NameText");
            textObj.transform.SetParent(transform);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 0.3f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = Vector2.zero;

            nameText = textObj.AddComponent<TextMeshProUGUI>();
            nameText.fontSize = 10;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color = Color.white;
            nameText.enabled = false;
        }

        /// <summary>
        /// Définit le rouage affiché dans ce slot
        /// </summary>
        public void SetCog(StoredCog cog)
        {
            storedCog = cog;

            if (cog != null && cog.cogData != null)
            {
                // Crée un sprite pour le rouage
                CreateCogSprite(cog.cogData);

                // Affiche le nom
                nameText.text = cog.cogData.CogName;
                nameText.enabled = true;

                cogImage.enabled = true;
            }
            else
            {
                ClearCog();
            }
        }

        /// <summary>
        /// Crée le sprite visuel pour le rouage
        /// </summary>
        private void CreateCogSprite(CogData cogData)
        {
            // Taille et couleur selon le type
            int spriteSize = cogData.Size switch
            {
                CogSize.Small => 32,
                CogSize.Medium => 40,
                CogSize.Large => 48,
                _ => 32
            };

            Color color = cogData.Size switch
            {
                CogSize.Small => new Color(0.2f, 0.8f, 1f),
                CogSize.Medium => new Color(0.2f, 1f, 0.2f),
                CogSize.Large => new Color(1f, 0.84f, 0.2f),
                _ => Color.white
            };

            int teeth = cogData.Size switch
            {
                CogSize.Small => 6,
                CogSize.Medium => 8,
                CogSize.Large => 12,
                _ => 6
            };

            Sprite sprite = GeometricSpriteGenerator.CreateGearSprite(spriteSize, color, teeth);
            cogImage.sprite = sprite;
        }

        /// <summary>
        /// Vide le slot
        /// </summary>
        public void ClearCog()
        {
            storedCog = null;
            cogImage.enabled = false;
            nameText.enabled = false;
        }

        /// <summary>
        /// Appelé quand on clique sur le slot
        /// </summary>
        public void OnPointerDown(PointerEventData eventData)
        {
            if (storedCog != null && storedCog.cogData != null)
            {
                // Démarre le drag & drop depuis le stockage
                var dragAndDrop = FindFirstObjectByType<CogDragAndDrop>();
                if (dragAndDrop != null)
                {
                    dragAndDrop.StartDragFromStorage(storedCog.cogData, slotIndex, storageZone);
                    Debug.Log($"Started drag from storage slot {slotIndex}");
                }
            }
        }
    }
}
