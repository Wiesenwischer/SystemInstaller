# TailAdmin Keycloak Theme for ReadyStackGo

<div align="center">
  <img src="../../assets/logo.png" alt="ReadyStackGo Logo" width="200">
</div>

> **Turn your specs into stacks**

Dieses Custom Theme für Keycloak wurde entwickelt, um das Design der TailAdmin Sign-In Seite zu replizieren und eine konsistente Benutzererfahrung zwischen dem ReadyStackGo (RSGO) Frontend und der Authentifizierung zu gewährleisten.

## Features

### 🎨 TailAdmin Design Language
- **Identisches Layout**: Repliziert das exakte Layout der TailAdmin Sign-In Seite
- **Moderne UI**: Sauberes, minimalistisches Design mit Tailwind CSS Styling
- **Responsive**: Vollständig responsive für alle Bildschirmgrößen
- **Dark Mode Support**: Unterstützt Hell- und Dunkelmodus

### 🔐 Authentifizierung Features
- **Email/Password Login**: Hauptauthentifizierung mit Email und Passwort
- **Social Login Buttons**: Vorkonfigurierte Buttons für Google, GitHub und andere Provider
- **Password Toggle**: Passwort anzeigen/verstecken Funktionalität
- **Remember Me**: "Angemeldet bleiben" Option
- **Forgot Password**: Passwort-Reset-Funktionalität

### 📱 Benutzerfreundlichkeit
- **Split Layout**: Links Formular, rechts Branding wie in TailAdmin
- **Moderne Icons**: SVG Icons für bessere Skalierbarkeit
- **Smooth Transitions**: Sanfte Hover-Effekte und Übergänge
- **Accessibility**: ARIA Labels und screen reader support

## Installation

Das Theme ist bereits konfiguriert und wird automatisch mit docker-compose geladen:

```bash
# Container stoppen
docker-compose down

# Container mit dem neuen Theme starten
docker-compose up -d
```

## Theme-Struktur

```
keycloak/themes/tailadmin/login/
├── theme.properties          # Theme-Konfiguration
├── template.ftl             # Basis-Template
├── login.ftl               # Haupt-Login-Seite
├── login-reset-password.ftl # Passwort vergessen
├── login-update-password.ftl # Passwort aktualisieren
└── resources/
    └── css/
        └── tailadmin.css    # Haupt-Stylesheet
```

## Anpassungen

### CSS-Variablen
Das Theme verwendet CSS Custom Properties für einfache Anpassungen:

```css
:root {
  --brand-500: #3b82f6;     /* Primärfarbe */
  --brand-600: #2563eb;     /* Hover-Zustand */
  --gray-50: #f9fafb;       /* Hintergrund */
  --error-500: #ef4444;     /* Fehlerfarbe */
}
```

### Branding anpassen
Das Branding auf der rechten Seite kann in der `login.ftl` angepasst werden:

```html
<div class="login-branding-content">
    <h2>Ihr Firmenname</h2>
    <p>Ihre Unternehmensbeschreibung</p>
</div>
```

### Social Provider
Neue Social Login Provider können in der Keycloak Admin Console konfiguriert werden. Das Theme erkennt automatisch:
- Google (mit Google-spezifischen Icons)
- GitHub (mit GitHub-Icons)
- Andere Provider (mit Standard-Icons)

## Technische Details

### Template Engine
- **FreeMarker Templates**: Keycloak's Template-Engine
- **Responsive Grid**: CSS Grid für das Layout
- **Flexbox**: Für die Komponentenausrichtung

### Browser-Unterstützung
- Chrome/Chromium 90+
- Firefox 88+
- Safari 14+
- Edge 90+

### Performance
- **Optimierte CSS**: Minimale CSS-Größe durch gezielte Selektoren
- **SVG Icons**: Vektorbasierte Icons für bessere Performance
- **Web Fonts**: Google Fonts (Outfit) für konsistente Typographie

## Troubleshooting

### Theme wird nicht angezeigt
1. Prüfen Sie ob das Theme-Verzeichnis korrekt gemountet ist
2. Stellen Sie sicher, dass `loginTheme: "tailadmin"` in der Realm-Konfiguration gesetzt ist
3. Container neu starten: `docker-compose restart keycloak`

### Styling-Probleme
1. Browser-Cache leeren
2. Prüfen Sie die Browser-Konsole auf CSS-Fehler
3. Validieren Sie die FreeMarker-Syntax in den .ftl-Dateien

### Custom Social Provider
Für neue Social Provider ohne vorgefertigte Icons:
1. Icon-SVG zur CSS-Datei hinzufügen
2. Entsprechende Bedingung in `login.ftl` ergänzen

## Sicherheit

### Best Practices
- **HTTPS**: Verwenden Sie immer HTTPS in der Produktion
- **CSP Headers**: Content Security Policy für zusätzliche Sicherheit
- **Secure Cookies**: SameSite und Secure Flags für Cookies

### Updates
- Regelmäßige Updates von Keycloak
- Überwachung der Abhängigkeiten
- Sicherheitspatches zeitnah anwenden

## Support

Bei Problemen oder Fragen:
1. Überprüfen Sie die Keycloak-Logs: `docker-compose logs keycloak`
2. Validieren Sie die Theme-Struktur
3. Testen Sie mit verschiedenen Browsern

## Lizenz

Dieses Theme basiert auf dem TailAdmin Template und folgt den gleichen Lizenzbestimmungen.
