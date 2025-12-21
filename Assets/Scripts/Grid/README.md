# Système de Grille Hexagonale Isométrique

## Vue d'ensemble

Ce système implémente une grille hexagonale avec rendu isométrique pour Cogs Tower Defense.

## Composants

### 1. **HexCoordinates**
Structure représentant des coordonnées hexagonales en système axial (q, r).

**Fonctionnalités :**
- Conversion offset ↔ axial ↔ cube
- Calcul de distance entre hexagones
- Récupération des voisins (6 directions)
- Opérateurs mathématiques (+, -, ==, !=)

**Exemple :**
```csharp
HexCoordinates hex = new HexCoordinates(2, 3);
HexCoordinates[] neighbors = hex.GetNeighbors();
int distance = hex.DistanceTo(otherHex);
```

### 2. **IsometricUtils**
Utilitaires statiques pour les conversions coordonnées ↔ position monde.

**Fonctionnalités :**
- `HexToWorldPosition()` : Coordonnées hex → Position 2D isométrique
- `WorldToHex()` : Position monde → Coordonnées hex
- `GetHexCorners()` : Récupère les 6 coins d'un hexagone
- `DrawHexGizmo()` : Dessine un hexagone en Gizmos (debug)

**Exemple :**
```csharp
Vector3 worldPos = IsometricUtils.HexToWorldPosition(hexCoords);
HexCoordinates hex = IsometricUtils.WorldToHex(mouseWorldPosition);
```

### 3. **HexCell**
Représente une cellule individuelle de la grille.

**Propriétés :**
- `Coordinates` : Position hexagonale
- `WorldPosition` : Position monde 2D
- `IsOccupied` : Cellule occupée par un objet
- `IsPlaceable` : Peut-on placer des rouages
- `IsWalkable` : Peuvent-elles marcher dessus
- `OccupyingObject` : Référence à l'objet placé

**Exemple :**
```csharp
HexCell cell = hexGrid.GetCell(coords);
if (!cell.IsOccupied && cell.IsPlaceable)
{
    cell.PlaceObject(myGameObject);
}
```

### 4. **HexGrid**
Composant principal gérant toute la grille.

**Paramètres Inspector :**
- `gridWidth` / `gridHeight` : Dimensions de la grille
- `hexSize` : Taille d'un hexagone (1.0 par défaut)
- `showGrid` : Afficher la grille en Gizmos
- Couleurs de visualisation (grid, occupied, placeable, hover)

**API Principale :**
```csharp
// Récupération de cellules
HexCell cell = hexGrid.GetCell(hexCoords);
HexCell cell = hexGrid.GetCellAtWorldPosition(mousePos);

// Placement d'objets
hexGrid.PlaceObject(coords, gameObject);
hexGrid.RemoveObject(coords);

// Queries
List<HexCell> range = hexGrid.GetCellsInRange(center, radius);
IEnumerable<HexCell> all = hexGrid.GetAllCells();
```

### 5. **HexGridRenderer**
Rend visuellement la grille avec des LineRenderers.

**Fonctionnalités :**
- Dessine les contours de tous les hexagones
- Système de highlight pour la cellule survolée
- Configurable (couleurs, épaisseur de ligne)

### 6. **HexGridTester**
Script de test pour interagir avec la grille.

**Contrôles :**
- **Clic gauche** : Place un objet de test
- **Clic droit** : Retire un objet
- **Hover** : Highlight de la cellule

## Setup dans Unity

### Création d'une scène de test

1. Créer un GameObject vide "HexGrid"
2. Ajouter les composants :
   - `HexGrid`
   - `HexGridRenderer`
   - `HexGridTester`
3. Configurer les paramètres dans l'Inspector
4. Lancer la scène et cliquer pour tester

### Paramètres recommandés

**HexGrid :**
- Width : 15
- Height : 10
- Hex Size : 1.0
- Show Grid : ✓

**Caméra :**
- Projection : Orthographic
- Size : 8-10
- Position : (0, 0, -10)

## Système de Coordonnées

### Types de coordonnées

1. **Axial (q, r)** : Système principal
   - q : colonne
   - r : rangée
   - Utilisé en interne

2. **Cube (x, y, z)** : Pour calculs de distance
   - x + y + z = 0
   - x = q, z = r, y = -q - r

3. **Offset (col, row)** : Grille rectangulaire
   - Utilisé pour initialisation
   - Layout "odd-r" (rangées impaires décalées)

### Directions des voisins

```
     2 (NW)    1 (NE)
         \    /
          \  /
3 (W) --- HEX --- 0 (E)
          /  \
         /    \
     4 (SW)    5 (SE)
```

## Performance

- **Stockage** : Dictionary<HexCoordinates, HexCell> - O(1) lookup
- **Voisinage** : Calcul direct sans iteration
- **Rendering** : LineRenderers statiques (pas de update)

## Extensions Futures

- [ ] Pathfinding A* optimisé
- [ ] Fog of War
- [ ] Zones de connexion pour rouages
- [ ] Visualisation des connexions entre rouages
- [ ] Support pour hexagones de tailles différentes
- [ ] Obstacles et terrain varié

## Références

- [Red Blob Games - Hexagonal Grids](https://www.redblobgames.com/grids/hexagons/)
- Orientation : Flat-top hexagons
- Layout : Odd-r offset
- Projection : Isométrique 30°
