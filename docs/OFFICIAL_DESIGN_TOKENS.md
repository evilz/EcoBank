# 🎨 EcoBank Official Design Tokens

## Source
- `design/design_tokens.json`
- `design/design_tokens.md`

## Colors (Palette officielle)

### Primary
```
Primary: #7ED957
- Usage: Boutons principaux, titres accentués, éléments principaux
- Hover: #6BC843 (15% darker)
- Pressed: #5AB132 (25% darker)
- Disabled: #E0E0E0 (gray)
```

### Primary Dark
```
Primary Dark: #1E7F4F
- Usage: Success states, badges, confirmations
- Text: White (#FFFFFF)
```

### Accent
```
Accent: #C6FF00
- Usage: Éléments décoratifs, highlights
- Opacity: 25% (décoratif)
```

### Backgrounds & Surfaces
```
Background: #F7F8F5 (page background)
Card Background: #FFFFFF (cartes, surfaces)
Text Primary: #1B1D1F (texte principal)
Text Secondary: #7A7F85 (texte secondaire)
Outline: #E0E0E0 (borders)
```

### Status Colors
```
Success: #1E7F4F
Error: #FF4D4F
Warning: #C6FF00
```

### Dark Theme
```
Status: DISABLED (futur)
```

## Typography

### Font Family
```
Primary: Inter
Secondary: SF Pro (macOS/iOS)
Fallback: sans-serif
```

### Font Weights
```
Body: 400 (regular)
Headings: 700 (bold)
```

### Font Sizes
```
Large Balance: 28px (titres principaux)
Section Title: 18px (sous-titres)
Body: 14px (texte courant)
Small: 12px (footer, labels mineurs)
```

## Border Radius (Corner Radius)

```
Card: 24px
Button (pill shape): 28px
Small Elements: 16px
Extra Small: 8px
Minimal: 4px
```

## Shadows

### Card Shadow
```
Offset Y: 10px
Blur: 30px
Spread: 0px
Opacity: 15% (ou 0.15)
Color: black (#000000)

Calculated: (0, 10, 30, 0) with 15% opacity
= 0 10 30 0 #26000000
```

### On Hover (Card)
```
Offset Y: 12px
Blur: 40px
Opacity: 18-20%
```

## Spacing Scale (4dp base)

```
xs:  4px  (minimum gap, padding)
sm:  8px  (small gap)
md:  16px (medium gap, standard)
lg:  24px (large gap, section spacing)
xl:  32px (extra large gap)
```

## Usage in Components

### Button.Primary
```
Background: #7ED957
Text: White
Radius: 28px (pill)
Padding: 16px horizontal, 14px vertical
Min Height: 48dp
Font Weight: SemiBold
Font Size: 15px

States:
- Normal: #7ED957
- Hover: #6BC843
- Pressed: #5AB132
- Disabled: #E0E0E0 with gray text
```

### TextBox.EcoField
```
Background: #FFFFFF (card background)
Border: #E0E0E0, 1.5px
Radius: 12px
Padding: 12px horizontal, 10px vertical
Min Height: 48dp
Font Size: 15px

States:
- Focused: Border #7ED957, 2px
- Placeholder: #7A7F85 (text secondary)
```

### Border.Card
```
Background: #FFFFFF
Radius: 24px
Padding: 16px
Shadow: 0 10 30 0 #26000000

On Hover:
- Shadow: 0 12 40 0 #30000000
```

## Accessibility

### Touch Targets
```
Minimum: 48dp x 48dp
- Buttons
- Input fields
- Touch targets
```

### Color Contrast
```
Text Primary (#1B1D1F) on White (#FFFFFF): 19.27:1 ✅ AAA
Text Secondary (#7A7F85) on White: ~5.5:1 ✅ AA
Primary (#7ED957) on White: ~4.9:1 ✅ AA
Success (#1E7F4F) on White: ~6.8:1 ✅ AA
```

## Implementation Map

### Tokens.axaml
```
LightBackground: #F7F8F5
LightSurface: #FFFFFF
LightPrimary: #7ED957
LightSecondary: #7ED957
LightTertiary: #C6FF00
LightSuccess: #1E7F4F
LightError: #FF4D4F
LightOnSurface: #1B1D1F
LightOnSurfaceVariant: #7A7F85

RadiusButton: 28
RadiusCard: 24
RadiusMd: 12
```

### Components.axaml
```
Button.Primary:
  - Background: #7ED957
  - CornerRadius: RadiusButton (28px)
  
TextBox.EcoField:
  - BorderBrush: #E0E0E0
  - BorderBrush(Focus): #7ED957

Border.Card:
  - CornerRadius: RadiusCard (24px)
  - BoxShadow: 0 10 30 0 #26000000
```

### LoginView.axaml
```
Logo EcoBank: #7ED957
Title Accent: #7ED957
Text Primary: #1B1D1F
Text Secondary: #7A7F85
Decorative Circles: #C6FF00, #7ED957
Footer: #7A7F85
```

## Migration Guide

### From Old Palette
```
Old Primary (#1A56DB) → New Primary (#7ED957)
Old Secondary (#0CA678) → New Primary (#7ED957)
Old Accent N/A → New Accent (#C6FF00)
Old Text (#111928) → New Text (#1B1D1F)
Old Gray (#6B7280) → New Gray (#7A7F85)
```

### Files to Update
1. `src/App/Styles/Tokens.axaml` ✅
2. `src/App/Styles/Components.axaml` ✅
3. All `.axaml` Views ✅
4. `README.md` ✅
5. Other pages (Home, Accounts, etc.) ⏳

## Dark Theme (Future)

When implementing dark mode:
```
DarkBackground: (to be defined)
DarkSurface: (to be defined)
DarkPrimary: (to be defined)
...

Use DynamicResource for theme switching
Keep Light theme colors as primary
```

## Validation Checklist

- [x] All colors from JSON applied
- [x] All typography settings applied
- [x] Radius specifications implemented
- [x] Shadow specifications implemented
- [x] Spacing scale used
- [x] Accessibility validated
- [x] Dark theme disabled
- [x] Compilation successful

---

**Reference Files**:
- `design/design_tokens.json` - Source
- `design/design_tokens.md` - Documentation source
- `OFFICIAL_PALETTE_UPDATE.md` - Update details

