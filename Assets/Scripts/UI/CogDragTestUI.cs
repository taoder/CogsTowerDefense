using UnityEngine;
using UnityEngine.UI;
using CogsTowerDefense.Cogs;

namespace CogsTowerDefense.UI
{
    /// <summary>
    /// Interface utilisateur simple pour tester le drag & drop de rouages
    /// Affiche des boutons pour chaque type de rouage
    /// </summary>
    public class CogDragTestUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CogDragAndDrop dragSystem;

        [Header("Cog Data")]
        [SerializeField] private CogData smallMeleeCog;
        [SerializeField] private CogData mediumRangedCog;
        [SerializeField] private CogData largeHeroCog;

        [Header("Buttons")]
        [SerializeField] private Button smallCogButton;
        [SerializeField] private Button mediumCogButton;
        [SerializeField] private Button largeCogButton;

        private void Start()
        {
            // Trouve la référence si non assignée
            if (dragSystem == null)
            {
                dragSystem = FindFirstObjectByType<CogDragAndDrop>();
            }

            // Configure les boutons
            if (smallCogButton != null)
            {
                smallCogButton.onClick.AddListener(() => OnCogButtonClicked(smallMeleeCog));
            }

            if (mediumCogButton != null)
            {
                mediumCogButton.onClick.AddListener(() => OnCogButtonClicked(mediumRangedCog));
            }

            if (largeCogButton != null)
            {
                largeCogButton.onClick.AddListener(() => OnCogButtonClicked(largeHeroCog));
            }
        }

        /// <summary>
        /// Démarre le drag d'un nouveau rouage
        /// </summary>
        private void OnCogButtonClicked(CogData cogData)
        {
            if (dragSystem == null)
            {
                Debug.LogError("CogDragAndDrop system not found!");
                return;
            }

            if (cogData == null)
            {
                Debug.LogError("CogData is null!");
                return;
            }

            dragSystem.StartDragNewCog(cogData);
            Debug.Log($"Started dragging {cogData.CogName}");
        }

        private void OnDestroy()
        {
            // Nettoie les listeners
            if (smallCogButton != null)
            {
                smallCogButton.onClick.RemoveAllListeners();
            }

            if (mediumCogButton != null)
            {
                mediumCogButton.onClick.RemoveAllListeners();
            }

            if (largeCogButton != null)
            {
                largeCogButton.onClick.RemoveAllListeners();
            }
        }
    }
}
