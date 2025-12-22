using System.Collections.Generic;
using UnityEngine;

namespace CogsTowerDefense.Cogs
{
    /// <summary>
    /// Gère les chaînes de rouages connectés à un moteur
    /// Validation par distance physique : les rouages doivent se toucher
    /// </summary>
    public class CogChain : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Engine engine;

        [Header("Contact Settings")]
        [SerializeField] private float contactTolerance = 0.1f; // Tolérance pour considérer que deux rouages se touchent

        // Tous les rouages dans le jeu
        private List<Cog> allCogs = new List<Cog>();

        // Rouages valides (connectés au moteur)
        private HashSet<Cog> validCogs = new HashSet<Cog>();

        /// <summary>
        /// Vérifie si deux positions se touchent (distance ≈ rayon1 + rayon2)
        /// </summary>
        private bool AreTouching(Vector2 pos1, float radius1, Vector2 pos2, float radius2)
        {
            float distance = Vector2.Distance(pos1, pos2);
            float requiredDistance = radius1 + radius2;

            // Les rouages se touchent si la distance est proche de la somme des rayons
            return Mathf.Abs(distance - requiredDistance) <= contactTolerance;
        }

        /// <summary>
        /// Vérifie si un rouage touche le moteur
        /// </summary>
        private bool TouchesEngine(Cog cog)
        {
            if (engine == null || cog == null) return false;

            return AreTouching(cog.Position, cog.Radius, engine.Position, engine.Radius);
        }

        /// <summary>
        /// Vérifie si deux rouages se touchent
        /// </summary>
        private bool TouchesCog(Cog cog1, Cog cog2)
        {
            if (cog1 == null || cog2 == null || cog1 == cog2) return false;

            return AreTouching(cog1.Position, cog1.Radius, cog2.Position, cog2.Radius);
        }

        /// <summary>
        /// Enregistre un rouage dans la chaîne
        /// Le rouage peut être connecté ou non au moteur
        /// </summary>
        public bool RegisterCog(Cog cog)
        {
            if (cog == null) return false;

            // Vérifie que le rouage n'est pas déjà enregistré
            if (allCogs.Contains(cog))
            {
                Debug.LogWarning($"Cog {cog.name} is already registered!");
                return false;
            }

            // Vérifie qu'il n'y a pas de collision avec d'autres rouages (overlap)
            if (OverlapsWithAnyCog(cog))
            {
                Debug.LogWarning($"Cog {cog.name} overlaps with another cog!");
                return false;
            }

            // Ajoute le rouage
            allCogs.Add(cog);

            // Vérifie la connexion au moteur
            if (IsConnectedToEngine(cog))
            {
                ConnectCogToEngine(cog);
                Debug.Log($"Cog {cog.name} registered and connected to engine");
            }
            else
            {
                Debug.Log($"Cog {cog.name} registered but not connected to engine (placement libre)");
            }

            return true;
        }

        /// <summary>
        /// Supprime un rouage de la chaîne
        /// </summary>
        public void UnregisterCog(Cog cog)
        {
            if (cog == null || !allCogs.Contains(cog))
            {
                return;
            }

            // Déconnecte du moteur
            cog.DisconnectFromEngine();
            validCogs.Remove(cog);
            allCogs.Remove(cog);

            // Revalide tous les rouages (certains pourraient être déconnectés maintenant)
            RevalidateAllCogs();
        }

        /// <summary>
        /// Vérifie si un rouage overlap avec un autre (collision)
        /// </summary>
        private bool OverlapsWithAnyCog(Cog newCog)
        {
            foreach (var existingCog in allCogs)
            {
                float distance = Vector2.Distance(newCog.Position, existingCog.Position);
                float minDistance = newCog.Radius + existingCog.Radius;

                // Overlap si la distance est inférieure à la somme des rayons
                if (distance < minDistance - contactTolerance)
                {
                    return true;
                }
            }

            // Vérifie aussi l'overlap avec le moteur
            if (engine != null)
            {
                float distance = Vector2.Distance(newCog.Position, engine.Position);
                float minDistance = newCog.Radius + engine.Radius;

                if (distance < minDistance - contactTolerance)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Vérifie si un rouage est connecté au moteur (directement ou via d'autres rouages)
        /// Utilise BFS pour trouver un chemin de contact physique
        /// </summary>
        private bool IsConnectedToEngine(Cog startCog)
        {
            if (engine == null || startCog == null) return false;

            // Si touche directement le moteur, c'est bon
            if (TouchesEngine(startCog))
            {
                return true;
            }

            // Sinon, cherche un chemin via d'autres rouages (BFS)
            HashSet<Cog> visited = new HashSet<Cog>();
            Queue<Cog> toVisit = new Queue<Cog>();

            toVisit.Enqueue(startCog);
            visited.Add(startCog);

            while (toVisit.Count > 0)
            {
                Cog current = toVisit.Dequeue();

                // Trouve tous les rouages qui touchent le rouage actuel
                foreach (var neighbor in allCogs)
                {
                    if (visited.Contains(neighbor)) continue;

                    if (TouchesCog(current, neighbor))
                    {
                        // Vérifie si ce voisin touche le moteur
                        if (TouchesEngine(neighbor))
                        {
                            return true;
                        }

                        // Sinon, continue la recherche
                        visited.Add(neighbor);
                        toVisit.Enqueue(neighbor);
                    }
                }
            }

            return false;
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
            foreach (var cog in allCogs)
            {
                cog.DisconnectFromEngine();
            }

            validCogs.Clear();

            // Reconnecte ceux qui sont encore valides
            foreach (var cog in allCogs)
            {
                if (IsConnectedToEngine(cog))
                {
                    ConnectCogToEngine(cog);
                }
            }

            Debug.Log($"Revalidated chain: {validCogs.Count}/{allCogs.Count} cogs connected");
        }

        /// <summary>
        /// Vérifie si on peut placer un rouage à une position
        /// Seule contrainte : pas d'overlap avec d'autres rouages
        /// La connexion au moteur n'est plus obligatoire
        /// </summary>
        public bool CanPlaceCog(Vector2 position, float radius, int powerRequired)
        {
            // Crée un rouage temporaire pour les tests
            Cog tempCog = CreateTempCog(position, radius);

            // Vérifie qu'il n'y a pas d'overlap
            if (OverlapsWithAnyCog(tempCog))
            {
                Destroy(tempCog.gameObject);
                return false;
            }

            // Vérifie si le rouage serait connecté au moteur
            bool wouldBeConnected = IsConnectedToEngine(tempCog);

            // Si connecté, vérifie que le moteur peut supporter la puissance
            if (wouldBeConnected && engine != null && !engine.CanAddCog(powerRequired))
            {
                Destroy(tempCog.gameObject);
                return false;
            }

            Destroy(tempCog.gameObject);
            return true; // OK même si non connecté
        }

        /// <summary>
        /// Crée un rouage temporaire pour les tests
        /// </summary>
        private Cog CreateTempCog(Vector2 position, float radius)
        {
            GameObject tempObj = new GameObject("TempCog");
            tempObj.transform.position = position;
            Cog tempCog = tempObj.AddComponent<Cog>();
            // Note: Le rayon sera récupéré via GetRadius() basé sur la taille
            return tempCog;
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
            return allCogs.Count;
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
            if (engine == null) return;

            // Dessine les connexions valides en vert
            Gizmos.color = Color.green;
            foreach (var cog in validCogs)
            {
                if (cog != null)
                {
                    Gizmos.DrawLine(engine.Position, cog.Position);
                }
            }

            // Dessine les rouages non connectés en rouge
            Gizmos.color = Color.red;
            foreach (var cog in allCogs)
            {
                if (!validCogs.Contains(cog))
                {
                    Gizmos.DrawWireCube(cog.Position, Vector3.one * 0.3f);
                }
            }
        }
    }
}
