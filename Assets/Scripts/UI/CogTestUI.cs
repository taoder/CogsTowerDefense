using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CogsTowerDefense.Cogs;

namespace CogsTowerDefense.UI
{
    /// <summary>
    /// Interface utilisateur pour tester le système de rouages
    /// Affiche les infos du moteur et permet de le contrôler
    /// </summary>
    public class CogTestUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Engine engine;
        [SerializeField] private CogTestSetup testSetup;

        [Header("UI Elements")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button stopButton;
        [SerializeField] private Button upgradePowerButton;
        [SerializeField] private Button upgradeSpeedButton;

        [Header("Display")]
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI powerText;
        [SerializeField] private TextMeshProUGUI speedText;

        private void Start()
        {
            // Trouve les références si non assignées
            if (engine == null)
            {
                engine = FindFirstObjectByType<Engine>();
            }

            if (testSetup == null)
            {
                testSetup = FindFirstObjectByType<CogTestSetup>();
            }

            // Configure les boutons
            if (startButton != null)
            {
                startButton.onClick.AddListener(OnStartClicked);
            }

            if (stopButton != null)
            {
                stopButton.onClick.AddListener(OnStopClicked);
            }

            if (upgradePowerButton != null)
            {
                upgradePowerButton.onClick.AddListener(OnUpgradePowerClicked);
            }

            if (upgradeSpeedButton != null)
            {
                upgradeSpeedButton.onClick.AddListener(OnUpgradeSpeedClicked);
            }
        }

        private void Update()
        {
            // Met à jour l'affichage chaque frame
            UpdateDisplay();
        }

        /// <summary>
        /// Met à jour l'affichage des informations
        /// </summary>
        private void UpdateDisplay()
        {
            if (engine == null) return;

            // Status
            if (statusText != null)
            {
                string status = engine.IsRunning ? "RUNNING" : "STOPPED";
                PowerStatus powerStatus = engine.Status;

                Color statusColor = powerStatus switch
                {
                    PowerStatus.OK => Color.green,
                    PowerStatus.Warning => Color.yellow,
                    PowerStatus.Overload => Color.red,
                    _ => Color.white
                };

                statusText.text = $"Status: {status} ({powerStatus})";
                statusText.color = statusColor;
            }

            // Power
            if (powerText != null)
            {
                powerText.text = $"Power: {engine.CurrentPowerUsed}/{engine.MaxPower}";

                // Change la couleur selon le statut
                powerText.color = engine.Status switch
                {
                    PowerStatus.OK => Color.green,
                    PowerStatus.Warning => Color.yellow,
                    PowerStatus.Overload => Color.red,
                    _ => Color.white
                };
            }

            // Speed
            if (speedText != null)
            {
                speedText.text = $"Speed: {engine.RotationSpeed:F2}x";
            }
        }

        /// <summary>
        /// Démarre le moteur
        /// </summary>
        private void OnStartClicked()
        {
            if (testSetup != null)
            {
                testSetup.StartEngine();
            }
            else if (engine != null)
            {
                engine.StartEngine();
            }

            Debug.Log("Start button clicked");
        }

        /// <summary>
        /// Arrête le moteur
        /// </summary>
        private void OnStopClicked()
        {
            if (testSetup != null)
            {
                testSetup.StopEngine();
            }
            else if (engine != null)
            {
                engine.StopEngine();
            }

            Debug.Log("Stop button clicked");
        }

        /// <summary>
        /// Améliore la puissance du moteur
        /// </summary>
        private void OnUpgradePowerClicked()
        {
            if (testSetup != null)
            {
                testSetup.UpgradeEnginePower();
            }
            else if (engine != null)
            {
                engine.UpgradePower(5);
            }

            Debug.Log("Upgrade Power button clicked");
        }

        /// <summary>
        /// Améliore la vitesse du moteur
        /// </summary>
        private void OnUpgradeSpeedClicked()
        {
            if (testSetup != null)
            {
                testSetup.UpgradeEngineSpeed();
            }
            else if (engine != null)
            {
                engine.UpgradeSpeed(0.5f);
            }

            Debug.Log("Upgrade Speed button clicked");
        }

        private void OnDestroy()
        {
            // Nettoie les listeners
            if (startButton != null)
            {
                startButton.onClick.RemoveListener(OnStartClicked);
            }

            if (stopButton != null)
            {
                stopButton.onClick.RemoveListener(OnStopClicked);
            }

            if (upgradePowerButton != null)
            {
                upgradePowerButton.onClick.RemoveListener(OnUpgradePowerClicked);
            }

            if (upgradeSpeedButton != null)
            {
                upgradeSpeedButton.onClick.RemoveListener(OnUpgradeSpeedClicked);
            }
        }
    }
}
