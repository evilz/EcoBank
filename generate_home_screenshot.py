#!/usr/bin/env python3
"""Generate Home screen screenshot with official EcoBank palette"""

from PIL import Image, ImageDraw, ImageFont

# Dimensions
width, height = 440, 920
img = Image.new('RGB', (width, height), color='#F7F8F5')
draw = ImageDraw.Draw(img)

# Official colors
colors = {
    'bg': '#F7F8F5',
    'card': '#FFFFFF',
    'primary': '#7ED957',
    'primary_light': '#F0F9E8',
    'accent': '#C6FF00',
    'text_dark': '#1B1D1F',
    'text_light': '#7A7F85',
    'danger': '#FF4D4F',
    'green': '#1E7F4F',
}

# Fonts
try:
    font_title = ImageFont.truetype('arial.ttf', 28)
    font_label = ImageFont.truetype('arial.ttf', 14)
    font_small = ImageFont.truetype('arial.ttf', 12)
except:
    font_title = ImageFont.load_default()
    font_label = font_title
    font_small = font_title

# Helper functions
def draw_card(y, height_val, content_func):
    """Draw a white card"""
    draw.rounded_rectangle(
        [(16, y), (width-16, y+height_val)],
        radius=24,
        fill=colors['card'],
        outline='#E0E0E0',
        width=1
    )
    content_func(y)

def draw_section_title(y, text):
    """Draw section title"""
    draw.text((16, y), text, fill=colors['text_dark'], font=font_label)
    return y + 30

# Status bar
draw.rectangle([(0, 0), (width, 24)], fill='#1B1D1F')
draw.text((10, 4), '9:41', fill='white', font=font_small)
draw.text((width-60, 4), '📶 📡 🔋', fill='white', font=font_small)

y = 24

# Hero card - Green
draw.rounded_rectangle(
    [(16, y), (width-16, y+140)],
    radius=24,
    fill=colors['primary'],
    outline=None
)
draw.text((30, y+15), 'Bonjour, Alex', fill='white', font=font_label)
draw.text((30, y+40), 'Total Balance', fill='white', font=font_small)
draw.text((30, y+60), '$45,000.00', fill='white', font=font_title)
draw.text((30, y+100), '+12.5% ↑', fill='white', font=font_small)
y += 160

# Evolution chart
draw.rounded_rectangle(
    [(16, y), (width-16, y+100)],
    radius=24,
    fill=colors['card'],
    outline='#E0E0E0',
    width=1
)
draw.text((30, y+10), 'Évolution du solde', fill=colors['text_dark'], font=font_label)
# Badge
draw.rounded_rectangle(
    [(width-100, y+10), (width-20, y+30)],
    radius=12,
    fill=colors['primary_light']
)
draw.text((width-90, y+13), '+12.5%', fill=colors['primary'], font=font_small)
# Chart placeholder
draw.text((width//2-30, y+40), '📈', fill=colors['primary'], font=font_label)
y += 110

# Quick actions
y = draw_section_title(y, 'Actions rapides')
action_y = y

actions = [
    ('📤', 'Envoyer'),
    ('✓', 'Reçu'),
    ('🔄', 'Virement'),
    ('•••', 'Plus')
]

action_width = (width - 40) // 4
for i, (emoji, label) in enumerate(actions):
    x = 20 + i * (action_width + 6)
    
    # Icon background
    draw.rounded_rectangle(
        [(x, action_y), (x+action_width-6, action_y+50)],
        radius=28,
        fill=colors['primary_light']
    )
    draw.text((x+action_width//2-10, action_y+10), emoji, fill=colors['primary'], font=font_label)
    
    # Label
    draw.text((x+5, action_y+60), label, fill=colors['text_dark'], font=font_small)

y = action_y + 90

# Accounts section
y = draw_section_title(y, 'Mes comptes')

accounts = [
    ('Compte Courant', '$12,500.50'),
    ('Épargne', '$32,499.50')
]

for label, balance in accounts:
    # Card
    draw.rounded_rectangle(
        [(16, y), (width-16, y+70)],
        radius=24,
        fill=colors['card'],
        outline='#E0E0E0',
        width=1
    )
    draw.text((30, y+10), label, fill=colors['text_dark'], font=font_label)
    draw.text((30, y+35), 'FR76 3000...', fill=colors['text_light'], font=font_small)
    draw.text((width-120, y+15), balance, fill=colors['primary'], font=font_label)
    
    y += 80

# Recent transactions
y = draw_section_title(y, 'Transactions')

transactions = [
    ('🏬 Amazon', '-$80.00', 'Aujourd\'hui', 'danger'),
    ('👤 Sara N.', '+$250.00', 'Hier', 'primary'),
    ('📺 Netflix', '-$15.99', 'Hier', 'danger'),
]

for emoji_name, amount, date, color_key in transactions:
    # Card
    draw.rounded_rectangle(
        [(16, y), (width-16, y+70)],
        radius=24,
        fill=colors['card'],
        outline='#E0E0E0',
        width=1
    )
    
    # Icon
    draw.rounded_rectangle(
        [(25, y+10), (55, y+40)],
        radius=28,
        fill=colors['primary_light']
    )
    draw.text((32, y+12), emoji_name[0], fill=colors['primary'], font=font_label)
    
    # Details
    draw.text((65, y+10), emoji_name[1:], fill=colors['text_dark'], font=font_label)
    draw.text((65, y+35), date, fill=colors['text_light'], font=font_small)
    
    # Amount
    amount_color = colors['danger'] if color_key == 'danger' else colors['primary']
    draw.text((width-130, y+15), amount, fill=amount_color, font=font_label)
    
    y += 80

# Bottom navigation (simulated)
y = height - 50
draw.rectangle([(0, y), (width, height)], fill='white')
draw.line([(0, y), (width, y)], fill='#E0E0E0', width=1)

nav_items = [('🏠', 'Accueil'), ('💳', 'Cartes'), ('⚙️', 'Opérations'), ('👤', 'Profil')]
nav_width = width // 4

for i, (icon, label) in enumerate(nav_items):
    x = i * nav_width + nav_width // 2
    draw.text((x-15, y+10), icon, fill=colors['primary'], font=font_label)

# Save
img.save('docs/screenshot-home.png', 'PNG')
print('✅ Home screen screenshot saved: docs/screenshot-home.png')

