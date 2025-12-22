using UnityEngine;
using CogsTowerDefense.Grid;

namespace CogsTowerDefense.Cogs
{
    /// <summary>
    /// Rouage qui tourne grâce au moteur et produit des unités
    /// </summary>
    public class Cog : MonoBehaviour
    {
        [Header("Cog Configuration")]
        [SerializeField] private CogSize cogSize = CogSize.Small;
        [SerializeField] private int level = 1;
        [SerializeField] private UnitType unitType = UnitType.Melee;

        [Header("Production Settings")]
        [SerializeField] private float baseRotationsRequired = 10f; // Rotations pour produire 1 unité

        [Header("Grid Integration")]
        [SerializeField] private HexCoordinates gridPosition;

        [Header("Visual")]
        [SerializeField] private Transform visualTransform; // L'objet qui tourne visuellement

        // État interne
        private float currentRotation = 0f; // Progrès de rotation (0-100%)
        private int powerRequired = 1;
        private Engine connectedEngine;
        private bool isConnected = false;

        // Propriétés publiques
        public CogSize Size => cogSize;
        public int Level => level;
        public UnitType UnitType => unitType;
        public int PowerRequired => powerRequired;
        public float RotationProgress => currentRotation / GetRotationsRequired();
        public HexCoordinates GridPosition => gridPosition;
        public bool IsConnected => isConnected;
        public Engine ConnectedEngine => connectedEngine;

        private void Awake()
        {
            RecalculatePowerRequired();

            // Si pas de visualTransform assigné, utilise l'objet lui-même
            if (visualTransform == null)
            {
                visualTransform = transform;
            }
        }

        /// <summary>
        /// Calcule la puissance requise selon la taille et le niveau
        /// Formule : taille + (niveau / 10)
        /// </summary>
        private void RecalculatePowerRequired()
        {
            int basePower = (int)cogSize;
            int levelBonus = level / 10;
            powerRequired = basePower + levelBonus;
        }

        /// <summary>
        /// Calcule le nombre de rotations nécessaires pour produire une unité
        /// Diminue avec le niveau
        /// </summary>
        private float GetRotationsRequired()
        {
            // Réduit le temps de production avec le niveau
            float levelBonus = 1f - (level * 0.01f); // -1% par niveau
            levelBonus = Mathf.Max(levelBonus, 0.5f); // Minimum 50%
            return baseRotationsRequired * levelBonus;
        }

        /// <summary>
        /// Fait tourner le rouage (appelé par l'Engine)
        /// </summary>
        public void Rotate(float rotationAmount)
        {
            if (!isConnected) return;

            currentRotation += rotationAmount;

            // Rotation visuelle
            if (visualTransform != null)
            {
                float degreesPerRotation = 360f;
                visualTransform.Rotate(Vector3.forward, rotationAmount * degreesPerRotation);
            }

            // Vérifie si on a atteint le seuil de production
            if (currentRotation >= GetRotationsRequired())
            {
                ProduceUnit();
                currentRotation = 0f; // Reset
            }
        }

        /// <summary>
        /// Produit une unité
        /// </summary>
        private void ProduceUnit()
        {
            Debug.Log($"Cog at {gridPosition} produced a {unitType} unit (Level {level})!");

            // TODO: Instancier l'unité réelle
            // Pour l'instant, juste un log
        }

        /// <summary>
        /// Connecte ce rouage à un moteur
        /// </summary>
        public bool ConnectToEngine(Engine engine)
        {
            if (engine == null) return false;

            // Déconnecte de l'ancien moteur si nécessaire
            if (connectedEngine != null)
            {
                DisconnectFromEngine();
            }

            // Vérifie si le moteur peut supporter ce rouage
            if (!engine.CanAddCog(powerRequired))
            {
                Debug.LogWarning($"Cannot connect cog: Engine would be overloaded ({engine.CurrentPowerUsed + powerRequired}/{engine.MaxPower})");
                return false;
            }

            connectedEngine = engine;
            isConnected = engine.RegisterCog(this);

            if (isConnected)
            {
                Debug.Log($"Cog connected to engine. Power: {powerRequired}");
            }

            return isConnected;
        }

        /// <summary>
        /// Déconnecte ce rouage du moteur
        /// </summary>
        public void DisconnectFromEngine()
        {
            if (connectedEngine != null)
            {
                connectedEngine.UnregisterCog(this);
                connectedEngine = null;
                isConnected = false;
                currentRotation = 0f; // Reset le progrès
            }
        }

        /// <summary>
        /// Améliore le niveau du rouage
        /// </summary>
        public bool Upgrade(int cost)
        {
            // TODO: Vérifier si le joueur a assez d'or

            int oldPowerRequired = powerRequired;
            level++;
            RecalculatePowerRequired();

            // Si connecté à un moteur, vérifie qu'il peut supporter l'augmentation de puissance
            if (isConnected && connectedEngine != null)
            {
                int powerIncrease = powerRequired - oldPowerRequired;

                if (powerIncrease > 0 && !connectedEngine.CanAddCog(powerIncrease))
                {
                    // Annule l'upgrade
                    level--;
                    RecalculatePowerRequired();
                    Debug.LogWarning("Cannot upgrade: Would overload the engine!");
                    return false;
                }

                // Recalcule la puissance du moteur
                connectedEngine.RecalculatePower();
            }

            Debug.Log($"Cog upgraded to level {level}. Power required: {powerRequired}");
            return true;
        }

        /// <summary>
        /// Initialise la position sur la grille
        /// </summary>
        public void SetGridPosition(HexCoordinates position)
        {
            gridPosition = position;
        }

        /// <summary>
        /// Change le type d'unité produite
        /// </summary>
        public void SetUnitType(UnitType newType)
        {
            unitType = newType;
            Debug.Log($"Cog now produces {unitType} units");
        }

        /// <summary>
        /// Dessine les Gizmos pour debug
        /// </summary>
        private void OnDrawGizmos()
        {
            // Couleur selon la connexion
            Gizmos.color = isConnected ? Color.green : Color.red;

            // Taille du Gizmo selon la taille du rouage
            float gizmoSize = (int)cogSize * 0.2f;
            Gizmos.DrawWireCube(transform.position, Vector3.one * gizmoSize);

            // Affiche la progression
            if (isConnected)
            {
                Gizmos.color = Color.yellow;
                float progressHeight = RotationProgress * 0.5f;
                Gizmos.DrawLine(transform.position, transform.position + Vector3.up * progressHeight);
            }
        }

        private void OnDestroy()
        {
            // Déconnecte proprement du moteur
            DisconnectFromEngine();
        }
    }
}
