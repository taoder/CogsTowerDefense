using UnityEngine;
using CogsTowerDefense.Utils;

namespace CogsTowerDefense.Cogs
{
    /// <summary>
    /// Script de setup automatique pour tester le système de rouages
    /// Génère les sprites et configure les objets au runtime
    /// Système de placement libre par contact physique
    /// </summary>
    public class CogTestSetup : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Engine engine;
        [SerializeField] private CogChain cogChain;

        [Header("Engine Setup")]
        [SerializeField] private Vector2 enginePosition = Vector2.zero;

        [Header("Test Cogs")]
        [SerializeField] private bool spawnTestCogs = true;
        [SerializeField] private int numberOfTestCogs = 3;

        private void Start()
        {
            // Trouve les références si non assignées
            if (cogChain == null)
            {
                cogChain = FindFirstObjectByType<CogChain>();
            }

            // Setup
            SetupEngine();

            if (spawnTestCogs)
            {
                SpawnTestCogs();
            }
        }

        /// <summary>
        /// Configure le moteur
        /// </summary>
        private void SetupEngine()
        {
            if (engine == null)
            {
                Debug.LogError("Engine not assigned!");
                return;
            }

            // Position le moteur
            engine.transform.position = enginePosition;

            Debug.Log($"Engine positioned at {enginePosition}");

            // Crée le sprite visuel pour le moteur
            CreateEngineVisual();
        }

        /// <summary>
        /// Crée le visuel du moteur
        /// </summary>
        private void CreateEngineVisual()
        {
            // Crée un enfant pour le visuel
            GameObject visual = new GameObject("EngineVisual");
            visual.transform.SetParent(engine.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = Vector3.one;

            // Ajoute un SpriteRenderer
            SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = GeometricSpriteGenerator.CreateCircleSprite(64, new Color(1f, 0.5f, 0f), true);
            sr.sortingOrder = 0;

            // Ajoute un marqueur au centre
            GameObject center = new GameObject("Center");
            center.transform.SetParent(visual.transform);
            center.transform.localPosition = Vector3.zero;
            center.transform.localScale = Vector3.one * 0.3f;

            SpriteRenderer centerSr = center.AddComponent<SpriteRenderer>();
            centerSr.sprite = GeometricSpriteGenerator.CreateCircleSprite(32, Color.yellow, false);
            centerSr.sortingOrder = 1;
        }

        /// <summary>
        /// Spawn quelques rouages de test autour du moteur
        /// Positionnés pour qu'ils touchent le moteur ou d'autres rouages
        /// </summary>
        private void SpawnTestCogs()
        {
            if (engine == null)
            {
                Debug.LogWarning("Cannot spawn test cogs: engine not assigned");
                return;
            }

            // Positions de test en cercle autour du moteur
            // Les rouages doivent toucher le moteur (distance = engineRadius + cogRadius)
            float[] angles = { 0f, 60f, 120f, 180f, 240f, 300f }; // 6 directions

            for (int i = 0; i < Mathf.Min(numberOfTestCogs, angles.Length); i++)
            {
                CreateTestCog(angles[i], i);
            }

            Debug.Log($"Spawned {numberOfTestCogs} test cogs");
        }

        /// <summary>
        /// Crée un rouage de test à un angle autour du moteur
        /// </summary>
        private void CreateTestCog(float angleDegrees, int index)
        {
            // Détermine la taille selon l'index
            CogSize size = (CogSize)((index % 3) + 1); // Small, Medium, Large

            // Calcule le rayon du rouage
            float cogRadius = size switch
            {
                CogSize.Small => 0.5f,
                CogSize.Medium => 1.0f,
                CogSize.Large => 1.8f,
                _ => 0.5f
            };

            // Position pour que le rouage touche le moteur
            float distance = engine.Radius + cogRadius;
            float angleRad = angleDegrees * Mathf.Deg2Rad;
            Vector2 position = enginePosition + new Vector2(
                Mathf.Cos(angleRad) * distance,
                Mathf.Sin(angleRad) * distance
            );

            // Crée le GameObject
            GameObject cogObj = new GameObject($"TestCog_{size}_{index}");
            cogObj.transform.position = position;

            // Ajoute le composant Cog
            Cog cog = cogObj.AddComponent<Cog>();

            // Crée le visuel
            CreateCogVisual(cogObj, size);

            // Enregistre dans la chaîne
            if (cogChain != null)
            {
                cogChain.RegisterCog(cog);
            }

            Debug.Log($"Created {size} cog at {position}, touching engine");
        }

        /// <summary>
        /// Crée le visuel d'un rouage
        /// </summary>
        private void CreateCogVisual(GameObject parent, CogSize size)
        {
            // Crée un enfant pour le corps du rouage
            GameObject body = new GameObject("CogBody");
            body.transform.SetParent(parent.transform);
            body.transform.localPosition = Vector3.zero;

            // Taille visuelle selon CogSize et rayons réels
            float scale = size switch
            {
                CogSize.Small => 1.0f,   // Rayon 0.5
                CogSize.Medium => 2.0f,  // Rayon 1.0
                CogSize.Large => 3.6f,   // Rayon 1.8
                _ => 1.0f
            };

            body.transform.localScale = Vector3.one * scale;

            // Couleur selon la taille
            Color color = size switch
            {
                CogSize.Small => new Color(0.2f, 0.8f, 1f),   // Cyan
                CogSize.Medium => new Color(0.8f, 0.2f, 1f),  // Magenta
                CogSize.Large => new Color(1f, 0.8f, 0.2f),   // Orange (héros)
                _ => Color.white
            };

            // Ajoute le SpriteRenderer avec un sprite d'engrenage
            SpriteRenderer sr = body.AddComponent<SpriteRenderer>();
            int spriteSize = 64; // Taille de base

            int teeth = size switch
            {
                CogSize.Small => 6,
                CogSize.Medium => 8,
                CogSize.Large => 12,  // Plus de dents pour les gros rouages
                _ => 8
            };

            sr.sprite = GeometricSpriteGenerator.CreateGearSprite(spriteSize, color, teeth);
            sr.sortingOrder = 10;

            Debug.Log($"Visual created for {size} cog with scale {scale}");
        }

        /// <summary>
        /// Démarre le moteur (appelé par un bouton UI)
        /// </summary>
        public void StartEngine()
        {
            if (engine != null)
            {
                engine.StartEngine();
                Debug.Log("Engine started from UI");
            }
        }

        /// <summary>
        /// Arrête le moteur
        /// </summary>
        public void StopEngine()
        {
            if (engine != null)
            {
                engine.StopEngine();
                Debug.Log("Engine stopped from UI");
            }
        }

        /// <summary>
        /// Améliore la puissance du moteur
        /// </summary>
        public void UpgradeEnginePower()
        {
            if (engine != null)
            {
                engine.UpgradePower(5);
            }
        }

        /// <summary>
        /// Améliore la vitesse du moteur
        /// </summary>
        public void UpgradeEngineSpeed()
        {
            if (engine != null)
            {
                engine.UpgradeSpeed(0.5f);
            }
        }
    }
}
