using UnityEngine;

namespace ClogTowerDefense.Grid
{
    /// <summary>
    /// Rend visuellement la grille hexagonale avec des LineRenderers ou sprites
    /// </summary>
    [RequireComponent(typeof(HexGrid))]
    public class HexGridRenderer : MonoBehaviour
    {
        [Header("Rendering")]
        [SerializeField] private bool renderGrid = true;
        [SerializeField] private Material lineMaterial;
        [SerializeField] private Color lineColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        [SerializeField] private float lineWidth = 0.05f;

        [Header("Cell Highlighting")]
        [SerializeField] private GameObject cellHighlightPrefab;
        [SerializeField] private Color highlightColor = new Color(0f, 1f, 0f, 0.3f);

        private HexGrid hexGrid;
        private GameObject gridLinesParent;
        private GameObject currentHighlight;

        private void Awake()
        {
            hexGrid = GetComponent<HexGrid>();
        }

        private void Start()
        {
            if (renderGrid)
            {
                RenderGridLines();
            }
        }

        /// <summary>
        /// Génère les lignes de la grille avec LineRenderer
        /// </summary>
        private void RenderGridLines()
        {
            if (gridLinesParent != null)
            {
                Destroy(gridLinesParent);
            }

            gridLinesParent = new GameObject("GridLines");
            gridLinesParent.transform.SetParent(transform);

            foreach (var cell in hexGrid.GetAllCells())
            {
                CreateHexOutline(cell);
            }
        }

        /// <summary>
        /// Crée le contour d'un hexagone avec LineRenderer
        /// </summary>
        private void CreateHexOutline(HexCell cell)
        {
            GameObject lineObj = new GameObject($"Hex_{cell.Coordinates}");
            lineObj.transform.SetParent(gridLinesParent.transform);
            lineObj.transform.position = cell.WorldPosition;

            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.material = lineMaterial;
            lr.startColor = lineColor;
            lr.endColor = lineColor;
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;
            lr.loop = true;
            lr.useWorldSpace = false;

            // Génère les coins de l'hexagone
            Vector3[] corners = IsometricUtils.GetHexCorners(cell.Coordinates, hexGrid.HexSize);
            Vector3 center = cell.WorldPosition;

            // Convertit en coordonnées locales
            Vector3[] localCorners = new Vector3[6];
            for (int i = 0; i < 6; i++)
            {
                localCorners[i] = corners[i] - center;
            }

            lr.positionCount = 6;
            lr.SetPositions(localCorners);

            // Assure que le renderer est sur le bon layer
            lr.sortingLayerName = "Ground";
            lr.sortingOrder = 0;
        }

        /// <summary>
        /// Affiche un highlight sur une cellule spécifique
        /// </summary>
        public void HighlightCell(HexCell cell, Color? color = null)
        {
            if (cell == null)
            {
                ClearHighlight();
                return;
            }

            if (currentHighlight == null)
            {
                if (cellHighlightPrefab != null)
                {
                    currentHighlight = Instantiate(cellHighlightPrefab, transform);
                }
                else
                {
                    // Crée un highlight par défaut
                    currentHighlight = CreateDefaultHighlight();
                }
            }

            currentHighlight.transform.position = cell.WorldPosition;

            // Change la couleur si fournie
            if (color.HasValue)
            {
                SpriteRenderer sr = currentHighlight.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = color.Value;
                }
            }

            currentHighlight.SetActive(true);
        }

        /// <summary>
        /// Retire le highlight
        /// </summary>
        public void ClearHighlight()
        {
            if (currentHighlight != null)
            {
                currentHighlight.SetActive(false);
            }
        }

        /// <summary>
        /// Crée un highlight hexagonal par défaut
        /// </summary>
        private GameObject CreateDefaultHighlight()
        {
            GameObject highlight = new GameObject("CellHighlight");
            highlight.transform.SetParent(transform);

            // Crée un sprite hexagonal simple ou utilise un quad
            SpriteRenderer sr = highlight.AddComponent<SpriteRenderer>();
            sr.color = highlightColor;
            sr.sortingLayerName = "Ground";
            sr.sortingOrder = 1;

            // TODO: Créer un sprite hexagonal procédural ou assigner un sprite
            // Pour l'instant, utilise un quad simple
            sr.sprite = CreateHexagonSprite();

            return highlight;
        }

        /// <summary>
        /// Crée un sprite hexagonal procédural (placeholder)
        /// </summary>
        private Sprite CreateHexagonSprite()
        {
            // Crée une texture simple pour le moment
            // TODO: Générer un vrai hexagone ou utiliser un sprite asset
            Texture2D tex = new Texture2D(64, 64);
            Color[] pixels = new Color[64 * 64];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = highlightColor;
            }
            tex.SetPixels(pixels);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
        }

        /// <summary>
        /// Régénère les lignes de grille (utile pour l'éditeur)
        /// </summary>
        [ContextMenu("Regenerate Grid Lines")]
        public void RegenerateGridLines()
        {
            RenderGridLines();
        }

        private void OnDestroy()
        {
            if (gridLinesParent != null)
            {
                Destroy(gridLinesParent);
            }
        }
    }
}
