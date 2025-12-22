using UnityEngine;
using UnityEngine.UI;
using CogsTowerDefense.Cogs;

namespace CogsTowerDefense.UI
{
    /// <summary>
    /// Gère les clics sur les boutons UI pour placer des rouages
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class CogButtonUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CogDragAndDrop dragAndDrop;
        [SerializeField] private CogData cogData;

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();

            // Trouve CogDragAndDrop si non assigné
            if (dragAndDrop == null)
            {
                dragAndDrop = FindFirstObjectByType<CogDragAndDrop>();
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
            if (dragAndDrop == null)
            {
                Debug.LogError("CogDragAndDrop not found!");
                return;
            }

            if (cogData == null)
            {
                Debug.LogError("CogData not assigned to button!");
                return;
            }

            // Démarre le drag and drop pour ce type de rouage
            dragAndDrop.StartDragNewCog(cogData);
            Debug.Log($"Button clicked: Starting placement of {cogData.CogName}");
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
