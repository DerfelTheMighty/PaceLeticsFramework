# Laufanalyse-Modul

Das Laufanalyse-Modul bildet einen eigenstaendigen Workflow fuer
Laufanalyse-Events ab. Die Kursverwaltung bleibt die Quelle fuer Kurse, Events
und Registrierungen. Das Laufanalyse-Modul uebernimmt danach die Analyse-spezifischen
Aufgaben: Teilnehmende verwalten, Google-Drive-Ordner bereitstellen,
Kameraaufnahmen hochladen und Analyse-Links fuer Athletes anzeigen.

## Zielbild

- Trainer erstellen in einem Kurs ein Event vom Typ `RunningAnalysis`.
- Athletes registrieren sich fuer dieses Event.
- Bei der Registrierung wird ein persoenlicher Google-Drive-Ordner vorbereitet.
- Athletes sehen den Ordnerlink unter `Meine Analysen`.
- Trainer starten am Analysetag die Session, waehlen Teilnehmende aus und nehmen
  Videos im Browser auf.
- Jede erfolgreiche Aufnahme wird in den persoenlichen Drive-Ordner des
  Teilnehmenden hochgeladen und als primaere Aufnahme markiert.
- Der Workflow benoetigt fuer Aufnahme und Upload eine Online-Verbindung.

## Projektstruktur

- `PaceLetics.RunningAnalysisModule.CodeBase`
  Enthaelt Domain-Modelle, Service-Interfaces, Kernlogik und Regeln.
- `PaceLetics.RunningAnalysisModule.Components`
  Enthaelt wiederverwendbare Razor-Komponenten fuer Roster, Linkliste,
  Kameraansicht und Browser-MediaRecorder.
- `PaceLetics.RunningAnalysisModule.Infrastructure.GoogleDrive`
  Implementiert den Storage-Port fuer Google Drive.
- `PaceLetics.Web.Services.RunningAnalysis`
  Verbindet das Modul mit Cosmos DB, Kursregistrierungen und der Web-App.
- `PaceLetics.Web.Pages.Trainers`
  Stellt Trainer-Uebersicht und Analyse-Session bereit.
- `PaceLetics.Web.Pages.Athletes`
  Stellt Kursregistrierung und Analyse-Links fuer Athletes bereit.

## Rollen und Navigation

### Trainer

Trainer benoetigen die ASP.NET-Identity-Rolle `ApplicationRoles.Trainer`.
Nur dann erscheinen die Trainerbereiche in der Navigation.

Wichtige Seiten:

- `/Trainers/courses`
  Kursverwaltung. Hier werden Kurse, Termine und Events gepflegt.
- `/Trainers/running-analyses`
  Direkte Uebersicht aller Laufanalyse-Events der eigenen Trainerkurse.
- `/Trainers/courses/{CourseId}/events/{EventId}/running-analysis`
  Durchfuehrung einer konkreten Laufanalyse-Session.

### Athlete

Athletes benoetigen nur eine normale Anmeldung. Sie nutzen:

- `/Athletes/courses`
  Kursbeitritt, Eventregistrierung und eventbezogener Ordnerstatus.
- `/Athletes/analyses`
  Uebersicht der eigenen Analyse-Ordnerlinks.

Trainer koennen gleichzeitig Athlete sein. Deshalb kann ein Trainer sowohl
`Meine Analysen` als auch `Laufanalysen` sehen.

## Bedienung fuer Trainer

### 1. Laufanalyse-Event erstellen

1. Als Trainer anmelden.
2. `Kurse verwalten` oeffnen.
3. Einen Kurs auswaehlen oder erstellen.
4. Im Eventbereich ein neues Event anlegen.
5. Als Eventtyp `Laufanalyse` auswaehlen.
6. Titel, Start, Ende, optional Ort, Beschreibung, Kapazitaet und Deadline
   setzen.
7. Event speichern.

Das Event ist ab dann fuer Athletes im Kurs sichtbar. Erst bei der
Athlete-Registrierung werden Teilnehmende und Drive-Ordner vorbereitet.

### 2. Drive-Konfiguration pruefen

Die Seite `Laufanalysen` zeigt Warnungen, wenn Google Drive noch nicht
konfiguriert ist:

- Fehlende Service-Account-Zugangsdaten verhindern Ordneranlage, Freigaben und
  Uploads.
- Eine fehlende `RootFolderId` ist nicht zwingend blockierend. Dann versucht der
  Adapter, den Root-Ordner ueber den konfigurierten Namen zu finden oder
  anzulegen.

Der Drive-Zugang ist zentral. Es gibt keinen eigenen Drive-Account pro Trainer.
Das Modul schreibt mit einem Service Account in einen zentralen Drive-Bereich.
Teilnehmende bekommen nur Links auf ihre freigegebenen Ordner.

### 3. Analyse starten

Es gibt zwei Einstiege:

1. `Laufanalysen` oeffnen und beim passenden Event `Analyse starten` waehlen.
2. Oder `Kurse verwalten` oeffnen, im Eventbereich das Laufanalyse-Event suchen
   und dort `Analyse starten` waehlen.

Beim Oeffnen der Session wird das Laufanalyse-Event vorbereitet oder aktualisiert.
Die Session zeigt:

- Eventstatus: `Entwurf`, `Vorbereitet`, `Laeuft`, `Abgeschlossen`.
- Online-Status des Browsers.
- Roster aller registrierten Teilnehmenden.
- Aufnahmeaktionen fuer den ausgewaehlten Teilnehmenden.

### 4. Teilnehmende aufnehmen

1. In der Roster-Liste einen Teilnehmenden auswaehlen.
2. `Record` bzw. die Aufnahmeaktion oeffnen.
3. Browser-Kamera freigeben.
4. `Start` druecken.
5. Teilnehmenden vorbeilaufen lassen.
6. `Stop` druecken.
7. Die Aufnahme wird automatisch hochgeladen.

Danach stehen zur Verfuegung:

- `Again`
  Beim selben Teilnehmenden bleiben und eine weitere Aufnahme starten.
- `Next`
  Zum naechsten Teilnehmenden wechseln.
- `List`
  Zurueck zur Roster-Liste.

Trainer koennen jederzeit zur Liste zurueckgehen, einen Teilnehmenden erneut
auswaehlen und eine weitere Aufnahme machen. Jede erfolgreiche neue Aufnahme
wird zur primaeren Aufnahme. Vorherige erfolgreiche Aufnahmen bleiben im Drive.

### 5. Analyse abschliessen

Nach der Session `Analyse abschliessen` waehlen. Das setzt den Status auf
`Completed` und blendet weitere Aufnahmen fuer diese Session aus.

## Bedienung fuer Athletes

### 1. Kurs beitreten

1. Als Athlete anmelden.
2. `Meine Kurse` oeffnen.
3. Einen Kurs hinzufuegen oder einen bestehenden Kurs auswaehlen.

### 2. Fuer Laufanalyse registrieren

1. Im Kurs den Eventbereich ansehen.
2. Ein Event mit Chip `Laufanalyse` suchen.
3. Registrieren.

Direkt nach der Registrierung startet im Hintergrund die Ordnerbereitstellung.
Die Registrierung bleibt gueltig, auch wenn die Drive-Bereitstellung fehlschlaegt.
Der Fehler wird im Laufanalyse-Modul gespeichert und kann spaeter untersucht
oder durch erneute Bereitstellung behoben werden.

### 3. Analyse-Ordner oeffnen

Athletes finden ihren Ordnerlink an zwei Stellen:

- Im jeweiligen Kurs unter dem registrierten Laufanalyse-Event.
- Unter `Meine Analysen`.

Der Link ist erst nutzbar, wenn Ordner und Berechtigung bereit sind. Der
Ordnerstatus kann sinngemaess sein:

- Vorbereitung laeuft.
- Ordner ist bereit.
- Bereitstellung ist fehlgeschlagen.

Die Freigabe erfolgt an die E-Mail-Adresse des Identity-Users. Der Ordner wird
mit Schreibrechten freigegeben. Neue Trainer-Aufnahmen landen automatisch in
diesem Ordner.

## Was im Hintergrund passiert

### Registrierung

1. `CourseService.RegisterForEventAsync` registriert den Athlete fuer ein
   Kurs-Event.
2. Nur wenn `CourseEventDocument.EventType == CourseEventTypes.RunningAnalysis`,
   ruft der Kursservice den Adapter
   `ICourseRunningAnalysisRegistrationAdapter` auf.
3. `CourseRunningAnalysisRegistrationAdapter` ermittelt User, Athlete-Profil,
   Anzeigenamen und E-Mail-Adresse.
4. Der Adapter ruft
   `IRunningAnalysisService.RegisterParticipantAsync` mit einer
   `RunningAnalysisRegistration` auf.

### Event-Vorbereitung

`RunningAnalysisService.PrepareEventAsync` erstellt oder aktualisiert ein
`RunningAnalysisEvent` anhand der externen Kurs-Event-ID.

Wichtige Felder:

- `ExternalEventId`: ID des Kurs-Events.
- `CourseId`: Kurs-ID.
- `Title`, `StartsAt`, `EndsAt`: aus dem Kurs-Event.
- `Status`: startet als `Prepared`, sobald das Event vorbereitet wurde.
- `DriveFolderId`, `DriveFolderUrl`: Drive-Referenz des Event-Ordners.

### Participant-Provisioning

Nach der Registrierung wird ein `RunningAnalysisParticipant` erstellt oder
aktualisiert.

Der Service versucht dann:

1. Ueber `IUserDriveFolderRegistry.FindReusableFolderAsync` einen vorhandenen
   Ordner fuer denselben Kurs, dasselbe Event und denselben Athlete zu finden.
2. Falls keiner existiert, ueber `IRunningAnalysisStorageProvider` einen
   Event-Ordner im zentralen Drive-Bereich zu sichern.
3. Darunter einen Teilnehmenden-Ordner anzulegen.
4. Die Ordnerreferenz ueber `IUserDriveFolderRegistry.SaveFolderReferenceAsync`
   zu speichern.
5. Den Ordner fuer die Athlete-E-Mail mit `writer`-Rechten freizugeben.

Wenn ein Schritt fehlschlaegt:

- Die Kursregistrierung wird nicht zurueckgerollt.
- Der Participant bleibt gespeichert.
- `FolderStatus`, `PermissionStatus` und `ProvisioningError` dokumentieren den
  Zustand.

### Ordnerstruktur in Google Drive

Der Google-Drive-Provider arbeitet in einem zentralen Root-Bereich.

Typische Struktur:

```text
PaceLetics Laufanalysen
  2026-06-06 Laufanalyse Kurs A
    Anna Beispiel - abc123
      20260606-153000-Anna-Beispiel.webm
    Max Beispiel - def456
      20260606-153900-Max-Beispiel.webm
```

Event-Ordner:

```text
yyyy-MM-dd {EventTitle}
```

Participant-Ordner:

```text
{DisplayName} - {letzte 6 Zeichen der AthleteUserId}
```

Dateinamen fuer Aufnahmen:

```text
yyyyMMdd-HHmmss-{DisplayName}.{webm|mp4}
```

### Aufnahme und Upload

1. Die Trainer-Session laedt das Roster ueber
   `IRunningAnalysisService.GetRosterAsync`.
2. Beim Oeffnen der Kamera wird der Browser-Online-Status ueber
   `runningAnalysisMediaRecorder.js` abgefragt.
3. `RunningAnalysisMediaRecorder` verwendet `navigator.mediaDevices.getUserMedia`
   und `MediaRecorder`.
4. Es wird Video ohne Audio aufgenommen.
5. Bevorzugte Formate sind `webm` mit VP9, `webm` mit VP8, `webm`, danach `mp4`,
   je nach Browser-Support.
6. Nach `Stop` liefert JavaScript die Videodaten an Blazor zurueck.
7. `RunningAnalysisService.UploadRecordingAsync` prueft:
   - Online-Status muss true sein.
   - Dateiname muss vorhanden sein.
   - Participant muss zum Analyse-Event gehoeren.
   - Participant-Ordner muss bereit sein.
8. Der Service speichert zunaechst eine Aufnahme mit Status `Uploading`.
9. Der Google-Drive-Provider laedt die Datei in den Participant-Ordner.
10. Bei Erfolg wird die Aufnahme `Uploaded`, bekommt Drive-ID und Drive-Link und
    wird als `IsPrimary` markiert.
11. Bei Fehler wird die Aufnahme `Failed` und nicht primaer.

## Zentrale Regeln

- Eine Aufnahme ist nur online erlaubt.
- Eine erfolgreiche neue Aufnahme wird primaer.
- Fehlgeschlagene Uploads ersetzen die primaere Aufnahme nicht.
- Drive-Provisioning-Fehler blockieren die Kursregistrierung nicht.
- Teilnehmende bekommen Schreibrechte auf ihren eigenen Ordner.
- Der Service Account wird nicht im Git-Repository gespeichert.

## Konfiguration

Die Konfiguration liegt im Abschnitt:

```json
{
  "PaceLeticsUserData": {
    "GoogleDrive": {
      "ApplicationName": "PaceLetics",
      "RootFolderId": "",
      "RootFolderName": "paceletics_user_data",
      "DelegatedUserEmail": "",
      "OAuthClientId": "",
      "OAuthClientSecret": "",
      "OAuthRefreshToken": "",
      "OAuthUserEmail": ""
    }
  }
}
```

Echte Zugangsdaten duerfen nicht in `PaceLetics.Web/appsettings.json`.
Die getrackte Datei enthaelt nur nicht-geheime Defaults.
Die neue primaere Konfiguration heisst `PaceLeticsUserData:GoogleDrive`. Werte
unter `PaceLeticsUserData` und `RunningAnalysis:GoogleDrive` werden noch als
Fallback gelesen, damit bestehende Umgebungen nicht sofort brechen.

### Empfohlene lokale Konfiguration mit User-Secrets

```powershell
dotnet user-secrets set "PaceLeticsUserData:GoogleDrive:RootFolderId" "<central-folder-id>" --project PaceLetics.Web
dotnet user-secrets set "PaceLeticsUserData:GoogleDrive:ServiceAccountJsonPath" "C:\secure\paceletics-google-drive-service-account.json" --project PaceLetics.Web
```

Wenn der Root-Ordner in einem normalen Google-My-Drive liegt, benoetigt der
Service Account Domain-wide Delegation auf einen echten Workspace-User mit
Drive-Speicherquote:

```powershell
dotnet user-secrets set "PaceLeticsUserData:GoogleDrive:DelegatedUserEmail" "drive-owner@example.com" --project PaceLetics.Web
```

Ohne Google Workspace kann der Provider stattdessen mit einem normalen
Google-OAuth-Refresh-Token arbeiten. Dann gehoeren hochgeladene Dateien dem
Google-Konto, das den OAuth-Zugriff erlaubt hat:

```powershell
dotnet user-secrets set "PaceLeticsUserData:GoogleDrive:OAuthClientId" "<oauth-client-id>" --project PaceLetics.Web
dotnet user-secrets set "PaceLeticsUserData:GoogleDrive:OAuthClientSecret" "<oauth-client-secret>" --project PaceLetics.Web
dotnet user-secrets set "PaceLeticsUserData:GoogleDrive:OAuthRefreshToken" "<oauth-refresh-token>" --project PaceLetics.Web
dotnet user-secrets set "PaceLeticsUserData:GoogleDrive:OAuthUserEmail" "drive-owner@gmail.com" --project PaceLetics.Web
```

Alternativ kann der komplette JSON-Inhalt gesetzt werden:

```powershell
dotnet user-secrets set "PaceLeticsUserData:GoogleDrive:ServiceAccountJson" "<service-account-json>" --project PaceLetics.Web
```

Das ist praktisch fuer Hosting-Umgebungen, aber fuer lokale Entwicklung ist ein
Dateipfad meist lesbarer.

### Konfiguration mit Environment-Variablen

```powershell
$env:PaceLeticsUserData__GoogleDrive__RootFolderId = "<central-folder-id>"
$env:PaceLeticsUserData__GoogleDrive__ServiceAccountJsonPath = "C:\secure\paceletics-google-drive-service-account.json"
$env:PaceLeticsUserData__GoogleDrive__DelegatedUserEmail = "drive-owner@example.com"
```

oder:

```powershell
$env:PaceLeticsUserData__GoogleDrive__ServiceAccountJson = "<service-account-json>"
```

oder fuer OAuth mit einem echten Google-Drive-Konto:

```powershell
$env:PaceLeticsUserData__GoogleDrive__OAuthClientId = "<oauth-client-id>"
$env:PaceLeticsUserData__GoogleDrive__OAuthClientSecret = "<oauth-client-secret>"
$env:PaceLeticsUserData__GoogleDrive__OAuthRefreshToken = "<oauth-refresh-token>"
$env:PaceLeticsUserData__GoogleDrive__OAuthUserEmail = "drive-owner@gmail.com"
```

### Lokale Datei fuer Entwicklung

Fuer lokale Entwicklung kann
`PaceLetics.Web/appsettings.Local.example.json` nach
`PaceLetics.Web/appsettings.Local.json` kopiert werden.

`appsettings.Local.json` ist in `.gitignore` eingetragen und wird in Development
optional geladen.

### Google-Drive-Vorbereitung

1. Google Cloud Projekt mit Drive API vorbereiten.
2. Service Account erstellen.
3. Service-Account-Key als JSON erzeugen.
4. JSON ausserhalb des Repositories speichern.
5. Zentralen Drive-Ordner erstellen.
6. Fuer produktive Uploads entweder einen Shared Drive verwenden oder
   Domain-wide Delegation fuer den Service Account aktivieren und
   `DelegatedUserEmail` auf einen Workspace-User mit Drive-Speicherquote setzen.
7. Zentralen Drive-Ordner mit der Service-Account-E-Mail oder dem delegierten
   Workspace-User teilen.
8. Die Folder-ID des zentralen Ordners als
   `PaceLeticsUserData:GoogleDrive:RootFolderId` konfigurieren.

### Google-Drive-OAuth ohne Workspace

Wenn kein Google Workspace vorhanden ist, wird kein Service Account fuer
Uploads in normale My-Drive-Ordner verwendet. Stattdessen wird ein OAuth-Client
in Google Cloud erstellt und einmalig mit dem echten Google-Konto autorisiert,
das den PaceLetics-Ordner besitzt.

1. Google Cloud Projekt mit Drive API vorbereiten.
2. OAuth Consent Screen konfigurieren.
3. OAuth Client vom Typ Web Application oder Desktop App erstellen.
4. Das Google-Konto autorisieren und einen Refresh Token mit Scope
   `https://www.googleapis.com/auth/drive` erzeugen.
5. `OAuthClientId`, `OAuthClientSecret`, `OAuthRefreshToken` und optional
   `OAuthUserEmail` konfigurieren.
6. Die Folder-ID des PaceLetics-Ordners aus diesem Google Drive als
   `PaceLeticsUserData:GoogleDrive:RootFolderId` setzen.

Sind vollstaendige OAuth-Werte gesetzt, verwendet der Provider automatisch
OAuth statt Service-Account-Zugangsdaten.

Ohne `RootFolderId` versucht der Adapter, einen Root-Ordner ueber
`RootFolderName` zu finden oder anzulegen. Fuer produktive Nutzung ist eine
explizite `RootFolderId` robuster.

## Persistenz

Die Web-Integration verwendet `CosmosRunningAnalysisRepository`.

Gespeichert wird im konfigurierten `AthleteDataOptions.CourseContainerName`.
Die Laufanalyse-Dokumente sind ueber `DocumentType` getrennt:

- `runningAnalysisEvent`
  Analyse-Event und Status.
- `runningAnalysisParticipant`
  Teilnehmender, Folderstatus, Permissionstatus und Drive-Link.
- `runningAnalysisRecording`
  Aufnahmeversuch, Uploadstatus, Drive-Datei und Primaerkennzeichnung.
- `runningAnalysisFolderReference`
  Wiederverwendbare Ordnerreferenz fuer Kurs, Event und Athlete.

## Wichtige Interfaces

- `IRunningAnalysisService`
  Hauptservice fuer Event-Vorbereitung, Registrierung, Roster, Athlete-Links,
  Start, Abschluss und Upload.
- `IRunningAnalysisRepository`
  Persistenz fuer Events, Participants und Recordings.
- `IRunningAnalysisStorageProvider`
  Abstraktion fuer externe Ablage. Aktuell durch Google Drive implementiert.
- `IUserDriveFolderRegistry`
  Sucht und speichert wiederverwendbare Drive-Ordner.
- `IRunningAnalysisClock`
  Kapselt Zeit fuer testbare Domainlogik.
- `ICourseRunningAnalysisRegistrationAdapter`
  Koppelt Kursregistrierung an Laufanalyse-Provisioning.

## Statuswerte

### Eventstatus

- `Draft`
  Noch nicht vorbereitet.
- `Prepared`
  Event ist im Laufanalyse-Modul angelegt.
- `InProgress`
  Trainer hat die Analyse gestartet oder die erste Aufnahme begonnen.
- `Completed`
  Trainer hat die Analyse abgeschlossen.

### Folderstatus

- `Missing`
  Noch kein Ordner bekannt.
- `Creating`
  Ordnerbereitstellung laeuft.
- `Ready`
  Ordner ist vorhanden.
- `Failed`
  Ordnerbereitstellung ist fehlgeschlagen.

### Permissionstatus

- `Missing`
  Noch keine Freigabe bekannt.
- `Granting`
  Freigabe wird gesetzt.
- `Granted`
  Schreibrecht wurde vergeben.
- `Failed`
  Freigabe ist fehlgeschlagen.

### Uploadstatus

- `Captured`
  Aufnahme wurde erfasst, aber noch nicht hochgeladen.
- `Uploading`
  Upload laeuft.
- `Uploaded`
  Upload war erfolgreich.
- `Failed`
  Upload ist fehlgeschlagen.

## Browser- und Betriebsanforderungen

- Kameraaufnahme benoetigt einen Browser mit `getUserMedia` und `MediaRecorder`.
- Kameraaufnahme benoetigt in der Regel HTTPS oder localhost.
- Der Browser muss online sein.
- Audio wird nicht aufgenommen.
- Teilnehmerordner muessen vor Uploads bereit sein.
- Der Server benoetigt Zugriff auf die Google-Drive-API.

## Troubleshooting

### Trainer sieht nur `Meine Analysen`

Der Account hat vermutlich nicht die Rolle `ApplicationRoles.Trainer`.
Trainerbereiche erscheinen erst mit dieser Rolle.

### `Laufanalysen` ist leer

Es gibt in den Kursen des Trainers noch kein Event mit Eventtyp
`RunningAnalysis`. In `Kurse verwalten` ein neues Event vom Typ `Laufanalyse`
anlegen.

### Drive-Warnung erscheint

Service-Account-Zugangsdaten fehlen. User-Secrets, Environment-Variablen oder
`appsettings.Local.json` pruefen. Keine echten Keys in getrackte Dateien
schreiben.

### Athlete sieht keinen Ordnerlink

Moegliche Ursachen:

- Registrierung wurde noch nicht abgeschlossen.
- Provisioning laeuft noch.
- Service Account ist nicht konfiguriert.
- Root-Ordner ist nicht mit dem Service Account geteilt.
- Athlete-User hat keine E-Mail-Adresse.
- Google Drive konnte die Berechtigung nicht setzen.

### Aufnahme startet nicht

Moegliche Ursachen:

- Browser blockiert Kamera.
- Seite laeuft nicht ueber HTTPS oder localhost.
- Browser unterstuetzt `MediaRecorder` nicht.
- Browser ist offline.

### Upload schlaegt fehl

Moegliche Ursachen:

- Browser ist offline.
- Participant-Ordner ist nicht `Ready`.
- Service Account hat keinen Zugriff auf den Root-Ordner.
- Drive API ist nicht aktiviert.
- Service-Account-Key ist falsch oder nicht erreichbar.

## Erweiterungspunkte

- Weitere Storage-Provider koennen ueber `IRunningAnalysisStorageProvider`
  implementiert werden.
- Ein Retry-Job kann fehlgeschlagene Folder-Provisionings anhand von
  `FolderStatus`, `PermissionStatus` und `ProvisioningError` erneut versuchen.
- Weitere UI-Komponenten koennen auf `IRunningAnalysisService` aufbauen, ohne an
  die Kursverwaltung gekoppelt zu sein.
- Zusaetzliche Analyse-Artefakte koennen spaeter im Participant-Ordner abgelegt
  und ueber neue Recording- oder Result-Modelle referenziert werden.
