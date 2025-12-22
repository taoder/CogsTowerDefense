namespace CogsTowerDefense.Cogs
{
    /// <summary>
    /// Taille d'un rouage, détermine la puissance requise et la vitesse de production
    /// </summary>
    public enum CogSize
    {
        Small = 1,   // Petit : Rapide, puissance 1
        Medium = 2,  // Moyen : Équilibré, puissance 2
        Large = 3    // Grand : Lent mais puissant, puissance 3
    }

    /// <summary>
    /// État de la puissance d'un moteur
    /// </summary>
    public enum PowerStatus
    {
        OK,         // Puissance suffisante (< 80% utilisée)
        Warning,    // Proche de la limite (80-100% utilisée)
        Overload    // Surchargé (> 100% utilisée) - ARRÊT
    }

    /// <summary>
    /// Type d'unité produite par un rouage
    /// </summary>
    public enum UnitType
    {
        Melee,      // Corps à corps
        Ranged,     // Distance
        AoE,        // Dégâts de zone
        Tank        // Tank défensif
    }
}
