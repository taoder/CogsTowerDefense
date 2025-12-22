# 🏗️ Architecture Technique - Cogs Tower Defense

## 📁 Structure des Dossiers Unity

```
Assets/
├── Scripts/
│   ├── Core/                      # Systèmes centraux
│   │   ├── GameManager.cs         # Gestionnaire principal du jeu
│   │   ├── LevelManager.cs        # Gestion des niveaux
│   │   ├── WaveManager.cs         # Gestion des vagues de monstres
│   │   ├── ResourceManager.cs     # Gestion Or + Points Meta
│   │   └── GameState.cs           # Enum des états du jeu
│   │
│   ├── Cogs/                      # Système de rouages
│   │   ├── Engine.cs              # Moteur
│   │   ├── Cog.cs                 # Rouage de base
│   │   ├── CogSize.cs             # Enum (Small, Medium, Large)
│   │   ├── CogData.cs             # ScriptableObject config rouage
│   │   ├── CogChain.cs            # Chaîne de rouages connectés
│   │   └── CogPlacementSystem.cs  # Système de placement
│   │
│   ├── Units/                     # Système d'unités
│   │   ├── Unit.cs                # Classe de base unité
│   │   ├── MeleeUnit.cs           # Unité corps à corps
│   │   ├── RangedUnit.cs          # Unité distance
│   │   ├── AoEUnit.cs             # Unité dégâts de zone
│   │   ├── TankUnit.cs            # Unité tank
│   │   ├── UnitData.cs            # ScriptableObject config unité
│   │   └── UnitSpawner.cs         # Production depuis rouages
│   │
│   ├── Enemies/                   # Système d'ennemis
│   │   ├── Enemy.cs               # Classe de base ennemi
│   │   ├── GroundEnemy.cs         # Ennemi terrestre
│   │   ├── FlyingEnemy.cs         # Ennemi volant
│   │   ├── BossEnemy.cs           # Boss
│   │   ├── EnemyData.cs           # ScriptableObject config ennemi
│   │   └── EnemySpawner.cs        # Spawn des vagues
│   │
│   ├── Combat/                    # Système de combat
│   │   ├── Health.cs              # Composant PV
│   │   ├── Damageable.cs          # Interface pour dégâts
│   │   ├── Attacker.cs            # Logique d'attaque
│   │   └── Projectile.cs          # Projectiles unités distance
│   │
│   ├── UI/                        # Interface utilisateur
│   │   ├── MainMenuUI.cs          # Menu principal
│   │   ├── GameHUD.cs             # HUD en jeu
│   │   ├── CogUpgradePanel.cs     # Panneau upgrade rouage
│   │   ├── EngineStatsUI.cs       # Jauge puissance moteur
│   │   ├── WaveIndicatorUI.cs     # Indicateur de vague
│   │   └── MetaProgressionUI.cs   # Écran déblocages
│   │
│   ├── Grid/                      # Système de grille
│   │   ├── GridSystem.cs          # Grille isométrique
│   │   ├── GridCell.cs            # Cellule de grille
│   │   └── IsometricUtils.cs      # Conversions coordonnées
│   │
│   ├── Data/                      # Données de configuration
│   │   ├── LevelData.cs           # ScriptableObject niveau
│   │   ├── WaveData.cs            # ScriptableObject vague
│   │   └── GameConfig.cs          # Config globale
│   │
│   └── Utils/                     # Utilitaires
│       ├── ObjectPool.cs          # Pool d'objets
│       ├── EventManager.cs        # Système d'événements
│       └── SaveSystem.cs          # Sauvegarde progression
│
├── Prefabs/
│   ├── Cogs/
│   ├── Units/
│   ├── Enemies/
│   └── UI/
│
├── ScriptableObjects/
│   ├── Cogs/
│   ├── Units/
│   ├── Enemies/
│   ├── Levels/
│   └── Waves/
│
├── Sprites/                       # Assets visuels
│   ├── Placeholders/              # Géométrie temporaire
│   ├── PixelArt/                  # Art final (futur)
│   └── UI/
│
├── Scenes/
│   ├── MainMenu.unity
│   ├── GameScene.unity
│   └── MetaProgression.unity
│
└── Resources/
    └── Configs/
```

---

## 🎯 Patterns de Conception Utilisés

### 1. **Singleton Pattern**
- `GameManager` : Point d'entrée unique
- `ResourceManager` : Gestion centralisée des ressources
- `EventManager` : Bus d'événements global

### 2. **State Machine Pattern**
- États du jeu : `MainMenu → Construction → Combat → Victory/Defeat → MetaProgression`
- Transitions claires entre phases

### 3. **Observer Pattern (Events)**
- Découplage via `EventManager`
- Exemples :
  - `OnCogDestroyed` → UI update vies + VFX
  - `OnGoldChanged` → Update UI or
  - `OnWaveComplete` → Transition phase

### 4. **Object Pool Pattern**
- Pool de projectiles
- Pool d'ennemis
- Pool d'unités
- Performance optimisée (pas de GC)

### 5. **ScriptableObject Architecture**
- Configuration data-driven
- Balance facile sans recompilation
- Partage de données entre instances

### 6. **Component-Based Design**
- Unity ECS-like approach
- `Health`, `Attacker`, `Movement` = composants réutilisables

---

## 🔧 Systèmes Principaux

### **1. GameManager (Orchestrateur Central)**

```
Responsabilités :
- Initialisation du jeu
- Gestion du state machine (Construction/Combat)
- Transitions entre phases
- Conditions victoire/défaite

État :
- currentState : GameState
- livesRemaining : int
- currentLevel : int

Méthodes clés :
- StartConstructionPhase()
- StartCombatPhase()
- OnCogDestroyed() → livesRemaining--
- CheckGameOver()
```

### **2. Engine (Moteur)**

```
Propriétés :
- maxPower : int (améliorable)
- rotationSpeed : float (améliorable)
- currentPowerUsed : int
- connectedCogs : List<Cog>

Méthodes :
- CanAddCog(Cog cog) : bool
- RegisterCog(Cog cog)
- UnregisterCog(Cog cog)
- UpgradePower(int amount)
- UpgradeSpeed(float amount)
- GetPowerStatus() : PowerStatus (OK/Warning/Overload)

Update :
- Rotation visuelle continue si currentPowerUsed <= maxPower
- Arrêt si surchauffe
```

### **3. Cog (Rouage)**

```
Propriétés :
- cogSize : CogSize (Small/Medium/Large)
- level : int
- rotationProgress : float (0-100%)
- unitToSpawn : UnitData
- powerRequired : int (calculé dynamiquement)

Méthodes :
- CalculatePowerRequired() : int
  → basePower = size.value
  → basePower + (level / 10)

- UpdateRotation(deltaTime, engineSpeed)
  → rotationProgress += deltaTime * engineSpeed
  → if progress >= 100% → SpawnUnit()

- Upgrade(int cost) : bool
  → level++
  → Recalcule powerRequired
  → Vérifie engine.CanSupport()

- TakeDamage(int damage)
  → Health component gère
  → OnDestroy → Event → GameManager perd vie
```

### **4. CogChain (Validation Chaîne)**

```
Responsabilités :
- Vérifier connexion rouages au moteur
- Calculer puissance totale requise
- Détection surchauffe

Méthodes :
- ValidateChain(Engine engine) : bool
- GetTotalPowerRequired() : int
- GetChainEfficiency() : float
```

### **5. WaveManager (Gestion Vagues)**

```
Propriétés :
- currentWave : int
- waveData : WaveData[]
- spawnTimer : float
- enemiesRemaining : int

Flow :
1. LoadWave(waveIndex)
2. SpawnEnemies() (coroutine avec délai)
3. Track enemiesRemaining
4. OnWaveComplete → Pause → Next wave

Méthodes :
- StartWave()
- SpawnEnemy(EnemyData data, Vector3 position)
- OnEnemyKilled(Enemy enemy)
  → ResourceManager.AddGold(enemy.goldReward)
  → enemiesRemaining--
```

### **6. ResourceManager (Économie)**

```
Propriétés :
- currentGold : int
- metaPoints : int

Méthodes :
- AddGold(int amount)
- SpendGold(int amount) : bool
- AddMetaPoints(int amount)
- CanAfford(int cost) : bool

Events :
- OnGoldChanged(int newAmount)
- OnMetaPointsChanged(int newAmount)
```

### **7. Unit (Unité Base)**

```
Composants :
- Health
- Movement (vers cible)
- Attacker

Propriétés :
- unitLevel : int
- damage : int
- attackSpeed : float
- moveSpeed : float

Comportement AI :
1. FindNearestEnemy()
2. MoveTowards(target)
3. if InRange(target) → Attack()
4. Repeat
```

### **8. Enemy (Ennemi Base)**

```
Composants :
- Health
- Movement (vers rouages)
- Attacker

Propriétés :
- goldReward : int
- targetCog : Cog (priorité plus proche)

Comportement AI :
1. FindNearestCog()
2. MoveTowards(targetCog)
3. if InRange(targetCog) → AttackCog()
4. OnDeath → Drop gold
```

---

## 🎮 Flow du Jeu (State Machine)

```
┌─────────────┐
│ MAIN MENU   │
└──────┬──────┘
       │ [Start Game]
       ▼
┌─────────────────────┐
│ META PROGRESSION    │ ◄──────┐
│ - Unlock units      │        │
│ - Spend meta points │        │
└──────┬──────────────┘        │
       │ [Start Level]          │
       ▼                        │
┌─────────────────────┐        │
│ CONSTRUCTION PHASE  │        │
│ - Place cogs        │        │
│ - Timer : 60s       │        │
│ - Cogs static       │        │
└──────┬──────────────┘        │
       │ [Timer ends]           │
       ▼                        │
┌─────────────────────┐        │
│ COMBAT PHASE        │        │
│ - Cogs rotate       │        │
│ - Units spawn       │        │
│ - Waves attack      │        │
│ - Real-time upgrade │        │
└──────┬──────────────┘        │
       │                        │
       ├─[All waves clear]──────┤
       │    VICTORY             │
       │    + Meta points       │
       │                        │
       └─[Lives = 0]────────────┤
            DEFEAT              │
            Retry with upgrades ─┘
```

---

## 📊 Data Architecture (ScriptableObjects)

### **CogData.cs**
```csharp
- cogSize : CogSize
- basePowerCost : int
- rotationTimeAtLevel1 : float
- goldCost : int
- upgradeCostCurve : AnimationCurve
- unitToProduce : UnitData
- sprite : Sprite
```

### **UnitData.cs**
```csharp
- unitType : UnitType (Melee, Ranged, AoE, Tank)
- baseHealth : int
- baseDamage : int
- attackSpeed : float
- moveSpeed : float
- range : float
- scalingPerLevel : StatScaling
- sprite : Sprite
```

### **EnemyData.cs**
```csharp
- enemyType : EnemyType (Ground, Flying, Boss)
- health : int
- damage : int
- moveSpeed : float
- goldReward : int
- sprite : Sprite
```

### **WaveData.cs**
```csharp
- waveNumber : int
- enemies : EnemySpawnInfo[] (type + count + delay)
- difficulty : float
```

### **LevelData.cs**
```csharp
- levelID : int
- enginePosition : Vector3
- waves : WaveData[]
- startingGold : int
- startingLives : int
- gridBounds : Bounds
```

---

## 🎨 Mockups des Écrans Principaux

### **1. Construction Phase**
```
┌────────────────────────────────────────────────┐
│ ⏱️ Construction : 45s    💰 Gold: 500    ❤️ x3 │
├────────────────────────────────────────────────┤
│                                                │
│         [Grille Isométrique]                   │
│              🔧 MOTEUR                         │
│         🟢 Jauge: 4/10 Puissance              │
│                                                │
│          ⚙️   ⚙️   ⚙️                         │
│              (Rouages placés)                  │
│                                                │
├────────────────────────────────────────────────┤
│ 📦 INVENTORY                                   │
│  [⚙️ Small] 50G   [⚙️ Med] 100G  [⚙️ Large] 200G│
└────────────────────────────────────────────────┘
```

### **2. Combat Phase**
```
┌────────────────────────────────────────────────┐
│ Wave 2/5    💰 Gold: 750    ❤️ x2    📊 Score  │
├────────────────────────────────────────────────┤
│  🟢 Puissance: 7/10                            │
│                                                │
│    👹 Enemies (5)        🔧 MOTEUR             │
│         ↓                                      │
│    [Combat Area]          ⚙️(Lv3)⚙️(Lv1)      │
│                                                │
│    🗡️🗡️ Units defending                       │
│                                                │
├────────────────────────────────────────────────┤
│ [📈 Upgrade Cog] [🔧 Upgrade Engine] [➕ Add] │
└────────────────────────────────────────────────┘
```

### **3. Écran Upgrade Rouage (Popup)**
```
┌──────────────────────────────┐
│   Upgrade Cog                │
├──────────────────────────────┤
│   ⚙️ Small Cog (Melee)       │
│                              │
│   Level: 5 → 6               │
│   Power: 1 → 1               │
│   Production: 10s → 9.5s     │
│   Unit Dmg: 15 → 18          │
│                              │
│   Cost: 120 💰               │
│                              │
│   [Cancel]  [✓ Upgrade]      │
└──────────────────────────────┘
```

### **4. Meta Progression**
```
┌────────────────────────────────────────────────┐
│          🎖️ META PROGRESSION                  │
├────────────────────────────────────────────────┤
│  Meta Points: 250 ⭐                           │
│                                                │
│  UNLOCK UNITS:                                 │
│  ✅ Melee Unit (Free)                          │
│  ✅ Ranged Unit (100⭐)                        │
│  🔒 AoE Unit (250⭐)  ← [UNLOCK]              │
│  🔒 Tank Unit (400⭐)                          │
│                                                │
│  PERMANENT UPGRADES:                           │
│  🔧 Starting Gold +10% (50⭐) [BUY]            │
│  ❤️ +1 Starting Life (150⭐) [BUY]            │
│                                                │
│            [Continue to Level 5]               │
└────────────────────────────────────────────────┘
```

---

## 🔄 Ordre d'Implémentation Recommandé

### **Phase 1 : Fondations (Semaine 1)**
1. Setup projet Unity 6 LTS ✅
2. Grille isométrique + conversions coordonnées ✅
3. GameManager + State Machine basique
4. ResourceManager + UI basique

### **Phase 2 : Système Rouages (Semaine 2)** ✅
5. Engine + indicateur puissance
6. Cog (placement, rotation visuelle)
7. CogChain validation
8. Système upgrade rouages
9. UI panels upgrade

### **Phase 3 : Système Combat (Semaine 3)**
10. Unit base + Movement + Attack
11. Enemy base + AI vers rouages
12. Health + Damageable components
13. Projectiles (unités distance)
14. Object Pooling

### **Phase 4 : Gameplay (Semaine 4)**
15. WaveManager + spawn vagues
16. Construction Phase timer
17. Combat Phase flow
18. Victory/Defeat conditions
19. Gold reward system

### **Phase 5 : Polish (Semaine 5)**
20. Meta Progression screen
21. Save System
22. Balance tuning (ScriptableObjects)
23. SFX placeholders
24. UI polish

### **Phase 6 : Content (Semaine 6+)**
25. Multiple unit types
26. Multiple enemy types
27. Level design (5-10 niveaux)
28. Wave design
29. Pixel art integration

---

## ⚠️ Points Techniques Critiques

### **1. Grille Isométrique**
- Conversion screen → iso coords
- Snap rouages sur grille
- Détection voisinage (hexagonal vs carré)

### **2. Performance**
- Object Pooling obligatoire (100+ unités/ennemis)
- Pas de Find() dans Update()
- Events pour découplage

### **3. Équilibrage**
- Toutes les valeurs dans ScriptableObjects
- Courbes d'upgrade (AnimationCurve)
- Fichier balance Excel → JSON import

### **4. Sauvegarde**
- Progression méta : PlayerPrefs ou JSON
- État en jeu : Pas de sauvegarde mid-level (reset)

---

## 📝 Prochaines Étapes

Avant de commencer le code, confirmez :

1. ✅ Cette architecture vous convient-elle ?
2. ❓ Y a-t-il des systèmes à ajouter/modifier ?
3. ❓ Ordre d'implémentation OK ?
4. ❓ On commence par quel système ? (Je recommande : Grille → Engine → Cog)

Une fois validé, je peux :
- Créer la structure de dossiers Unity
- Implémenter le premier système (Grille isométrique)
- Créer les ScriptableObject templates
