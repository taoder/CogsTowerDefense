using UnityEngine;

namespace CogsTowerDefense.Grid
{
    /// <summary>
    /// Utilitaires pour la conversion entre coordonnées hexagonales et position monde isométrique
    /// </summary>
    public static class IsometricUtils
    {
        // Taille d'un hexagone (flat-top orientation)
        public const float HEX_SIZE = 1.0f;

        // Facteurs de conversion pour hexagones flat-top
        private static readonly float SQRT_3 = Mathf.Sqrt(3f);

        // Angles pour la vue isométrique (30 degrés standard)
        private const float ISO_ANGLE = 30f;
        private static readonly float ISO_SCALE_X = Mathf.Cos(ISO_ANGLE * Mathf.Deg2Rad);
        private static readonly float ISO_SCALE_Y = Mathf.Sin(ISO_ANGLE * Mathf.Deg2Rad);

        /// <summary>
        /// Convertit des coordonnées hexagonales vers position monde 2D (vue isométrique)
        /// Hexagones flat-top (côté plat en haut)
        /// </summary>
        public static Vector3 HexToWorldPosition(HexCoordinates hex, float hexSize = HEX_SIZE)
        {
            // Position hexagonale standard (flat-top)
            float x = hexSize * (3f/2f * hex.Q);
            float z = hexSize * (SQRT_3 / 2f * hex.Q + SQRT_3 * hex.R);

            // Projection isométrique
            Vector3 worldPos = new Vector3(
                x * ISO_SCALE_X - z * ISO_SCALE_X,
                x * ISO_SCALE_Y + z * ISO_SCALE_Y,
                0f
            );

            return worldPos;
        }

        /// <summary>
        /// Convertit une position monde vers coordonnées hexagonales (approximation)
        /// Nécessite un arrondi avec HexRound()
        /// </summary>
        public static HexCoordinates WorldToHex(Vector3 worldPos, float hexSize = HEX_SIZE)
        {
            // Inverse de la projection isométrique
            float x = (worldPos.x / ISO_SCALE_X + worldPos.y / ISO_SCALE_Y) / 2f;
            float z = (worldPos.y / ISO_SCALE_Y - worldPos.x / ISO_SCALE_X) / 2f;

            // Conversion vers coordonnées hexagonales (fractionnelles)
            float q = (2f/3f * x) / hexSize;
            float r = (-1f/3f * x + SQRT_3/3f * z) / hexSize;

            return HexRound(q, r);
        }

        /// <summary>
        /// Arrondit des coordonnées hexagonales fractionnelles vers les coordonnées entières les plus proches
        /// </summary>
        public static HexCoordinates HexRound(float q, float r)
        {
            float s = -q - r;

            int rq = Mathf.RoundToInt(q);
            int rr = Mathf.RoundToInt(r);
            int rs = Mathf.RoundToInt(s);

            float q_diff = Mathf.Abs(rq - q);
            float r_diff = Mathf.Abs(rr - r);
            float s_diff = Mathf.Abs(rs - s);

            if (q_diff > r_diff && q_diff > s_diff)
            {
                rq = -rr - rs;
            }
            else if (r_diff > s_diff)
            {
                rr = -rq - rs;
            }

            return new HexCoordinates(rq, rr);
        }

        /// <summary>
        /// Retourne les coins d'un hexagone en coordonnées monde
        /// Utile pour dessiner la grille ou les contours
        /// </summary>
        public static Vector3[] GetHexCorners(HexCoordinates hex, float hexSize = HEX_SIZE)
        {
            Vector3 center = HexToWorldPosition(hex, hexSize);
            Vector3[] corners = new Vector3[6];

            for (int i = 0; i < 6; i++)
            {
                float angleDeg = 60f * i;
                float angleRad = angleDeg * Mathf.Deg2Rad;

                // Position du coin en coordonnées hexagonales
                float hexX = hexSize * Mathf.Cos(angleRad);
                float hexZ = hexSize * Mathf.Sin(angleRad);

                // Projection isométrique du coin
                float isoX = hexX * ISO_SCALE_X - hexZ * ISO_SCALE_X;
                float isoY = hexX * ISO_SCALE_Y + hexZ * ISO_SCALE_Y;

                corners[i] = center + new Vector3(isoX, isoY, 0f);
            }

            return corners;
        }

        /// <summary>
        /// Retourne les coins d'un hexagone pour une orientation pointy-top (alternative)
        /// </summary>
        public static Vector3[] GetHexCornersPointy(HexCoordinates hex, float hexSize = HEX_SIZE)
        {
            Vector3 center = HexToWorldPosition(hex, hexSize);
            Vector3[] corners = new Vector3[6];

            for (int i = 0; i < 6; i++)
            {
                float angleDeg = 60f * i + 30f; // +30° pour pointy-top
                float angleRad = angleDeg * Mathf.Deg2Rad;

                float hexX = hexSize * Mathf.Cos(angleRad);
                float hexZ = hexSize * Mathf.Sin(angleRad);

                float isoX = hexX * ISO_SCALE_X - hexZ * ISO_SCALE_X;
                float isoY = hexX * ISO_SCALE_Y + hexZ * ISO_SCALE_Y;

                corners[i] = center + new Vector3(isoX, isoY, 0f);
            }

            return corners;
        }

        /// <summary>
        /// Dessine un hexagone dans la scène (pour debug)
        /// </summary>
        public static void DrawHexGizmo(HexCoordinates hex, Color color, float hexSize = HEX_SIZE, bool pointyTop = false)
        {
            Vector3[] corners = pointyTop ? GetHexCornersPointy(hex, hexSize) : GetHexCorners(hex, hexSize);

            Gizmos.color = color;
            for (int i = 0; i < 6; i++)
            {
                Gizmos.DrawLine(corners[i], corners[(i + 1) % 6]);
            }
        }

        /// <summary>
        /// Dessine une ligne entre deux hexagones
        /// </summary>
        public static void DrawHexConnection(HexCoordinates from, HexCoordinates to, Color color, float hexSize = HEX_SIZE)
        {
            Vector3 fromPos = HexToWorldPosition(from, hexSize);
            Vector3 toPos = HexToWorldPosition(to, hexSize);

            Gizmos.color = color;
            Gizmos.DrawLine(fromPos, toPos);
        }
    }
}
