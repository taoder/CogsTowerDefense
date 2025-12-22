using System.Collections.Generic;
using UnityEngine;
using CogsTowerDefense.Grid;

namespace CogsTowerDefense.Cogs
{
    /// <summary>
    /// Gère les chaînes de rouages connectés à un moteur
    /// Valide les connexions et calcule la puissance totale
    /// </summary>
    public class CogChain : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Engine engine;
        [SerializeField] private HexGrid hexGrid;

        // Cache des rouages par position
        private Dictionary<HexCoordinates, Cog> cogsByPosition = new Dictionary<HexCoordinates, Cog>();

        // Rouages valides (connectés au moteur)
        private HashSet<Cog> validCogs = new HashSet<Cog>();

        private void Awake()
        {
            if (hexGrid == null)
            {
                hexGrid = FindFirstObjectByType<HexGrid>();
            }
        }

        /// <summary>
        /// Enregistre un rouage dans la chaîne
        /// </summary>
        public bool RegisterCog(Cog cog, HexCoordinates position)
        {
            if (cog == null) return false;

            // Vérifie si la position est déjà occupée
            if (cogsByPosition.ContainsKey(position))
            {
                Debug.LogWarning($"Position {position} already has a cog!");
                return false;
            }

            // Ajoute le rouage
            cogsByPosition[position] = cog;
            cog.SetGridPosition(position);

            // Vérifie la connexion au moteur
            if (IsConnectedToEngine(position))
            {
                ConnectCogToEngine(cog);
            }
            else
            {
                Debug.LogWarning($"Cog at {position} is not connected to the engine!");
            }

            return true;
        }

        /// <summary>
        /// Supprime un rouage de la chaîne
        /// </summary>
        public void UnregisterCog(HexCoordinates position)
        {
            if (!cogsByPosition.TryGetValue(position, out Cog cog))
            {
                return;
            }

            // Déconnecte du moteur
            cog.DisconnectFromEngine();
            validCogs.Remove(cog);
            cogsByPosition.Remove(position);

            // Revalide tous les rouages (certains pourraient être déconnectés maintenant)
            RevalidateAllCogs();
        }

        /// <summary>
        /// Vérifie si une position est connectée au moteur (directement ou via d'autres rouages)
        /// </summary>
        private bool IsConnectedToEngine(HexCoordinates position)
        {
            if (engine == null) return false;

            // Utilise BFS pour trouver un chemin vers le moteur
            HashSet<HexCoordinates> visited = new HashSet<HexCoordinates>();
            Queue<HexCoordinates> toVisit = new Queue<HexCoordinates>();

            toVisit.Enqueue(position);
            visited.Add(position);

            while (toVisit.Count > 0)
            {
                HexCoordinates current = toVisit.Dequeue();

                // Vérifie si on est adjacent au moteur
                if (IsAdjacentToEngine(current))
                {
                    return true;
                }

                // Vérifie les voisins
                HexCoordinates[] neighbors = current.GetNeighbors();
                foreach (var neighbor in neighbors)
                {
                    if (visited.Contains(neighbor)) continue;

                    // Si le voisin a un rouage connecté, continue la recherche
                    if (cogsByPosition.ContainsKey(neighbor))
                    {
                        visited.Add(neighbor);
                        toVisit.Enqueue(neighbor);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Vérifie si une position est adjacente au moteur
        /// </summary>
        private bool IsAdjacentToEngine(HexCoordinates position)
        {
            if (engine == null) return false;

            HexCoordinates enginePos = engine.GridPosition;
            return position.DistanceTo(enginePos) == 1;
        }

        /// <summary>
        /// Connecte un rouage au moteur
        /// </summary>
        private void ConnectCogToEngine(Cog cog)
        {
            if (cog.ConnectToEngine(engine))
            {
                validCogs.Add(cog);
            }
        }

        /// <summary>
        /// Revalide tous les rouages (appelé après suppression)
        /// </summary>
        private void RevalidateAllCogs()
        {
            // Déconnecte tous les rouages
            foreach (var cog in cogsByPosition.Values)
            {
                cog.DisconnectFromEngine();
            }

            validCogs.Clear();

            // Reconnecte ceux qui sont encore valides
            foreach (var kvp in cogsByPosition)
            {
                if (IsConnectedToEngine(kvp.Key))
                {
                    ConnectCogToEngine(kvp.Value);
                }
            }

            Debug.Log($"Revalidated chain: {validCogs.Count}/{cogsByPosition.Count} cogs connected");
        }

        /// <summary>
        /// Vérifie si on peut placer un rouage à une position
        /// </summary>
        public bool CanPlaceCog(HexCoordinates position, Cog cog)
        {
            // Vérifie que la position est libre
            if (cogsByPosition.ContainsKey(position))
            {
                return false;
            }

            // Vérifie que le rouage serait connecté au moteur
            if (!IsConnectedToEngine(position))
            {
                return false;
            }

            // Vérifie que le moteur peut supporter la puissance
            if (engine != null && !engine.CanAddCog(cog.PowerRequired))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Obtient le rouage à une position
        /// </summary>
        public Cog GetCogAtPosition(HexCoordinates position)
        {
            cogsByPosition.TryGetValue(position, out Cog cog);
            return cog;
        }

        /// <summary>
        /// Obtient tous les rouages valides (connectés)
        /// </summary>
        public IReadOnlyCollection<Cog> GetValidCogs()
        {
            return validCogs;
        }

        /// <summary>
        /// Obtient le nombre total de rouages
        /// </summary>
        public int GetTotalCogCount()
        {
            return cogsByPosition.Count;
        }

        /// <summary>
        /// Obtient le nombre de rouages connectés
        /// </summary>
        public int GetConnectedCogCount()
        {
            return validCogs.Count;
        }

        /// <summary>
        /// Dessine les Gizmos pour debug
        /// </summary>
        private void OnDrawGizmos()
        {
            if (engine == null || hexGrid == null) return;

            // Dessine la zone autour du moteur
            Gizmos.color = Color.cyan;
            Vector3 engineWorldPos = IsometricUtils.HexToWorldPosition(engine.GridPosition, hexGrid.HexSize);
            Gizmos.DrawWireSphere(engineWorldPos, 1.5f);

            // Dessine les connexions valides en vert
            Gizmos.color = Color.green;
            foreach (var cog in validCogs)
            {
                if (cog != null)
                {
                    Gizmos.DrawLine(engineWorldPos, cog.transform.position);
                }
            }

            // Dessine les rouages non connectés en rouge
            Gizmos.color = Color.red;
            foreach (var kvp in cogsByPosition)
            {
                if (!validCogs.Contains(kvp.Value))
                {
                    Vector3 cogWorldPos = IsometricUtils.HexToWorldPosition(kvp.Key, hexGrid.HexSize);
                    Gizmos.DrawWireCube(cogWorldPos, Vector3.one * 0.3f);
                }
            }
        }
    }
}
