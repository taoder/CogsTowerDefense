using UnityEngine;
using CogsTowerDefense.Grid;
using CogsTowerDefense.Utils;

namespace CogsTowerDefense.Cogs
{
    /// <summary>
    /// Script de setup automatique pour tester le système de rouages
    /// Génère les sprites et configure les objets au runtime
    /// </summary>
    public class CogTestSetup : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HexGrid hexGrid;
        [SerializeField] private Engine engine;
        [SerializeField] private CogChain cogChain;
        [SerializeField] private CogPlacementSystem placementSystem;

        [Header("Engine Setup")]
        [SerializeField] private HexCoordinates enginePosition = new HexCoordinates(0, 0);
        [SerializeField] private int engineMaxPower = 15;
        [SerializeField] private float engineRotationSpeed = 1.0f;

        [Header("Test Cogs")]
        [SerializeField] private bool spawnTestCogs = true;
        [SerializeField] private int numberOfTestCogs = 3;

        private void Start()
        {
            // Trouve les références si non assignées
            if (hexGrid == null)
            {
                hexGrid = FindFirstObjectByType<HexGrid>();
            }

            if (cogChain == null)
            {
                cogChain = FindFirstObjectByType<CogChain>();
            }

            if (placementSystem == null)
            {
                placementSystem = FindFirstObjectByType<CogPlacementSystem>();
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

            // Position le moteur sur la grille
            Vector3 engineWorldPos = IsometricUtils.HexToWorldPosition(enginePosition, hexGrid.HexSize);
            engine.transform.position = engineWorldPos;
            engine.SetGridPosition(enginePosition);

            // Configure les valeurs
            // Note: On devrait avoir des propriétés setter dans Engine.cs
            Debug.Log($"Engine positioned at {enginePosition}, world pos {engineWorldPos}");

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
            sr.sortingLayerName = "Cogs";
            sr.sortingOrder = 0;

            // Ajoute un marqueur au centre
            GameObject center = new GameObject("Center");
            center.transform.SetParent(visual.transform);
            center.transform.localPosition = Vector3.zero;
            center.transform.localScale = Vector3.one * 0.3f;

            SpriteRenderer centerSr = center.AddComponent<SpriteRenderer>();
            centerSr.sprite = GeometricSpriteGenerator.CreateCircleSprite(32, Color.yellow, false);
            centerSr.sortingLayerName = "Cogs";
            centerSr.sortingOrder = 1;
        }

        /// <summary>
        /// Spawn quelques rouages de test autour du moteur
        /// </summary>
        private void SpawnTestCogs()
        {
            if (hexGrid == null || engine == null)
            {
                Debug.LogWarning("Cannot spawn test cogs: missing references");
                return;
            }

            // Positions de test autour du moteur
            HexCoordinates[] testPositions = new HexCoordinates[]
            {
                new HexCoordinates(1, 0),   // Est
                new HexCoordinates(0, 1),   // Sud-Est
                new HexCoordinates(-1, 1),  // Sud-Ouest
                new HexCoordinates(2, 0),   // Plus loin à l'Est
                new HexCoordinates(1, 1),   // Diagonal
            };

            for (int i = 0; i < Mathf.Min(numberOfTestCogs, testPositions.Length); i++)
            {
                CreateTestCog(testPositions[i], i);
            }

            Debug.Log($"Spawned {numberOfTestCogs} test cogs");
        }

        /// <summary>
        /// Crée un rouage de test à une position
        /// </summary>
        private void CreateTestCog(HexCoordinates position, int index)
        {
            // Détermine la taille selon l'index
            CogSize size = (CogSize)((index % 3) + 1); // Small, Medium, Large

            // Crée le GameObject
            GameObject cogObj = new GameObject($"TestCog_{position}_{size}");
            Vector3 worldPos = IsometricUtils.HexToWorldPosition(position, hexGrid.HexSize);
            cogObj.transform.position = worldPos;

            // Ajoute le composant Cog
            Cog cog = cogObj.AddComponent<Cog>();
            // Note: Les propriétés sont private, on devrait les exposer via des méthodes publiques

            // Crée le visuel
            CreateCogVisual(cogObj, size);

            // Enregistre dans la grille
            hexGrid.PlaceObject(position, cogObj);

            // Enregistre dans la chaîne
            if (cogChain != null)
            {
                cogChain.RegisterCog(cog, position);
            }

            Debug.Log($"Created {size} cog at {position}");
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

            // Taille selon CogSize
            float scale = size switch
            {
                CogSize.Small => 0.4f,
                CogSize.Medium => 0.6f,
                CogSize.Large => 0.8f,
                _ => 0.5f
            };

            body.transform.localScale = Vector3.one * scale;

            // Couleur selon la taille
            Color color = size switch
            {
                CogSize.Small => new Color(0.2f, 0.8f, 1f),   // Cyan
                CogSize.Medium => new Color(0.8f, 0.2f, 1f),  // Magenta
                CogSize.Large => new Color(1f, 0.8f, 0.2f),   // Orange
                _ => Color.white
            };

            // Ajoute le SpriteRenderer avec un sprite d'engrenage
            SpriteRenderer sr = body.AddComponent<SpriteRenderer>();
            int spriteSize = size switch
            {
                CogSize.Small => 48,
                CogSize.Medium => 64,
                CogSize.Large => 80,
                _ => 64
            };

            int teeth = size switch
            {
                CogSize.Small => 6,
                CogSize.Medium => 8,
                CogSize.Large => 10,
                _ => 8
            };

            sr.sprite = GeometricSpriteGenerator.CreateGearSprite(spriteSize, color, teeth);
            sr.sortingLayerName = "Cogs";
            sr.sortingOrder = 10;

            // L'enfant Visual tournera
            Cog cogComponent = parent.GetComponent<Cog>();
            if (cogComponent != null)
            {
                // Note: Devrait assigner visualTransform via une méthode publique
                Debug.Log($"Visual created for {size} cog");
            }
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
