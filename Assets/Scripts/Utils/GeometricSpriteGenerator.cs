using UnityEngine;

namespace CogsTowerDefense.Utils
{
    /// <summary>
    /// Génère des sprites géométriques simples pour les placeholders
    /// </summary>
    public static class GeometricSpriteGenerator
    {
        /// <summary>
        /// Crée un sprite hexagonal
        /// </summary>
        public static Sprite CreateHexagonSprite(int size, Color color)
        {
            int texSize = size * 2;
            Texture2D tex = new Texture2D(texSize, texSize);
            Color[] pixels = new Color[texSize * texSize];

            // Remplit avec du transparent
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.clear;
            }

            // Dessine un hexagone
            Vector2 center = new Vector2(texSize / 2f, texSize / 2f);
            float radius = size * 0.9f;

            for (int y = 0; y < texSize; y++)
            {
                for (int x = 0; x < texSize; x++)
                {
                    Vector2 point = new Vector2(x, y);
                    if (IsPointInHexagon(point, center, radius))
                    {
                        pixels[y * texSize + x] = color;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            tex.filterMode = FilterMode.Point; // Pixel-perfect

            return Sprite.Create(tex, new Rect(0, 0, texSize, texSize), new Vector2(0.5f, 0.5f), size);
        }

        /// <summary>
        /// Crée un sprite circulaire (pour le moteur)
        /// </summary>
        public static Sprite CreateCircleSprite(int size, Color color, bool outline = true)
        {
            int texSize = size * 2;
            Texture2D tex = new Texture2D(texSize, texSize);
            Color[] pixels = new Color[texSize * texSize];

            Vector2 center = new Vector2(texSize / 2f, texSize / 2f);
            float radius = size * 0.9f;
            float innerRadius = outline ? radius * 0.7f : 0;

            for (int y = 0; y < texSize; y++)
            {
                for (int x = 0; x < texSize; x++)
                {
                    Vector2 point = new Vector2(x, y);
                    float dist = Vector2.Distance(point, center);

                    if (dist <= radius && dist >= innerRadius)
                    {
                        pixels[y * texSize + x] = color;
                    }
                    else
                    {
                        pixels[y * texSize + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            tex.filterMode = FilterMode.Point;

            return Sprite.Create(tex, new Rect(0, 0, texSize, texSize), new Vector2(0.5f, 0.5f), size);
        }

        /// <summary>
        /// Crée un sprite d'engrenage simplifié
        /// </summary>
        public static Sprite CreateGearSprite(int size, Color color, int teeth = 6)
        {
            int texSize = size * 2;
            Texture2D tex = new Texture2D(texSize, texSize);
            Color[] pixels = new Color[texSize * texSize];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.clear;
            }

            Vector2 center = new Vector2(texSize / 2f, texSize / 2f);
            float baseRadius = size * 0.6f;
            float toothRadius = size * 0.9f;

            for (int y = 0; y < texSize; y++)
            {
                for (int x = 0; x < texSize; x++)
                {
                    Vector2 point = new Vector2(x, y);
                    float dist = Vector2.Distance(point, center);
                    float angle = Mathf.Atan2(point.y - center.y, point.x - center.x) * Mathf.Rad2Deg;

                    // Calcule si on est sur une dent
                    float normalizedAngle = (angle + 360f) % 360f;
                    float toothAngle = 360f / teeth;
                    bool onTooth = (normalizedAngle % toothAngle) < (toothAngle * 0.4f);

                    float maxRadius = onTooth ? toothRadius : baseRadius;

                    if (dist <= maxRadius && dist >= baseRadius * 0.4f)
                    {
                        pixels[y * texSize + x] = color;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            tex.filterMode = FilterMode.Point;

            return Sprite.Create(tex, new Rect(0, 0, texSize, texSize), new Vector2(0.5f, 0.5f), size);
        }

        /// <summary>
        /// Vérifie si un point est dans un hexagone
        /// </summary>
        private static bool IsPointInHexagon(Vector2 point, Vector2 center, float radius)
        {
            Vector2[] corners = new Vector2[6];
            for (int i = 0; i < 6; i++)
            {
                float angle = 60f * i * Mathf.Deg2Rad;
                corners[i] = center + new Vector2(
                    radius * Mathf.Cos(angle),
                    radius * Mathf.Sin(angle)
                );
            }

            // Utilise la méthode du raycasting pour vérifier si le point est dans le polygone
            bool inside = false;
            for (int i = 0, j = 5; i < 6; j = i++)
            {
                if (((corners[i].y > point.y) != (corners[j].y > point.y)) &&
                    (point.x < (corners[j].x - corners[i].x) * (point.y - corners[i].y) / (corners[j].y - corners[i].y) + corners[i].x))
                {
                    inside = !inside;
                }
            }

            return inside;
        }
    }
}
