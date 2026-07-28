# TamagoShit — Guide utilisateur du build final

Bienvenue dans **TamagoShit**, une petite expérience 3D interactive avec une île low-poly, un personnage-guide appelé **Momo**, une IA locale, des déplacements dans la scène, des interactions avec des objets et une animation spéciale du dragon.

Ce guide explique comment préparer votre ordinateur, lancer le jeu, utiliser les fonctionnalités IA et résoudre les problèmes les plus courants.

---

## 1. Présentation rapide

Dans TamagoShit, vous explorez une île stylisée et discutez avec Momo, une petite taupe-guide intégrée à la scène.

Momo peut :

- se présenter au lancement de la scène ;
- répondre à vos messages ;
- garder en mémoire les derniers échanges ;
- regarder des objets de la scène ;
- se déplacer vers certains points d'intérêt ;
- déclencher l'animation d'attaque du dragon si vous lui demandez ;
- utiliser une personnalité par défaut ou une personnalité personnalisée.

Le jeu utilise une IA locale avec **Ollama**. Cela signifie que l'IA tourne sur votre ordinateur, sans utiliser un service en ligne comme ChatGPT.

---

## 2. Configuration recommandée

### Système

Le build est prévu pour :

```txt
Windows 10 ou Windows 11
```

### Configuration conseillée

```txt
Processeur : CPU moderne 4 cœurs ou plus
RAM : 16 Go recommandés
GPU : carte graphique compatible Unity
Stockage : quelques Go libres
```

### IA locale

Pour utiliser les fonctionnalités IA, il faut pouvoir faire tourner un modèle local avec Ollama.

Le modèle recommandé est :

```txt
qwen3:4b-instruct
```

Ce modèle a été choisi car il est relativement léger pour une exécution locale tout en étant suffisamment fiable pour suivre des instructions, tenir un rôle et produire des réponses structurées.

---

## 3. Dépendances à installer

Avant de lancer le jeu avec l'IA, vous devez installer :

1. **Ollama**
2. Le modèle **qwen3:4b-instruct**

Sans Ollama, le jeu peut se lancer, mais Momo ne pourra pas répondre correctement via l'IA.

---

## 4. Installer Ollama sur Windows

### Étape 1 — Télécharger Ollama

Téléchargez Ollama depuis le site officiel :

```txt
https://ollama.com
```

Installez-le comme une application Windows classique.

### Étape 2 — Vérifier l'installation

Ouvrez PowerShell, puis tapez :

```powershell
ollama --version
```

Si une version s'affiche, Ollama est installé.

### Étape 3 — Installer le modèle IA

Dans PowerShell :

```powershell
ollama pull qwen3:4b-instruct
```

Le téléchargement peut prendre un moment.

### Étape 4 — Vérifier que le modèle est présent

```powershell
ollama list
```

Vous devriez voir une ligne contenant :

```txt
qwen3:4b-instruct
```

---

## 5. Vérifier qu'Ollama fonctionne

Avant de lancer le jeu, il est conseillé de tester l'API locale.

Dans PowerShell :

```powershell
$body = @{
    model = "qwen3:4b-instruct"
    prompt = "Réponds uniquement: OK"
    stream = $false
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:11434/api/generate" -Method Post -ContentType "application/json" -Body $body
```

Résultat attendu : une réponse contenant `OK`.

Si cela fonctionne, le jeu pourra communiquer avec l'IA locale.

---

## 6. Lancer le jeu

### Étape 1 — Décompresser le build

Décompressez l'archive du build final dans un dossier simple, par exemple :

```txt
C:\Games\TamagoShit\
```

Évitez les chemins trop longs ou les dossiers système protégés.

### Étape 2 — Lancer Ollama

Dans la plupart des cas, Ollama tourne automatiquement en arrière-plan après installation.

Si besoin, ouvrez PowerShell et lancez :

```powershell
ollama serve
```

Si un message indique que le port est déjà utilisé, ce n'est pas forcément une erreur : cela peut simplement vouloir dire qu'Ollama est déjà lancé.

### Étape 3 — Lancer le jeu

Double-cliquez sur l'exécutable du jeu :

```txt
TamagoShit.exe
```

---

## 7. Menu principal

Au lancement, vous arrivez sur le menu principal.

Les boutons disponibles sont :

### Jouer avec Momo par défaut

Lance directement la scène avec la personnalité standard de Momo.

C'est l'option recommandée pour une première démonstration.

### Jouer avec IA custom

Ouvre le menu de création de personnalité.

Vous pouvez définir :

- le nom du personnage ;
- son rôle ;
- son ton ;
- ses règles de comportement ;
- sa backstory.

Ensuite, cliquez sur :

```txt
Utiliser cette personnalité et jouer
```

La scène se lance avec votre personnage custom.

### Quitter

Ferme le jeu.

---

## 8. Utiliser la chatbox

Dans la scène principale, une chatbox apparaît en bas de l'écran.

Pour parler à Momo :

1. Cliquez dans le champ texte.
2. Écrivez votre message.
3. Appuyez sur **Entrée**.

Exemples :

```txt
Salut, qui es-tu ?
```

```txt
Regarde le puits.
```

```txt
Va au dock.
```

```txt
Va voir le palmier.
```

```txt
Active le dragon.
```

---

## 9. Fonctionnalités IA

### Dialogue naturel

Vous pouvez discuter avec Momo en langage naturel.

Momo peut répondre à des questions simples sur lui-même, la scène, les objets visibles et les interactions disponibles.

### Mémoire courte

Momo garde un historique récent de la conversation.

Exemple :

```txt
Vous : Où est le puits ?
Momo : Le puits est au centre de la place.
Vous : Regarde-le.
Momo : Je regarde le puits.
```

L'IA peut comprendre que `le` fait référence au puits mentionné juste avant.

### Personnalité

La personnalité influence :

- la façon de parler ;
- le ton ;
- la backstory ;
- le style des réponses ;
- la manière de réagir aux objets.

---

## 10. IA custom

Le mode IA custom permet de remplacer Momo par un personnage personnalisé.

### Champs disponibles

#### Nom du personnage

Exemple :

```txt
Biscotte
```

#### Rôle

Exemple :

```txt
Guide rêveur de l'île, spécialisé dans les vieux mécanismes.
```

#### Ton

Exemple :

```txt
Calme, poétique, un peu mystérieux.
```

#### Règles de comportement

Exemple :

```txt
Répondre en phrases courtes. Ne jamais sortir du personnage. Ne pas inventer d'objets absents de la scène.
```

#### Backstory

Exemple :

```txt
Biscotte est une créature discrète qui connaît les secrets de l'île et adore observer le dragon depuis la place centrale.
```

---

## 11. Interactions disponibles

Momo peut interagir avec plusieurs éléments de la scène.

### Regarder un objet

Exemples :

```txt
Regarde le puits.
```

```txt
Tourne-toi vers le puits.
```

```txt
Observe le puits.
```

Momo se tourne vers l'objet demandé.

### Se déplacer vers un objet

Exemples :

```txt
Va au dock.
```

```txt
Va près du port.
```

```txt
Va voir le dragon.
```

Momo utilise le NavMesh pour se déplacer jusqu'au point d'intérêt correspondant.

### Décrire un objet

Exemples :

```txt
Décris le marché.
```

```txt
C'est quoi les cagettes ?
```

```txt
Parle-moi du dragon.
```

Momo répond avec une courte description.

### Activer le dragon

Exemples :

```txt
Active le dragon.
```

```txt
Fais attaquer le dragon.
```

```txt
Réveille le dragon.
```

Si l'action est reconnue, l'animation d'attaque du dragon se déclenche.

---

## 12. Objets interactifs


```txt
dock / ponton
well / puits
fruit_stall / présentoir
dragon / dragon
```

L'IA est censée utiliser uniquement les objets déclarés dans la scène. Si un objet n'est pas reconnu, essayez d'utiliser un mot plus simple ou plus direct.

---

## 13. Conseils pour une bonne expérience

### Faites des phrases simples

Préférez :

```txt
Va au dock.
```

plutôt que :

```txt
Serait-il possible, dans l'hypothèse où cela ne te dérange pas, de peut-être envisager une promenade vers la structure en bois près de l'eau ?
```

### Mentionnez clairement l'objet

Préférez :

```txt
Regarde le puits.
```

plutôt que :

```txt
Regarde ça.
```

Sauf si l'objet a été mentionné juste avant.

### Attendez la réponse

Le modèle tourne localement, donc la réponse peut prendre quelques secondes selon votre machine.

---

## 14. Problèmes fréquents

### Momo ne répond pas

Causes possibles :

- Ollama n'est pas lancé.
- Le modèle `qwen3:4b-instruct` n'est pas installé.
- Le port local `11434` n'est pas accessible.
- Le modèle est trop lent à répondre.

Solutions :

1. Ouvrez PowerShell.
2. Tapez :

```powershell
ollama list
```

3. Vérifiez que `qwen3:4b-instruct` est présent.
4. Testez l'API avec la commande de vérification du chapitre 5.

### Le jeu dit que l'IA locale ne répond pas

Vérifiez qu'Ollama tourne.

Essayez :

```powershell
ollama serve
```

Puis relancez le jeu.

### Le modèle n'existe pas

Installez-le :

```powershell
ollama pull qwen3:4b-instruct
```

### Momo répond très lentement

C'est normal sur certaines machines.

Solutions possibles :

- fermer les applications lourdes ;
- vérifier que votre PC n'est pas en mode économie d'énergie ;
- essayer un modèle plus léger si le projet a été configuré pour l'accepter ;
- attendre quelques secondes entre les commandes.

### Momo va au mauvais endroit

Cela dépend des points d'intérêt placés dans la scène.

Dans le build final, cela ne peut pas être corrigé par l'utilisateur.  
Pour les développeurs, il faut ajuster les marqueurs Unity dans la scène.

### Momo ne bouge pas

Causes possibles :

- destination inaccessible ;
- NavMesh incomplet ;
- Momo n'est pas sur le NavMesh ;
- l'IA a répondu avec une action de regard plutôt qu'une action de déplacement.

Essayez une commande plus directe :

```txt
Va au dock.
```

### Le dragon ne s'active pas

Essayez une commande claire :

```txt
Active le dragon.
```

ou :

```txt
Fais attaquer le dragon.
```

Si cela ne fonctionne pas, il peut s'agir d'un problème de configuration du build ou de l'Animator du dragon.

### La personnalité custom n'est pas utilisée

Vérifiez que vous avez bien utilisé :

```txt
Jouer avec IA custom
```

puis :

```txt
Utiliser cette personnalité et jouer
```

Si vous choisissez `Jouer avec Momo par défaut`, le jeu réutilise volontairement la personnalité standard de Momo.

---

## 15. Données locales

Le jeu peut créer un fichier local pour stocker la personnalité custom.

Ce fichier ne contient pas d'information sensible par défaut, seulement les champs de personnalité que vous avez écrits.

Emplacement typique sur Windows :

```txt
C:\Users\<Utilisateur>\AppData\LocalLow\<CompanyName>\<ProductName>\ai_personality.json
```

## 16. Confidentialité

L'IA fonctionne localement avec Ollama.

Cela signifie que les messages envoyés à Momo sont traités par un modèle qui tourne sur votre machine.  
Le projet n'utilise pas volontairement d'API cloud pour générer les réponses IA.

Attention : si vous modifiez le projet ou Ollama pour utiliser un autre backend, ce comportement peut changer.

---

## 17. Résumé de lancement rapide

Pour lancer le jeu avec l'IA :

1. Installer Ollama.
2. Installer le modèle :

```powershell
ollama pull qwen3:4b-instruct
```

3. Vérifier :

```powershell
ollama list
```

4. Lancer le build :

```txt
TamagoShit.exe
```

5. Choisir un mode :

```txt
Jouer avec Momo par défaut
```

ou

```txt
Jouer avec IA custom
```

6. Dans la scène, écrire dans la chatbox et appuyer sur Entrée.