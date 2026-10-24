# AGM_API

ASP.NET Core 8 Web API, EF Core 9 mit Npgsql (PostgreSQL + NetTopologySuite für Feldgeometrien).

## Befehle
- Build (Pflicht vor „fertig“): `dotnet build`
- Starten: `dotnet run` (Swagger unter `/swagger` außer in Production, Health-Check `/health`)
- Migration anlegen: `dotnet ef migrations add <Name>` – Migrationen laufen beim Start automatisch (`Database.Migrate()` in `Program.cs`).
  Migrationen nie von Hand umschreiben, die schon deployed sind.
- Umgebungen: `appsettings.json`, `appsettings.Development.json`, `appsettings.QA.json`. Secrets über Env-Variablen (`ConnectionStrings__Local`, `Keycloak__Authority`).
- Deploy: Push auf `main` → GitHub Actions → Railway (Docker).

## Auth
- `Keycloak:Enabled=true` → JWT-Validierung gegen Keycloak (Audience `agm-api`). `false` → `Services/Auth/DevAuthHandler` (nur lokal!).
- `KeycloakClaimsTransformation` + `KeycloakUserProvisioningService` legen beim ersten Request den lokalen `User` an
  und setzen `ClaimTypes.NameIdentifier` auf die interne User-ID sowie `isAdmin`.
- Admin-Endpunkte: `[Authorize(Policy = "AdminOnly")]`.

## Pflicht-Muster für Farm-Daten
Jeder Endpoint, der Daten einer Farm berührt, prüft die Mitgliedschaft. Vorbild: `Controllers/Machine/MachineController.cs`.
Kurzform: `User.GetUserId()` (`Services/ClaimsPrincipalExtensions.cs`) und `_auth.GetFarmIdFor…Async(id)` für Machine, MachineService,
Task, Season, Stall, StallAnimal, StallEvent, Field, Action.
```csharp
var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
var farmId = await _auth.GetFarmIdForFieldAsync(fieldId);      // bzw. passender Helper
if (farmId == null) return NotFound();
if (!await _auth.IsMemberAsync(farmId.Value, userId)) return Forbid();   // lesen
if (!await _auth.CanWriteAsync(farmId.Value, userId)) return Forbid();   // schreiben (Rollen AD, MN, WR)
if (!await _auth.IsAdminAsync(farmId.Value, userId)) return Forbid();    // Farm-Admin (AD)
```
Fehlt ein Helper (z. B. Farm-ID für Maschine, Task, Stall, Saison), wird er in `Services/FarmAuthorizationService.cs` ergänzt.
Bei schreibenden Aktionen zusätzlich `ActivityLogService` aufrufen, wie in den bestehenden Controllern.

## Struktur
- `Controllers/<Bereich>/` – Controller, `[Route("api/[controller]")]`, `[ApiController]`, `[Authorize]`.
- `Controllers/Records/` – Request/Response-DTOs als C# `record`s. Entities nie direkt zurückgeben.
- `Models/<Bereich>/` – EF-Entities. `Database/AppDbContext.cs` – Konfiguration.
- `Services/SeedData/SeedData.json` – Stammdaten (Maschinentypen, Marken, Kulturen, Feldmaßnahmen-Typen …), wird beim Start geladen.
- Uploads (Farm-Logos, Fotos zu Feldmaßnahmen) laufen **nur** über `Services/FileStorageService`: speichern mit `SaveAsync`
  (vorher `ValidateImageAsync`), in der DB nur den relativen Pfad ablegen, nach außen immer `GetUrl(...)` (signierte URL,
  ausgeliefert vom `FilesController`). Nie direkt in `wwwroot` schreiben. Konfiguration: `Uploads:Path`, `Uploads:SigningKey`, `Uploads:PublicBaseUrl`.
- Account-Löschung: `AccountDeletionService` + `KeycloakAdminService` (`Keycloak:AdminClientId/AdminClientSecret`).

## Konventionen
- Lese-Queries mit `AsNoTracking()` und Projektion per `.Select(...)` in Records.
- Rückgaben: `NotFound()`, `Forbid()`, `BadRequest(...)`, `NoContent()` bei Update/Delete.
- Nach Änderungen an Records die TypeScript-Typen in `AGM_Mobile/src/types/index.ts` (und ggf. `AGM_Admin/src/types.ts`) anpassen.
