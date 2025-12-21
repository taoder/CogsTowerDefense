using System;
using UnityEngine;

namespace ClogTowerDefense.Grid
{
    /// <summary>
    /// Représente des coordonnées hexagonales en système axial (q, r)
    /// Conversion vers système cube (x, y, z) où x + y + z = 0
    /// </summary>
    [System.Serializable]
    public struct HexCoordinates : IEquatable<HexCoordinates>
    {
        public int Q { get; private set; } // Colonne (axe horizontal)
        public int R { get; private set; } // Rangée (axe diagonal)

        // Conversion vers système cube pour certains calculs
        public int X => Q;
        public int Y => -Q - R;
        public int Z => R;

        public HexCoordinates(int q, int r)
        {
            Q = q;
            R = r;
        }

        /// <summary>
        /// Crée des coordonnées hexagonales depuis des coordonnées cube
        /// </summary>
        public static HexCoordinates FromCube(int x, int y, int z)
        {
            // Vérification : x + y + z doit être 0
            if (x + y + z != 0)
            {
                Debug.LogError($"Invalid cube coordinates: {x}, {y}, {z}. Sum must be 0.");
            }
            return new HexCoordinates(x, z);
        }

        /// <summary>
        /// Convertit des coordonnées offset (grille rectangulaire) vers hex axial
        /// Utilise odd-r layout (rangées impaires décalées vers la droite)
        /// </summary>
        public static HexCoordinates FromOffset(int col, int row)
        {
            int q = col - (row - (row & 1)) / 2;
            int r = row;
            return new HexCoordinates(q, r);
        }

        /// <summary>
        /// Convertit vers coordonnées offset (pour rendering ou debug)
        /// </summary>
        public Vector2Int ToOffset()
        {
            int col = Q + (R - (R & 1)) / 2;
            int row = R;
            return new Vector2Int(col, row);
        }

        /// <summary>
        /// Distance entre deux hexagones (en nombre de cases)
        /// </summary>
        public int DistanceTo(HexCoordinates other)
        {
            return (Mathf.Abs(X - other.X) + Mathf.Abs(Y - other.Y) + Mathf.Abs(Z - other.Z)) / 2;
        }

        /// <summary>
        /// Retourne les 6 voisins directs d'un hexagone
        /// </summary>
        public HexCoordinates[] GetNeighbors()
        {
            return new HexCoordinates[]
            {
                new HexCoordinates(Q + 1, R),     // Est
                new HexCoordinates(Q + 1, R - 1), // Nord-Est
                new HexCoordinates(Q, R - 1),     // Nord-Ouest
                new HexCoordinates(Q - 1, R),     // Ouest
                new HexCoordinates(Q - 1, R + 1), // Sud-Ouest
                new HexCoordinates(Q, R + 1)      // Sud-Est
            };
        }

        /// <summary>
        /// Retourne un voisin dans une direction spécifique (0-5)
        /// </summary>
        public HexCoordinates GetNeighbor(int direction)
        {
            direction = direction % 6;
            if (direction < 0) direction += 6;

            return direction switch
            {
                0 => new HexCoordinates(Q + 1, R),     // Est
                1 => new HexCoordinates(Q + 1, R - 1), // Nord-Est
                2 => new HexCoordinates(Q, R - 1),     // Nord-Ouest
                3 => new HexCoordinates(Q - 1, R),     // Ouest
                4 => new HexCoordinates(Q - 1, R + 1), // Sud-Ouest
                5 => new HexCoordinates(Q, R + 1),     // Sud-Est
                _ => this
            };
        }

        public override string ToString()
        {
            return $"Hex({Q}, {R})";
        }

        public bool Equals(HexCoordinates other)
        {
            return Q == other.Q && R == other.R;
        }

        public override bool Equals(object obj)
        {
            return obj is HexCoordinates other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Q, R);
        }

        public static bool operator ==(HexCoordinates a, HexCoordinates b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(HexCoordinates a, HexCoordinates b)
        {
            return !a.Equals(b);
        }

        public static HexCoordinates operator +(HexCoordinates a, HexCoordinates b)
        {
            return new HexCoordinates(a.Q + b.Q, a.R + b.R);
        }

        public static HexCoordinates operator -(HexCoordinates a, HexCoordinates b)
        {
            return new HexCoordinates(a.Q - b.Q, a.R - b.R);
        }
    }
}
