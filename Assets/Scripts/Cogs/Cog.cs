using UnityEngine;

namespace CogsTowerDefense.Cogs
{
    /// <summary>
    /// Rouage qui tourne grâce au moteur et produit des unités
    /// Système de placement libre : les rouages doivent se toucher physiquement
    /// </summary>
    public class Cog : MonoBehaviour
    {
        [Header("Cog Configuration")]
        [SerializeField] private CogSize cogSize = CogSize.Small;
        [SerializeField] private int level = 1;
        [SerializeField] private UnitType unitType = UnitType.Melee;

        [Header("Production Settings")]
        [SerializeField] private float baseRotationsRequired = 10f; // Rotations pour produire 1 unité

        [Header("Visual")]
        [SerializeField] private Transform visualTransform; // L'objet qui tourne visuellement

        // Rayons fixes par taille (grosse différence entre les tailles)
        private const float SMALL_RADIUS = 0.5f;
        private const float MEDIUM_RADIUS = 1.0f;  // 2x plus grand
        private const float LARGE_RADIUS = 1.8f;   // 3.6x plus grand, pour les héros

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
        public Vector2 Position => transform.position;
        public bool IsConnected => isConnected;
        public Engine ConnectedEngine => connectedEngine;
        public float Radius => GetRadius();

        private void Awake()
        {
            RecalculatePowerRequired();

            // Si pas de visualTransform assigné, utilise l'objet lui-même
            if (visualTransform == null)
            {
                visualTransform = transform;
            }

            // Ajoute un CircleCollider2D pour les collisions physiques
            // Utilise le rayon incluant les dents
            CircleCollider2D collider = GetComponent<CircleCollider2D>();
            if (collider == null)
            {
                collider = gameObject.AddComponent<CircleCollider2D>();
            }
            collider.radius = GetToothRadius();
            collider.isTrigger = true; // Pas de physique, juste détection
        }

        /// <summary>
        /// Retourne le rayon du rouage en fonction de sa taille (rayon logique)
        /// </summary>
        public float GetRadius()
        {
            return cogSize switch
            {
                CogSize.Small => SMALL_RADIUS,
                CogSize.Medium => MEDIUM_RADIUS,
                CogSize.Large => LARGE_RADIUS,
                _ => SMALL_RADIUS
            };
        }

        /// <summary>
        /// Retourne le rayon visuel incluant les dents (pour collision et contact)
        /// Les dents ajoutent 8 pixels, ce qui varie selon le scale du sprite
        /// </summary>
        public float GetToothRadius()
        {
            // Le sprite de base a des dents de 8 pixels avec pixelsPerUnit = 64
            // Donc hauteur de base = 8/64 = 0.125 unités
            // Multiplié par le scale selon la taille
            float spriteBaseToothHeight = 8f / 64f;
            float scale = GetSpriteScale();

            // Le rayon de base du sprite est Radius * 1.4 (baseRadiusRatio = 0.7, et sprite fait 0.5 en unités de base)
            // Donc rayon avec dents = rayon de base visuel + hauteur des dents
            float visualBaseRadius = Radius * 1.4f; // Le sprite a un baseRadiusRatio de 0.7
            return visualBaseRadius + (spriteBaseToothHeight * scale);
        }

        /// <summary>
        /// Retourne le scale du sprite selon la taille
        /// </summary>
        private float GetSpriteScale()
        {
            return cogSize switch
            {
                CogSize.Small => 1.0f,
                CogSize.Medium => 2.0f,
                CogSize.Large => 3.6f,
                _ => 1.0f
            };
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
            Debug.Log($"Cog ({cogSize}) at {transform.position} produced a {unitType} unit (Level {level})!");

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

            // Cercle selon le rayon réel du rouage
            Gizmos.DrawWireSphere(transform.position, GetRadius());

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
