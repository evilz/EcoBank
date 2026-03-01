# 🚀 Guide de démarrage rapide - Après implémentation

## Démarrage en 5 minutes

### Étape 1 : Compiler le projet
```bash
cd "E:\PROJECTS\GITHUB\EcoBank"
dotnet build
```

**Résultat attendu :**
```
✅ EcoBank.Shared
✅ EcoBank.Core
✅ EcoBank.Infrastructure.Xpollens
✅ EcoBank.App
✅ EcoBank.Desktop (avec avertissements Android, OK)
```

### Étape 2 : Lancer l'application Desktop
```bash
cd "E:\PROJECTS\GITHUB\EcoBank"
dotnet run --project src/Desktop/EcoBank.Desktop.csproj
```

**L'application devrait démarrer** ✅

### Étape 3 : Tester la fonctionnalité

#### Test 1 : Interface OTP
1. Cliquez sur "Ajouter un profil"
2. Remplissez les champs
3. À l'écran PIN, vous devriez voir **4 champs carrés** (pas une TextBox)
4. ✅ Tapez les chiffres - la navigation devrait être automatique

#### Test 2 : Persistence
1. Ajoutez un profil avec PIN "1234"
2. Fermez l'application
3. Relancez-la
4. ✅ Le profil devrait toujours être là

---

## 🔍 Vérification rapide

### Fichiers créés
```bash
# Vérifier que les nouveaux fichiers existent
ls "E:\PROJECTS\GITHUB\EcoBank\src\App\Services\FileSecureStorage.cs"
ls "E:\PROJECTS\GITHUB\EcoBank\src\App\Views\Auth\OtpPinInput.axaml"
ls "E:\PROJECTS\GITHUB\EcoBank\src\App\Views\Auth\OtpPinInput.axaml.cs"
```

### Fichiers modifiés
```bash
# Vérifier que LoginView contient le composant OTP
grep "OtpPinInput" "E:\PROJECTS\GITHUB\EcoBank\src\App\Views\Auth\LoginView.axaml"

# Vérifier que DependencyInjection utilise FileSecureStorage
grep "FileSecureStorage" "E:\PROJECTS\GITHUB\EcoBank\src\App\DependencyInjection.cs"
```

### Stockage persistant
```bash
# Vérifier le dossier de stockage
dir "%APPDATA%\EcoBank\secure_storage\"

# Doit contenir un fichier .dat après avoir ajouté un profil
```

---

## 💡 Tips & Tricks

### Effacer l'état
Si vous voulez recommencer à zéro :
```bash
# Supprimer le stockage persistant
rmdir "%APPDATA%\EcoBank" -Recurse -Force

# Relancer l'application
dotnet run --project src/Desktop/EcoBank.Desktop.csproj
```

### Voir le contenu du stockage
Le fichier `saved_profiles.dat` contient du JSON en Base64 :
```bash
# Décoder et voir le contenu
$content = [IO.File]::ReadAllText("$env:APPDATA\EcoBank\secure_storage\saved_profiles.dat")
[System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($content))
```

### Déboguer la liaison OTP
```csharp
// Dans OtpPinInput.cs, ajouter un log
public string GetOtpValue()
{
    var result = string.Concat(_inputs.Select(i => i.Text ?? ""));
    Debug.WriteLine($"OTP Value: {result}");
    return result;
}
```

---

## 📋 Checklist avant de démarrer

- [ ] Vous avez clôné le repository
- [ ] Vous avez .NET 10.0+ installé (`dotnet --version`)
- [ ] Vous avez ouvert le terminal dans le dossier root
- [ ] Vous avez assez d'espace disque (~500MB pour build)
- [ ] Vous avez accès à `%APPDATA%` (Windows)

---

## ⚠️ Problèmes courants

### "dotnet not found"
```bash
# Installer .NET à partir de https://dotnet.microsoft.com/download
# Ou ajouter à PATH
```

### "Fichier verrouillé lors de la compilation"
```bash
# Tuer tous les processus .NET
Get-Process | Where-Object {$_.ProcessName -like "*dotnet*"} | Stop-Process -Force

# Relancer la compilation
dotnet clean
dotnet build
```

### "L'OTP ne s'affiche pas"
1. Vérifier que le namespace `auth` est défini dans LoginView.axaml
2. Vérifier que OtpPinInput.xaml est au bon endroit
3. Compiler uniquement le projet App :
   ```bash
   dotnet build src/App/EcoBank.App.csproj
   ```

### "Les données ne persistent pas"
1. Vérifier que `FileSecureStorage` est injecté dans DependencyInjection.cs
2. Vérifier que le dossier existe : `%APPDATA%\EcoBank\`
3. Vérifier les permissions d'accès au dossier AppData

---

## 📞 Support

### Documentation
- **Implémentation détaillée** : `IMPLEMENTATION_PIN_AND_PERSISTENCE.md`
- **Guide de test complet** : `TESTING_GUIDE.md`
- **Résumé des changements** : `CHANGES_SUMMARY.md`
- **Référence rapide** : `QUICK_REFERENCE.md`

### Fichiers importants
- **Stockage** : `src/App/Services/FileSecureStorage.cs`
- **OTP UI** : `src/App/Views/Auth/OtpPinInput.axaml`
- **OTP Logic** : `src/App/Views/Auth/OtpPinInput.axaml.cs`
- **Injection** : `src/App/DependencyInjection.cs`

---

**Bon test ! 🎉**

Pour des questions ou des problèmes, consultez les fichiers de documentation ou examinez le code source directement.

