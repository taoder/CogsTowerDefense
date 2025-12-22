using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CogsTowerDefense.Cogs;

namespace CogsTowerDefense.UI
{
    /// <summary>
    /// Contrôles UI pour démarrer/arrêter le moteur
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class EngineControlUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Engine engine;
        [SerializeField] private TextMeshProUGUI buttonText;

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();

            // Trouve le moteur si non assigné
            if (engine == null)
            {
                engine = FindFirstObjectByType<Engine>();
            }

            // Trouve le texte du bouton si non assigné
            if (buttonText == null)
            {
                buttonText = GetComponentInChildren<TextMeshProUGUI>();
            }

            // Ajoute le listener
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClick);
            }

            UpdateButtonText();
        }

        private void Update()
        {
            // Met à jour le texte si l'état du moteur change
            UpdateButtonText();
        }

        /// <summary>
        /// Appelé quand le bouton est cliqué
        /// </summary>
        private void OnButtonClick()
        {
            if (engine == null)
            {
                Debug.LogError("Engine not found!");
                return;
            }

            // Toggle l'état du moteur
            if (engine.IsRunning)
            {
                engine.StopEngine();
                Debug.Log("Engine stopped from UI");
            }
            else
            {
                engine.StartEngine();
                Debug.Log("Engine started from UI");
            }

            UpdateButtonText();
        }

        /// <summary>
        /// Met à jour le texte du bouton selon l'état du moteur
        /// </summary>
        private void UpdateButtonText()
        {
            if (buttonText == null || engine == null) return;

            buttonText.text = engine.IsRunning ? "Stop Engine" : "Start Engine";

            // Change la couleur du bouton
            if (button != null)
            {
                ColorBlock colors = button.colors;
                colors.normalColor = engine.IsRunning ? new Color(1f, 0.3f, 0.3f) : new Color(0.3f, 1f, 0.3f);
                button.colors = colors;
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
