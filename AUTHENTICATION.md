# SystemInstaller Authentifizierung mit Keycloak

## Übersicht

Das SystemInstaller-Projekt verwendet Keycloak für die Authentifizierung und Autorisierung. Keycloak ist ein Open-Source Identity und Access Management System, das OpenID Connect unterstützt.

## Setup

### 1. Container starten

```bash
docker-compose up -d
```

Dies startet:
- SQL Server (Port 1433)
- Keycloak (Port 8080)
- SystemInstaller Web App (Port 8081)

### 2. Keycloak konfigurieren

Nach dem Start der Container, führe das Setup-Script aus:

```bash
./setup-keycloak.sh
```

Das Script:
- Erstellt ein "systeminstaller" Realm
- Konfiguriert den "systeminstaller-web" Client
- Erstellt einen Test-User
- Zeigt das Client Secret an

### 3. Client Secret aktualisieren

Kopiere das vom Script angezeigte Client Secret und aktualisiere:

- `appsettings.Development.json`
- `appsettings.json`
- `docker-compose.yml`

Ersetze `your-client-secret` mit dem generierten Secret.

### 4. Anwendung neu starten

```bash
docker-compose restart web
```

## Verwendung

### Anmeldung

1. Gehe zu `http://localhost:8081`
2. Du wirst zu Keycloak weitergeleitet
3. Melde dich mit den Test-Credentials an:
   - **Benutzername**: `testuser`
   - **Passwort**: `password123`

### Abmeldung

Klicke auf "Abmelden" in der Navigation oder gehe zu `/logout`

## Konfiguration

### Keycloak Admin Console

- **URL**: `http://localhost:8080`
- **Username**: `admin`
- **Password**: `admin123`

### Neue Benutzer hinzufügen

1. Gehe zur Keycloak Admin Console
2. Wähle das "systeminstaller" Realm
3. Gehe zu "Users" > "Add user"
4. Fülle die Details aus und setze ein Passwort

### Client-Konfiguration

Der Client ist konfiguriert für:
- **Standard Flow**: Authorization Code Flow
- **Valid Redirect URIs**: `http://localhost:8081/*`
- **Web Origins**: `http://localhost:8081`

## Fehlerbehebung

### Keycloak startet nicht

- Überprüfe, ob Port 8080 frei ist
- Warte bis zu 60 Sekunden für den ersten Start
- Prüfe die Logs: `docker-compose logs keycloak`

### Anmeldung funktioniert nicht

- Stelle sicher, dass das Client Secret korrekt konfiguriert ist
- Überprüfe die Redirect URIs in Keycloak
- Prüfe die Logs: `docker-compose logs web`

### HTTPS-Fehler

In der Entwicklungsumgebung ist HTTP erlaubt. Für Production sollte HTTPS konfiguriert werden.

## Produktionshinweise

Für die Produktion sollten folgende Punkte beachtet werden:

1. **HTTPS**: Aktiviere `RequireHttpsMetadata: true`
2. **Sichere Passwörter**: Ändere Admin-Passwörter
3. **Client Secrets**: Verwende sichere, zufällige Secrets
4. **Realm-Konfiguration**: Anpassen der Token-Lebensdauer
5. **Backup**: Regelmäßige Backups der Keycloak-Datenbank
