# Jinx

[![CI](https://github.com/ElictFishy/Jinx/actions/workflows/ci.yml/badge.svg)](https://github.com/ElictFishy/Jinx/actions/workflows/ci.yml)

Adaptation numérique du jeu de société Jinx, développée en C# / .NET MAUI dans le cadre d'une SAÉ en BUT Informatique 1ère année (2025/2026).

Le jeu oppose 2 à 4 joueurs sur 3 manches, en cherchant à accumuler le maximum de points en récupérant des cartes numérotées sur un plateau de 4x4.

**Réalisé en binôme.**

---

## Prérequis

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Visual Studio 2022+](https://visualstudio.microsoft.com/) avec la charge de travail **.NET MAUI**

---

```powershell
## Structure du projet
src/
├── JinxApp/             → Application .NET MAUI 
├── Models/              → Classes du modele 
├── Managers/            → Logique metier
├── DataService/         → Persistance (sauvegarde, historique, classement)
├── ConsoleApp/          → Version console jouable
├── Stub/                → Données fictives pour les tests et le développement
├── Tests/
│   ├── Models.Tests/    → Tests unitaires du modele
│   └── Managers.Tests/  → Tests unitaires des managers
├── JinxSln.sln          → Solution complète (avec MAUI)
└── Jinx_SansMaui.sln   → Solution utilisée en CI (sans MAUI)
```
---

## Lancer le projet

### 1. Mettre à jour les workloads .NET MAUI

```powershell
dotnet workload update
```

### 2. Restaurer les dépendances

```powershell
cd src
dotnet restore JinxSln.sln
```

### 3. Compiler le projet

```powershell
dotnet build --no-restore JinxSln.sln
```

### 4. Lancer l'application MAUI

```powershell
dotnet run --project src/JinxApp/JinxApp.csproj
```

### 5. Lancer la version console

```powershell
cd src/ConsoleApp
dotnet run
```

### 6. Lancer les tests unitaires

```powershell
cd src
dotnet test Jinx_SansMaui.sln
```

---

## Technologies & outils

| Outil | Usage |
|---|---|
| C# / .NET 10 | Langage principal |
| .NET MAUI | Interface graphique multiplateforme |
| xUnit | Tests unitaires |
| Doxygen | Génération de documentation |
| PlantUML | Diagrammes UML |

---

## CI/CD

Le pipeline CI est configuré via GitHub Actions (`.github/workflows/ci.yml`) et se compose de deux étapes, sur la solution sans MAUI (`Jinx_SansMaui.slnx`) :

- **Build** — compilation des projets `Models`, `Managers`, `ConsoleApp` et `Tests`
- **Tests** — exécution des tests unitaires (`Models.Tests`, `Managers.Tests`)

Le build de l'application mobile (MAUI) et l'analyse qualité ne sont pas inclus ici : ils demandaient l'infrastructure CI de l'établissement (image MAUI dédiée, SonarQube).

---

## État du projet

### ✅ Fonctionnel
- Version console jouable de bout en bout
- Application MAUI jouable (2 à 4 joueurs, 3 manches)
- Plateau 4x4 avec sélection de cartes
- Lancer et relancer du dé
- Gestion des tours et des manches
- Calcul des scores en direct et en fin de manche
- Vérification et suppression des cartes de mauvaise couleur (annonce des cartes perdues en fin de manche)
- **Cartes chance** : pioche par échange et logique complète (augmenter/réduire/relancer le dé, multi-couleurs, prise directe carte basse/haute)
- **Évènements aléatoires** : Orage et Tornade intégrés au déroulement de la partie (déclenchement aléatoire)
- **IA** : joueurs IA autonomes (lancer, cartes chance, choix de carte, échanges)
- **Mode démonstration** : depuis la page des règles, « Voir un exemple de partie » lance une partie jouée par deux IA (sans sauvegarde, ni classement, ni historique)
- **Persistance** : sauvegarde/reprise de la partie en cours, historique des parties, classement des meilleurs scores (via `XmlDataService`)
- Navigation entre les pages (accueil, configuration, jeu, règles, historique, classement)
- DataBinding MAUI (plateau, deck joueur, score, tour, joueur courant)
- Pipeline CI (build, tests)
- Tests unitaires des modèles et managers

### 🔄 Partiellement implémenté
- **Tests unitaires** : couverture non complète

---

## Bugs connus

- Aucun
