using System.Collections.Generic;
using UnityEngine;

namespace CogsTowerDefense.Cogs
{
    /// <summary>
    /// Moteur central qui fournit la puissance et fait tourner les rouages connectés
    /// Le moteur est un rouage comme les autres, de taille Small (rayon 0.5)
    /// </summary>
    public class Engine : MonoBehaviour
    {
        [Header("Engine Stats")]
        [SerializeField] private int maxPower = 10;
        [SerializeField] private float rotationSpeed = 1.0f; // Rotations par seconde

        [Header("Upgrade Costs")]
        [SerializeField] private int powerUpgradeCost = 100;
        [SerializeField] private int speedUpgradeCost = 150;

        [Header("Visual Feedback")]
        [SerializeField] private Color okColor = Color.green;
        [SerializeField] private Color warningColor = Color.yellow;
        [SerializeField] private Color overloadColor = Color.red;

        // Le moteur est un rouage de taille Small
        private const float ENGINE_RADIUS = 0.5f;
        private const float ENGINE_SPRITE_SCALE = 1.0f; // Même scale qu'un Small cog

        // État interne
        private List<Cog> connectedCogs = new List<Cog>();
        private int currentPowerUsed = 0;
        private PowerStatus currentStatus = PowerStatus.OK;
        private bool isRunning = false;

        // Propriétés publiques
        public int MaxPower => maxPower;
        public float RotationSpeed => rotationSpeed;
        public int CurrentPowerUsed => currentPowerUsed;
        public PowerStatus Status => currentStatus;
        public bool IsRunning => isRunning;
        public Vector2 Position => transform.position;
        public float Radius => ENGINE_RADIUS;
        public IReadOnlyList<Cog> ConnectedCogs => connectedCogs;

        /// <summary>
        /// Retourne le rayon visuel incluant les dents (pour collision et contact)
        /// </summary>
        public float GetToothRadius()
        {
            // Même calcul que pour un Small cog
            float spriteBaseToothHeight = 8f / 64f; // 0.125 unités
            float visualBaseRadius = ENGINE_RADIUS * 1.4f; // baseRadiusRatio = 0.7
            return visualBaseRadius + (spriteBaseToothHeight * ENGINE_SPRITE_SCALE);
        }

        private void Update()
        {
            if (isRunning)
            {
                // Fait tourner tous les rouages connectés
                foreach (var cog in connectedCogs)
                {
                    cog.Rotate(rotationSpeed * Time.deltaTime);
                }
            }
        }

        /// <summary>
        /// Enregistre un rouage connecté au moteur
        /// </summary>
        public bool RegisterCog(Cog cog)
        {
            if (cog == null || connectedCogs.Contains(cog))
            {
                return false;
            }

            connectedCogs.Add(cog);
            RecalculatePower();
            return true;
        }

        /// <summary>
        /// Désenregistre un rouage
        /// </summary>
        public bool UnregisterCog(Cog cog)
        {
            if (cog == null || !connectedCogs.Contains(cog))
            {
                return false;
            }

            connectedCogs.Remove(cog);
            RecalculatePower();
            return true;
        }

        /// <summary>
        /// Recalcule la puissance totale utilisée et met à jour l'état
        /// </summary>
        public void RecalculatePower()
        {
            currentPowerUsed = 0;

            foreach (var cog in connectedCogs)
            {
                currentPowerUsed += cog.PowerRequired;
            }

            UpdateStatus();
        }

        /// <summary>
        /// Met à jour l'état du moteur selon la puissance utilisée
        /// </summary>
        private void UpdateStatus()
        {
            float usagePercent = (float)currentPowerUsed / maxPower;

            if (currentPowerUsed > maxPower)
            {
                currentStatus = PowerStatus.Overload;
                isRunning = false;
                Debug.LogWarning($"Engine OVERLOAD! {currentPowerUsed}/{maxPower} power used. Chain stopped.");
            }
            else if (usagePercent >= 0.8f)
            {
                currentStatus = PowerStatus.Warning;
                isRunning = true;
            }
            else
            {
                currentStatus = PowerStatus.OK;
                isRunning = true;
            }
        }

        /// <summary>
        /// Vérifie si on peut ajouter un rouage sans surcharger
        /// </summary>
        public bool CanAddCog(int additionalPower)
        {
            return (currentPowerUsed + additionalPower) <= maxPower;
        }

        /// <summary>
        /// Améliore la puissance maximale du moteur
        /// </summary>
        public bool UpgradePower(int amount = 5)
        {
            // TODO: Vérifier si le joueur a assez d'or
            maxPower += amount;
            RecalculatePower();
            Debug.Log($"Engine power upgraded to {maxPower}");
            return true;
        }

        /// <summary>
        /// Améliore la vitesse de rotation du moteur
        /// </summary>
        public bool UpgradeSpeed(float amount = 0.5f)
        {
            // TODO: Vérifier si le joueur a assez d'or
            rotationSpeed += amount;
            Debug.Log($"Engine speed upgraded to {rotationSpeed}");
            return true;
        }

        /// <summary>
        /// Démarre manuellement le moteur (pour la phase de combat)
        /// </summary>
        public void StartEngine()
        {
            if (currentStatus != PowerStatus.Overload)
            {
                isRunning = true;
                Debug.Log("Engine started!");
            }
            else
            {
                Debug.LogWarning("Cannot start engine: OVERLOAD");
            }
        }

        /// <summary>
        /// Arrête le moteur (pour la phase de construction)
        /// </summary>
        public void StopEngine()
        {
            isRunning = false;
            Debug.Log("Engine stopped!");
        }

        /// <summary>
        /// Obtient la couleur visuelle selon l'état
        /// </summary>
        public Color GetStatusColor()
        {
            return currentStatus switch
            {
                PowerStatus.OK => okColor,
                PowerStatus.Warning => warningColor,
                PowerStatus.Overload => overloadColor,
                _ => Color.white
            };
        }

        /// <summary>
        /// Dessine les Gizmos pour debug
        /// </summary>
        private void OnDrawGizmos()
        {
            Gizmos.color = GetStatusColor();
            Gizmos.DrawWireSphere(transform.position, ENGINE_RADIUS);

            // Dessine les connexions aux rouages
            if (connectedCogs != null)
            {
                Gizmos.color = Color.cyan;
                foreach (var cog in connectedCogs)
                {
                    if (cog != null)
                    {
                        Gizmos.DrawLine(transform.position, cog.transform.position);
                    }
                }
            }
        }
    }
}
