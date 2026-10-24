# AGM API

ASP.NET Core 8 Web API mit EF Core 9 und PostgreSQL (NetTopologySuite für Feldgeometrien).
Authentifizierung über Keycloak (JWT), lokal optional ohne Login.

## Lokal starten

Voraussetzungen: .NET 8 SDK, PostgreSQL mit PostGIS.

```bash
dotnet run          # http://0.0.0.0:5199, Swagger unter /swagger
```

- Connection String: `appsettings.Development.json` → `ConnectionStrings:Local`
- `Keycloak:Enabled=false` (Development): kein Login nötig, jeder Request läuft als fester Dev-User (`Services/Auth/DevAuthHandler.cs`)
- Datenbank-Migrationen und Stammdaten (`Services/SeedData/SeedData.json`) werden beim Start automatisch angewendet
- Neue Migration: `dotnet ef migrations add <Name>`
- Health-Check: `GET /health`

## Konfiguration

Alle Werte lassen sich per Umgebungsvariable setzen (`:` wird zu `__`, z. B. `Keycloak__Authority`).

| Schlüssel | Bedeutung |
|---|---|
| `ConnectionStrings:Local` | PostgreSQL-Verbindung |
| `Keycloak:Enabled` | `false` nur lokal |
| `Keycloak:Authority` | z. B. `https://agm-auth.up.railway.app/realms/agm` |
| `Keycloak:Audience` | `agm-api` |
| `Keycloak:AdminClientId` / `AdminClientSecret` | Service-Account für die Konto-Löschung (siehe `AGM_Keycloak/README.md`) |
| `Uploads:Path` | Speicherort für Fotos und Logos (Docker: `/app/data/uploads`, lokal: `App_Data/uploads`) |
| `Uploads:SigningKey` | Geheimer Schlüssel für signierte Datei-URLs (ohne: zufällig, URLs ungültig nach Neustart) |
| `Uploads:PublicBaseUrl` | Optional, öffentliche Basis-URL für Datei-Links |
| `CorsSettings:AllowedOrigins` | Erlaubte Web-Origins (leer = alle) |

## Wichtige Konzepte

- **Berechtigungen:** Jeder Endpoint mit Farm-Daten prüft die Mitgliedschaft über `FarmAuthorizationService`
  (lesen: Mitglied, schreiben: Rollen AD/MN/WR, Farm-Admin: AD).
- **Dateien:** Uploads laufen über `FileStorageService`. In der DB stehen nur relative Pfade;
  nach außen gibt die API signierte Links aus, die 24 h gültig sind (`GET /api/files/...`).
- **Konto löschen:** `DELETE /api/profile/me` löscht den Keycloak-Login, Farmen ohne weitere Mitglieder
  und anonymisiert die persönlichen Daten (`AccountDeletionService`).

## Projektstruktur

```
Controllers/   Endpunkte, Records/ enthält die DTOs
Models/        EF-Entities
Database/      AppDbContext
Migrations/    EF-Migrationen
Services/      Berechtigungen, Dateien, Keycloak, Aktivitäts-Log, Seed-Daten
```

## Deployment (Railway)

`.github/workflows/deploy.yml`:

| Auslöser | Ablauf |
|---|---|
| Pull Request | Build (+ Tests) |
| Push auf `develop` | Build → Deploy auf QA (übersprungen, solange `RAILWAY_TOKEN_QA` fehlt) |
| Push auf `main` | Build → wartet auf Freigabe → Deploy auf Production |

Einmalig einrichten:

1. **Railway:** im Projekt eine zweite Umgebung `qa` anlegen (Environments → New → Duplicate production),
   dort eine eigene Datenbank verwenden und `ASPNETCORE_ENVIRONMENT=QA` setzen. Für jede Umgebung einen
   Projekt-Token erzeugen (Settings → Tokens).
2. **GitHub** (Repo → Settings → Environments):
   - Environment `qa` mit Secret `RAILWAY_TOKEN_QA`
   - Environment `production` mit Secret `RAILWAY_TOKEN` und **Required reviewers** (du selbst) – dann
     wartet jeder Prod-Deploy auf deinen Klick „Approve“
3. Branch `develop` als Standard-Branch für Pull Requests verwenden; `main` mit Branch-Protection schützen.

Auf Railway (je Umgebung) einstellen:

1. **Volume** auf `/app/data` mounten – sonst gehen hochgeladene Fotos bei jedem Deploy verloren.
2. Variablen: `ConnectionStrings__Local`, `Keycloak__Authority`, `Keycloak__AdminClientId`, `Keycloak__AdminClientSecret`,
   `Uploads__SigningKey`, optional `Uploads__PublicBaseUrl` und `CorsSettings__AllowedOrigins__0` usw.
