using UnityEngine;
using UnityEngine.UI;
using CogsTowerDefense.Cogs;

namespace CogsTowerDefense.UI
{
    /// <summary>
    /// Gère les clics sur les boutons UI pour ajouter des rouages au stockage
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class CogButtonUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CogStorageZone storageZone;
        [SerializeField] private CogData cogData;

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();

            // Trouve CogStorageZone si non assigné
            if (storageZone == null)
            {
                storageZone = FindFirstObjectByType<CogStorageZone>();
            }

            // Ajoute le listener
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClick);
            }
        }

        /// <summary>
        /// Appelé quand le bouton est cliqué
        /// </summary>
        private void OnButtonClick()
        {
            if (storageZone == null)
            {
                Debug.LogError("CogStorageZone not found!");
                return;
            }

            if (cogData == null)
            {
                Debug.LogError("CogData not assigned to button!");
                return;
            }

            // Ajoute le rouage à la zone de stockage
            if (storageZone.AddCog(cogData))
            {
                Debug.Log($"Button clicked: Added {cogData.CogName} to storage");
            }
            else
            {
                Debug.LogWarning($"Cannot add {cogData.CogName}: storage is full");
            }
        }

        private void OnDestroy()
        {
            // Nettoie le listener
            if (button != null)
            {
                button.onClick.RemoveListener(OnButtonClick);
            }
        }
    }
}
