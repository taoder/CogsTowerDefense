# 🎮 Clog Tower Defense

Un jeu de tower defense unique où vous construisez des chaînes de rouages pour produire des unités qui défendent contre les vagues de monstres.

## 📖 Concept

**Clog Tower Defense** combine mécanique de production et tower defense :

### Phase 1 : Construction
- Placez des rouages sur une grille hexagonale isométrique
- Connectez-les à un moteur central
- Gérez la puissance du moteur (nombre limité de rouages)
- Chaque rouage produit un type d'unité spécifique

### Phase 2 : Défense
- Les rouages tournent et produisent des unités
- Les unités combattent automatiquement les monstres
- Améliorez vos rouages en temps réel avec l'or gagné
- Protégez vos rouages : chaque rouage détruit = -1 vie

## 🎯 Mécaniques Principales

### Système de Rouages
- **3 tailles** : Petit (rapide), Moyen (équilibré), Grand (lent mais puissant)
- **Niveaux** : Amélioration augmente la force des unités ET la vitesse de production
- **Puissance** : Chaque rouage consomme de la puissance du moteur
  - Formule : `puissance = taille + (niveau / 10)`
  - Si puissance totale > puissance moteur → ARRÊT COMPLET

### Système de Moteur
- **Puissance** : Limite le nombre de rouages (améliorable)
- **Vitesse** : Affecte la vitesse de rotation des rouages (améliorable)
- **Indicateur visuel** : Jauge de puissance (Vert/Orange/Rouge)

### Types d'Unités
- Corps à corps
- Distance
- Dégâts de zone (AoE)
- Tank

### Ennemis
- Terrestres (basique, rapide, tank)
- Volants (nécessitent unités distance)
- Boss de fin de vague

### Économie
- **Or** : Placement et upgrade de rouages (gagné en tuant des monstres)
- **Points Méta** : Déblocage permanent d'unités et upgrades (gagné en fin de niveau)
- **Vies** : Perdre un rouage = -1 vie

## 🏗️ État Actuel du Projet

### ✅ Implémenté

**Phase 1 : Grille Hexagonale Isométrique**
- ✅ Système de coordonnées hexagonales (axial, cube, offset)
- ✅ Conversion coordonnées ↔ position monde isométrique
- ✅ Gestion des cellules (placement, occupation, voisinage)
- ✅ Rendering avec LineRenderer + Gizmos
- ✅ Système de highlight interactif
- ✅ Scène de test avec placement d'objets

### 🔜 À Implémenter

**Phase 2 : Système de Rouages** (Prochaine étape)
- Engine (moteur central)
- Cog (rouages placables)
- CogChain (validation des connexions)
- Système de rotation visuelle
- Système d'upgrade

**Phase 3 : Système de Combat**
- Classes Unit de base
- Types d'unités (Melee, Ranged, AoE, Tank)
- Classes Enemy de base
- Système Health + Damage
- Projectiles

**Phase 4 : Gameplay**
- GameManager + State Machine
- WaveManager
- ResourceManager
- UI de jeu (HUD, upgrade panels)
- Victory/Defeat conditions

**Phase 5 : Progression**
- Système de méta-progression
- Save/Load
- Déblocage d'unités
- Level design

## 📁 Structure du Projet

```
Assets/
├── Scripts/
│   ├── Core/           # Managers centraux (GameManager, ResourceManager, etc.)
│   ├── Cogs/           # Système de rouages et moteur
│   ├── Units/          # Unités produites
│   ├── Enemies/        # Ennemis
│   ├── Combat/         # Système de combat
│   ├── UI/             # Interface utilisateur
│   ├── Grid/ ✅        # Grille hexagonale (IMPLÉMENTÉ)
│   ├── Data/           # ScriptableObjects
│   └── Utils/          # Utilitaires
├── Prefabs/
├── ScriptableObjects/
├── Sprites/
│   └── Placeholders/   # Formes géométriques temporaires
└── Scenes/
    └── GridTest.unity ✅  # Scène de test de la grille
```

## 🚀 Démarrage

### Prérequis
- **Unity 2022.3 LTS** (recommandé)
- Git

### Installation

1. Clonez le repository
```bash
git clone <repository-url>
cd ClogTowerDefense
```

2. Ouvrez le projet dans Unity

3. Ouvrez la scène de test
   - `Assets/Scenes/GridTest.unity`

4. Lancez la scène
   - **Clic gauche** : Placer un objet test
   - **Clic droit** : Retirer un objet
   - La grille hexagonale s'affiche avec highlight au survol

### Test de la Grille

La scène `GridTest.unity` contient :
- **HexGrid** : Composant principal de la grille (15x10)
- **HexGridRenderer** : Rendu visuel avec LineRenderer
- **HexGridTester** : Script de test interactif

**Fonctionnalités :**
- Hover sur une cellule → highlight vert/rouge
- Clic → placement d'un objet test
- Gizmos affichent les connexions entre voisins

## 📚 Documentation

- [Architecture Complète](ARCHITECTURE.md) - Design patterns, systèmes, flow
- [Documentation Grille Hexagonale](Assets/Scripts/Grid/README.md) - API et utilisation

## 🎨 Style Visuel

**Cible** : Pixel art isométrique

**Actuel** : Placeholders géométriques (carrés colorés, formes simples)

Les sprites finaux seront intégrés plus tard. Le gameplay est prioritaire.

## 🛠️ Technologies

- **Engine** : Unity 2022.3 LTS
- **Language** : C#
- **Architecture** : Component-based, ScriptableObjects
- **Patterns** : Singleton, State Machine, Observer, Object Pooling

## 📋 Roadmap

**✅ Phase 1 - Fondations** (COMPLÉTÉ)
- [x] Structure projet
- [x] Grille hexagonale isométrique
- [x] Système de coordonnées
- [x] Scène de test

**🔜 Phase 2 - Système de Rouages** (EN COURS)
- [ ] Engine component
- [ ] Cog component
- [ ] Placement system
- [ ] Rotation visuelle
- [ ] Système d'upgrade

**Phase 3 - Combat**
- [ ] Units de base
- [ ] Enemies de base
- [ ] Health/Damage system
- [ ] Projectiles

**Phase 4 - Gameplay**
- [ ] GameManager
- [ ] WaveManager
- [ ] ResourceManager
- [ ] UI principale

**Phase 5 - Polish**
- [ ] Méta-progression
- [ ] Save system
- [ ] Balance
- [ ] SFX/VFX

**Phase 6 - Content**
- [ ] Types d'unités
- [ ] Types d'ennemis
- [ ] Level design
- [ ] Pixel art

## 🤝 Contribution

Projet en développement actif. Architecture et systèmes de base en cours d'implémentation.

## 📄 Licence

À définir

---

**Status** : 🟢 En développement actif
**Version** : 0.1.0-alpha
**Dernière mise à jour** : 2025-12-21
