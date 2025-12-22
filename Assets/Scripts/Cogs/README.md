# Système de Rouages (Cogs System)

## Vue d'ensemble

Le système de rouages est le cœur mécanique de Cogs Tower Defense. Les rouages tournent grâce au moteur et produisent des unités pour combattre les ennemis.

## Composants

### 1. **Engine (Moteur)**

Le moteur central qui fournit la puissance et fait tourner les rouages.

**Propriétés clés :**
- `maxPower` : Puissance maximale disponible
- `rotationSpeed` : Vitesse de rotation (rotations/seconde)
- `currentPowerUsed` : Puissance actuellement consommée
- `status` : État (OK / Warning / Overload)

**Mécaniques :**
- Démarre/Arrête lors des transitions de phase
- S'arrête automatiquement en cas de surcharge
- Peut être amélioré (puissance + vitesse)
- Fait tourner tous les rouages connectés

**API Principale :**
```csharp
Engine engine = GetComponent<Engine>();

// Connecter un rouage
engine.RegisterCog(cog);

// Vérifier capacité
bool canAdd = engine.CanAddCog(powerRequired);

// Améliorer
engine.UpgradePower(5);    // +5 puissance max
engine.UpgradeSpeed(0.5f); // +0.5 vitesse

// Contrôle
engine.StartEngine();
engine.StopEngine();
```

### 2. **Cog (Rouage)**

Rouage qui tourne et produit des unités.

**Propriétés clés :**
- `cogSize` : Small / Medium / Large
- `level` : Niveau actuel (1-∞)
- `unitType` : Type d'unité produite
- `powerRequired` : Puissance nécessaire
- `rotationProgress` : Progrès de production (0-100%)

**Calculs importants :**
```
Puissance requise = taille + (niveau / 10)
Exemples :
- Small Lv1  → 1 + 0 = 1
- Small Lv10 → 1 + 1 = 2
- Large Lv25 → 3 + 2 = 5
```

**API Principale :**
```csharp
Cog cog = GetComponent<Cog>();

// Connecter au moteur
cog.ConnectToEngine(engine);

// Améliorer
cog.Upgrade(cost);

// Changer le type d'unité
cog.SetUnitType(UnitType.Ranged);
```

### 3. **CogData (Configuration)**

ScriptableObject contenant la configuration d'un type de rouage.

**Propriétés :**
- Configuration de base (taille, type, coûts)
- Paramètres de production
- Visuels (prefab, icon, couleur)

**Création :**
```
Assets → Create → Cogs Tower Defense → Cog Data
```

**Utilisation :**
```csharp
CogData smallMeleeCog; // Assigner dans l'Inspector

int cost = smallMeleeCog.GetUpgradeCost(5);
int power = smallMeleeCog.GetPowerRequired(10);
```

### 4. **CogChain (Validation des Chaînes)**

Gère les connexions entre rouages et moteur.

**Responsabilités :**
- Valide que les rouages sont connectés au moteur
- Utilise BFS pour trouver les chemins
- Recalcule automatiquement après suppression
- Différencie rouages connectés vs déconnectés

**Règles de Connexion :**
- Un rouage doit être adjacent au moteur OU à un autre rouage connecté
- La chaîne se propage de proche en proche
- Suppression d'un rouage peut déconnecter d'autres rouages

**API Principale :**
```csharp
CogChain chain = GetComponent<CogChain>();

// Enregistrer un rouage
chain.RegisterCog(cog, hexPosition);

// Vérifier si placement possible
bool canPlace = chain.CanPlaceCog(position, cog);

// Obtenir statistiques
int total = chain.GetTotalCogCount();
int connected = chain.GetConnectedCogCount();
```

### 5. **CogPlacementSystem (Placement)**

Système de placement interactif sur la grille.

**Fonctionnalités :**
- Preview en temps réel
- Feedback visuel (vert = valide, rouge = invalide)
- Snapping automatique sur la grille hexagonale
- Validation avant placement

**Utilisation :**
```csharp
CogPlacementSystem placement = GetComponent<CogPlacementSystem>();

// Démarrer le placement
placement.StartPlacement(cogData);

// Annuler
placement.CancelPlacement();

// Changer le type
placement.SetCogToPlace(otherCogData);
```

**Inputs :**
- **Souris hover** : Preview + validation
- **Clic gauche** : Placer le rouage
- **Clic droit** : Annuler

## Flow du Système

### Phase de Construction

1. Joueur sélectionne un type de rouage (CogData)
2. `CogPlacementSystem` active le mode placement
3. Preview suit la souris avec feedback visuel
4. Clic → Instanciation du rouage
5. `CogChain` valide la connexion au moteur
6. Si valide : `Cog.ConnectToEngine()`
7. `Engine` enregistre le rouage

### Phase de Combat

1. `Engine.StartEngine()` appelé
2. Le moteur fait tourner tous les rouages connectés
3. Chaque rouage accumule de la rotation
4. À 100% → Production d'une unité
5. Reset du compteur

### Surcharge

```
Si currentPowerUsed > maxPower:
  → PowerStatus = Overload
  → Engine.isRunning = false
  → TOUTE la chaîne s'arrête
  → Joueur doit :
    - Supprimer des rouages
    - OU améliorer le moteur
```

## Enums

### CogSize
```csharp
Small  = 1  // Rapide, peu de puissance
Medium = 2  // Équilibré
Large  = 3  // Lent, beaucoup de puissance
```

### PowerStatus
```csharp
OK       // < 80% puissance utilisée (vert)
Warning  // 80-100% puissance utilisée (jaune)
Overload // > 100% puissance utilisée (rouge) - ARRÊT
```

### UnitType
```csharp
Melee   // Corps à corps
Ranged  // Distance
AoE     // Dégâts de zone
Tank    // Tank défensif
```

## Exemples d'Utilisation

### Créer une Scène avec Moteur et Rouages

```csharp
// 1. Créer le moteur
GameObject engineObj = new GameObject("Engine");
Engine engine = engineObj.AddComponent<Engine>();
engine.SetGridPosition(new HexCoordinates(0, 0));

// 2. Créer la chaîne
GameObject chainObj = new GameObject("CogChain");
CogChain chain = chainObj.AddComponent<CogChain>();

// 3. Créer un rouage
GameObject cogObj = Instantiate(cogPrefab);
Cog cog = cogObj.GetComponent<Cog>();

// 4. Enregistrer dans la chaîne
chain.RegisterCog(cog, new HexCoordinates(1, 0)); // Adjacent au moteur

// 5. Démarrer le moteur
engine.StartEngine();
```

### Gérer les Upgrades

```csharp
// Améliorer un rouage
if (playerGold >= cog.GetUpgradeCost())
{
    playerGold -= cost;

    if (cog.Upgrade(cost))
    {
        Debug.Log($"Upgraded to level {cog.Level}");
    }
    else
    {
        playerGold += cost; // Rembourser si échec
        Debug.Log("Upgrade failed: Would overload engine");
    }
}
```

## Gizmos de Debug

### Engine
- **Sphère** colorée selon le statut (vert/jaune/rouge)
- **Lignes cyan** vers tous les rouages connectés

### Cog
- **Cube** (taille proportionnelle à CogSize)
- Couleur : vert si connecté, rouge sinon
- **Ligne jaune** verticale = progrès de production

### CogChain
- **Sphère cyan** autour du moteur
- **Lignes vertes** : Connexions valides
- **Cubes rouges** : Rouages déconnectés

## Points Techniques

### Performance
- Dictionary<HexCoordinates, Cog> pour lookup O(1)
- BFS optimisé pour validation des chaînes
- Recalcul uniquement quand nécessaire

### Thread Safety
- Tous les composants sont thread-safe
- Pas de coroutines (Update simple)

### Extensibilité
- CogData permet d'ajouter facilement de nouveaux types
- UnitType enum extensible
- Système modulaire et découplé

## Prochaines Étapes

- [ ] Intégration avec le système d'unités
- [ ] UI de gestion des rouages
- [ ] Système d'économie (or)
- [ ] Visual effects de rotation
- [ ] Particules de production
- [ ] Sons (rotation, production, surcharge)

## Références

- Architecture.md - Design global
- Grid/README.md - Système de grille hexagonale
