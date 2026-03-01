# 🧪 Guide de test - EcoBank Desktop Improvements

## Avant de commencer

Assurez-vous d'avoir compilé le projet :
```bash
cd E:\PROJECTS\GITHUB\EcoBank
dotnet build
```

---

## Test 1 : Vérifier la persistance des données ✅

### Objectif
Vérifier que les profils utilisateur sont sauvegardés et rechargés correctement.

### Étapes

1. **Démarrer l'application**
   - Lancez : `dotnet run --project src/Desktop/EcoBank.Desktop.csproj`

2. **Ajouter un profil**
   - Cliquez sur "Ajouter un profil"
   - Remplissez les champs :
     - **Client ID** : test-client-123
     - **Client Secret** : test-secret-abc
     - **App User ID** : test-user-001
     - **Code PIN** : 1234
   - Cochez "Enregistrer le profil"
   - Cliquez sur "Enregistrer & Continuer"

3. **Vérifier le stockage**
   - Les données doivent être sauvegardées dans :
     ```
     %APPDATA%\EcoBank\secure_storage\
     ```
   - Vous devriez voir un fichier `.dat` contenant les profils

4. **Redémarrer l'application**
   - Fermez l'application
   - Relancez-la

5. **Vérifier la restauration**
   - ✅ Le profil précédemment ajouté doit apparaître
   - ✅ L'écran "MON PROFIL FAVORI" doit afficher votre profil
   - ✅ Vous devriez pouvoir cliquer sur "Me connecter"

---

## Test 2 : Tester le composant OTP PIN ✅

### Objectif
Vérifier que le composant OTP fonctionne correctement avec la navigation et la validation.

### Étapes

1. **Sélectionner un profil existant**
   - Cliquez sur "Me connecter" sur un profil

2. **Vérifier l'interface OTP**
   - ✅ Vous devriez voir 4 champs carrés (au lieu d'une TextBox unique)
   - ✅ Chaque champ doit accepter un seul chiffre
   - ✅ Les champs doivent avoir un style vert/gris clair

3. **Tester la navigation automatique**
   - Tapez le premier chiffre (ex: 1)
   - ✅ Le curseur doit automatiquement aller au 2e champ
   - ✅ Répétez pour les 3 autres champs
   - ✅ Après le 4e chiffre, le focus doit rester ou revenir au début

4. **Tester la navigation par flèches**
   - Placez le curseur dans le 2e champ
   - Appuyez sur `Flèche gauche` → ✅ Devrait aller au 1er champ
   - Appuyez sur `Flèche droite` → ✅ Devrait aller au 3e champ

5. **Tester la suppression (Backspace)**
   - Remplissez les 4 champs : "1234"
   - Positionnez-vous sur le 4e champ
   - Appuyez sur `Backspace` → ✅ Le 4e champ doit s'effacer
   - Appuyez sur `Backspace` à nouveau → ✅ Devrait aller au 3e champ et l'effacer
   - Appuyez sur `Backspace` à nouveau → ✅ Devrait aller au 2e champ et l'effacer

6. **Tester la soumission**
   - Entrez le PIN correct : "1234"
   - ✅ Le bouton "Continuer" doit rester actif
   - Cliquez sur "Continuer"
   - ✅ Vous devriez être authentifié (ou voir un message d'erreur si le PIN est incorrect)

---

## Test 3 : Validation du PIN ❌❌❌ vs ✅

### Objectif
Vérifier la validation des PINs corrects et incorrects.

### Étapes

1. **Tester avec un PIN incorrect**
   - Sélectionnez un profil
   - Entrez un PIN différent du vôtre (ex: "0000")
   - Cliquez sur "Continuer"
   - ✅ Doit afficher : "Code PIN incorrect"
   - ✅ Doit rester sur l'écran de PIN

2. **Tester avec un PIN correct**
   - Effacez le champ (Backspace)
   - Entrez le PIN correct (ex: "1234")
   - Cliquez sur "Continuer"
   - ✅ Doit procéder à l'authentification
   - ✅ Doit naviguer vers l'écran d'accueil

---

## Test 4 : Annuler et revenir ⬅️

### Objectif
Vérifier la navigation et les annulations.

### Étapes

1. **Depuis l'écran PIN**
   - Cliquez sur le bouton profil en haut (avec le "✕")
   - ✅ Doit revenir à l'écran "MON PROFIL FAVORI"
   - ✅ Le PIN doit être effacé

2. **Sélectionner un autre profil**
   - ✅ Doit montrer l'écran PIN pour le nouveau profil

---

## Test 5 : Vérifier la persistance pour plusieurs profils 👥

### Objectif
Vérifier que plusieurs profils peuvent être sauvegardés et restaurés.

### Étapes

1. **Ajouter plusieurs profils**
   - Répétez le Test 1 avec 2-3 profils différents
   - Chaque profil devrait avoir un PIN différent

2. **Redémarrer et vérifier**
   - Fermez l'application
   - Relancez-la
   - ✅ Tous les profils doivent être présents

3. **Vérifier chaque profil**
   - Testez l'authentification pour chaque profil
   - ✅ Chaque PIN doit fonctionner correctement

---

## 🔍 Débogage

### Si les données ne persistent pas

1. **Vérifier le dossier AppData**
   ```
   %APPDATA%\EcoBank\secure_storage\
   ```
   - Doit contenir au moins le fichier `saved_profiles.dat`

2. **Vérifier les permissions**
   - Le dossier doit être accessible en lecture/écriture

3. **Vérifier les logs**
   - Recherchez les exceptions dans la console

### Si le composant OTP ne fonctionne pas

1. **Vérifier la compilation**
   ```bash
   dotnet build src/App/EcoBank.App.csproj
   ```

2. **Vérifier les namespaces**
   - `xmlns:auth="using:EcoBank.App.Views.Auth"` doit être dans LoginView.axaml

3. **Effacer le cache**
   ```bash
   dotnet clean
   dotnet restore
   dotnet build
   ```

---

## 📊 Résultats attendus

### ✅ Succès
- [x] Les profils persistent entre les redémarrages
- [x] Le composant OTP affiche 4 champs
- [x] La navigation automatique fonctionne
- [x] Les flèches naviguent entre les champs
- [x] Backspace efface et navigue correctement
- [x] Le PIN correct authentifie l'utilisateur
- [x] Le PIN incorrect affiche une erreur
- [x] L'UI est responsive et fluide

### ❌ Échecs possibles
- [ ] Les profils ne persistent pas → Vérifier FileSecureStorage
- [ ] L'OTP ne s'affiche pas → Vérifier les namespaces XAML
- [ ] La navigation OTP est cassée → Vérifier OtpPinInput.axaml.cs
- [ ] L'authentification échoue → Vérifier le ProfileService

---

## 📝 Notes

- Les données sont stockées en **Base64** (pour production, utiliser **AES-256**)
- Le PIN est hashé avec **SHA256** avant stockage
- Les fichiers `.dat` ne doivent PAS être partagés entre les utilisateurs
- Pour les environnements critiques, implémenter le chiffrement AES

---

**Bon test ! 🚀**

