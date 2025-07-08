# SystemInstaller

Ein Web-basiertes Dashboard zur Verwaltung von Docker-Environment-Installationen mit Keycloak-Authentifizierung.

## Features

- 🐳 **Docker Environment Management**: Verwalte und überwache Docker-basierte Installationen
- 🔐 **Keycloak Authentifizierung**: Sichere Anmeldung mit OpenID Connect
- 📊 **Dashboard**: Übersicht über alle laufenden und abgeschlossenen Installations-Tasks
- 🏗️ **Environment Management**: Erstelle und verwalte verschiedene Umgebungen
- 🔄 **Real-time Updates**: Live-Updates des Installationsstatus
- 📱 **Responsive Design**: Optimiert für Desktop und Mobile

## Technologie-Stack

- **Backend**: ASP.NET Core 8.0 mit Blazor Server
- **Frontend**: Blazor Components mit Bootstrap
- **Datenbank**: SQL Server mit Entity Framework Core
- **Authentifizierung**: Keycloak mit OpenID Connect
- **Containerisierung**: Docker & Docker Compose

## Quick Start

### 1. Voraussetzungen

- Docker & Docker Compose
- .NET 8.0 SDK (für lokale Entwicklung)
- `jq` (für Keycloak Setup Script)

### 2. Container starten

```bash
# Alle Services starten
docker-compose up -d

# Logs verfolgen
docker-compose logs -f
```

### 3. Keycloak konfigurieren

Nach dem ersten Start der Container:

```bash
# Keycloak Setup ausführen
./setup-keycloak.sh
```

Das Script gibt ein **Client Secret** aus. Kopiere es und aktualisiere:

- `appsettings.Development.json`
- `appsettings.json` 
- `docker-compose.yml`

Ersetze `your-client-secret` mit dem generierten Secret.

### 4. Web-App neu starten

```bash
docker-compose restart web
```

### 5. Anwendung verwenden

- **SystemInstaller Dashboard**: http://localhost:8081
- **Keycloak Admin Console**: http://localhost:8080

**Test-Login:**
- Username: `testuser`
- Password: `password123`

## Entwicklung

### Lokale Entwicklung

```bash
# Nur Datenbank und Keycloak starten
docker-compose up -d db keycloak

# Keycloak konfigurieren
./setup-keycloak.sh

# Anwendung lokal starten
dotnet run
```

### Neue Migration erstellen

```bash
dotnet ef migrations add YourMigrationName
```

### Datenbank zurücksetzen

```bash
docker-compose down -v
docker-compose up -d
./setup-keycloak.sh
```

## Projektstruktur

```
SystemInstaller/
├── Components/
│   ├── Layout/          # Layout-Komponenten
│   └── Pages/           # Seiten-Komponenten
├── Data/                # Entity Models & DbContext
├── Services/            # Business Logic Services
├── Migrations/          # Entity Framework Migrationen
├── docker-compose.yml   # Container-Konfiguration
├── setup-keycloak.sh    # Keycloak Setup Script
└── AUTHENTICATION.md    # Authentifizierung-Dokumentation
```

## Konfiguration

### Umgebungsvariablen

| Variable | Beschreibung | Default |
|----------|--------------|---------|
| `ConnectionStrings__DefaultConnection` | SQL Server Verbindung | siehe appsettings.json |
| `Authentication__Keycloak__Authority` | Keycloak Realm URL | http://keycloak:8080/realms/systeminstaller |
| `Authentication__Keycloak__ClientId` | Keycloak Client ID | systeminstaller-web |
| `Authentication__Keycloak__ClientSecret` | Keycloak Client Secret | ⚠️ Muss gesetzt werden |

### Ports

| Service | Port | Beschreibung |
|---------|------|--------------|
| Web App | 8081 | SystemInstaller Dashboard |
| Keycloak | 8080 | Identity Provider |
| SQL Server | 1433 | Datenbank |

## Troubleshooting

### Container starten nicht

```bash
# Prüfe Ports
netstat -tulpn | grep -E "(8080|8081|1433)"

# Logs anzeigen
docker-compose logs [service-name]

# Container neu bauen
docker-compose build --no-cache
```

### Authentifizierung funktioniert nicht

1. Prüfe Client Secret in allen Konfigurationsdateien
2. Stelle sicher, dass Keycloak vollständig gestartet ist
3. Prüfe Redirect URIs in Keycloak Admin Console

### Datenbank-Probleme

```bash
# Datenbank-Container neu starten
docker-compose restart db

# Migrations manuell ausführen
dotnet ef database update
```

## API-Dokumentation

Die Anwendung bietet folgende Endpunkte:

- `GET /` - Dashboard
- `GET /environments` - Umgebungsliste
- `GET /environments/new` - Neue Umgebung erstellen
- `GET /installations` - Installation-Tasks
- `GET /login` - Anmeldung
- `POST /logout` - Abmeldung

## Deployment

### Produktion

Für Production-Deployment:

1. Aktualisiere `appsettings.json` mit Production-Werten
2. Setze `RequireHttpsMetadata: true`
3. Verwende sichere Passwörter für Keycloak Admin
4. Konfiguriere SSL/TLS Zertifikate
5. Backup-Strategie für Datenbanken implementieren

## Beitragen

1. Fork das Repository
2. Erstelle einen Feature-Branch
3. Committe deine Änderungen
4. Erstelle einen Pull Request

## Lizenz

Dieses Projekt steht unter der MIT-Lizenz. Siehe [LICENSE](LICENSE) für Details.

## Support

Bei Fragen oder Problemen:

1. Prüfe die [Troubleshooting](#troubleshooting) Sektion
2. Durchsuche die [Issues](../../issues)
3. Erstelle ein neues Issue mit detaillierter Beschreibung
