using UnityEngine;

namespace CogsTowerDefense.Cogs
{
    /// <summary>
    /// ScriptableObject contenant la configuration d'un type de rouage
    /// Permet de créer facilement différents types de rouages via l'éditeur
    /// </summary>
    [CreateAssetMenu(fileName = "New Cog Data", menuName = "Cogs Tower Defense/Cog Data")]
    public class CogData : ScriptableObject
    {
        [Header("Basic Info")]
        [SerializeField] private string cogName = "Basic Cog";
        [SerializeField] private CogSize size = CogSize.Small;
        [SerializeField] private UnitType unitType = UnitType.Melee;

        [Header("Costs")]
        [SerializeField] private int placementCost = 50;
        [SerializeField] private int upgradeCostBase = 25;
        [SerializeField] private float upgradeCostMultiplier = 1.5f; // Coût x1.5 par niveau

        [Header("Production")]
        [SerializeField] private float baseRotationsRequired = 10f;
        [SerializeField] private float productionSpeedPerLevel = 0.01f; // +1% par niveau

        [Header("Power")]
        [SerializeField] private int basePowerRequired = 1;

        [Header("Visual")]
        [SerializeField] private GameObject prefab;
        [SerializeField] private Sprite icon;
        [SerializeField] private Color cogColor = Color.white;

        [Header("Description")]
        [TextArea(3, 5)]
        [SerializeField] private string description = "A basic cog that produces units.";

        // Propriétés publiques
        public string CogName => cogName;
        public CogSize Size => size;
        public UnitType UnitType => unitType;
        public int PlacementCost => placementCost;
        public float BaseRotationsRequired => baseRotationsRequired;
        public int BasePowerRequired => basePowerRequired;
        public GameObject Prefab => prefab;
        public Sprite Icon => icon;
        public Color CogColor => cogColor;
        public string Description => description;

        /// <summary>
        /// Calcule le coût d'upgrade pour un niveau donné
        /// </summary>
        public int GetUpgradeCost(int currentLevel)
        {
            return Mathf.RoundToInt(upgradeCostBase * Mathf.Pow(upgradeCostMultiplier, currentLevel - 1));
        }

        /// <summary>
        /// Calcule la puissance requise pour un niveau donné
        /// Formule : basePower + (niveau / 10)
        /// </summary>
        public int GetPowerRequired(int level)
        {
            int levelBonus = level / 10;
            return basePowerRequired + levelBonus;
        }

        /// <summary>
        /// Calcule les rotations nécessaires pour un niveau donné
        /// </summary>
        public float GetRotationsRequired(int level)
        {
            float reduction = 1f - (level * productionSpeedPerLevel);
            reduction = Mathf.Max(reduction, 0.5f); // Minimum 50% du temps de base
            return baseRotationsRequired * reduction;
        }
    }
}
