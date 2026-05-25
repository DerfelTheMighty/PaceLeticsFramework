# Optimierungsplan: Services, Provider, ViewModels, Factories und JSON-Konfiguration

## Ausgangslage

Im PaceLetics Framework sind Begriffe und Verantwortlichkeiten aktuell nicht konsequent getrennt:

- `DefinitionFactory` erzeugt Workout- und Exercise-Beispieldaten hart codiert in C#.
- `WorkoutFactory` ist faktisch nur ein dünner Wrapper um `new Workout(...)` und bringt keinen Mehrwert.
- `ExerciseProvider` und `WorkoutProvider` laden ihre Daten direkt aus `DefinitionFactory` und koppeln dadurch Datenquelle, Katalog und Laufzeitobjekterstellung.
- Im Running-Modul existiert bereits ein JSON-basierter Ansatz mit `RunningSessionFactory` und `TrainingPlanProvider`, allerdings mit anderen Namens- und Fehlerbehandlungsregeln.
- Reader/Writer-Klassen wie `PaceModelReaderWriter` mischen Persistenzbegriff und fachlichen Provider-Begriff.
- ViewModels sind als klar benannte eigene Schicht kaum sichtbar; Blazor Pages/Components greifen teilweise direkt auf Provider, Services und Domain-Objekte zu.

Das Ziel ist, dass fachliche Definitionen nicht mehr über Factory-Methoden gepflegt werden, sondern über validierte JSON-Dokumente. C#-Factories sollen nur noch Laufzeitobjekte aus bereits geladenen Definitionen erzeugen.

## Zielbild

Die Architektur soll vier Rollen sauber unterscheiden:

1. **Definitionen**
   Reine DTO-/Konfigurationsmodelle, die aus JSON geladen werden, z. B. `ExerciseDefinition`, `WorkoutDefinition`, `TrainingPlanDefinition`.

2. **Repositories / Stores**
   Lesen und validieren JSON-Dokumente. Sie kennen Dateipfade, `JsonSerializerOptions`, Schemaversionen und Fehlerdetails, erzeugen aber keine laufenden Workouts.

3. **Factories**
   Erzeugen Domain-/Runtime-Objekte aus validierten Definitionen, z. B. `Workout`, `Exercise`, `RunningSession`. Sie lesen keine Dateien und enthalten keine Beispieldaten.

4. **Services / Provider / ViewModels**
   - `Service`: fachlicher Anwendungsfall, orchestriert Repositories, Factories und weitere Abhängigkeiten.
   - `Provider`: read-only Zugriff auf einen Katalog oder Lookup, z. B. `IExerciseCatalog`.
   - `ViewModel`: UI-Zustand und UI-Aktionen für Blazor-Komponenten/Pages.

## Namenskonventionen

### Provider

Provider sollten nur verwendet werden, wenn eine Klasse einen bereits geladenen Katalog bereitstellt:

- `IExerciseCatalog` statt `IExerciseProvider`
- `IWorkoutCatalog` statt `IWorkoutProvider`, sofern nur Lookups und Previews angeboten werden
- Methoden: `GetById`, `TryGetById`, `GetAll`, `GetIds`, `GetVariants`

Wenn die Klasse fachliche Operationen ausführt, sollte sie `Service` heißen.

### Services

Services bilden Anwendungsfälle ab:

- `WorkoutService`: Auswahl, Start, aktive Workout-Session, Sets/Rounds
- `TrainingPlanService`: Laden und Bereitstellen von Trainingsplänen
- `AthleteService`: Athletenbezogene Operationen

Services dürfen Factories und Repositories verwenden, sollten aber keine JSON-Parsingdetails enthalten.

### Factories

Factories dürfen nur konstruieren, nicht laden:

- `WorkoutFactory.Create(WorkoutDefinition definition, WorkoutBuildOptions options)`
- `ExerciseFactory.Create(ExerciseDefinition definition)`
- `RunningSessionFactory.Create(RunningSessionDefinition definition)`

Zu vermeiden:

- `CreateExamples`
- `Load`
- Dateipfade in Factory-Methoden
- statische Factory-Methoden, die JSON lesen und gleichzeitig Domain-Objekte bauen

### Repositories / Stores

JSON-Zugriff sollte explizit benannt sein:

- `IExerciseDefinitionRepository`
- `JsonExerciseDefinitionRepository`
- `IWorkoutDefinitionRepository`
- `JsonWorkoutDefinitionRepository`
- `JsonTrainingPlanRepository`

Methoden:

- `LoadAll()`
- `LoadById(string id)`
- `Save(...)`, falls Bearbeitung aus der App heraus geplant ist

## Zielstruktur pro Modul

```text
PaceLetics.WorkoutModule.CodeBase
  Definitions/
    ExerciseDefinition.cs
    WorkoutDefinition.cs
    WorkoutCatalogDocument.cs
  Repositories/
    IExerciseDefinitionRepository.cs
    JsonExerciseDefinitionRepository.cs
    IWorkoutDefinitionRepository.cs
    JsonWorkoutDefinitionRepository.cs
  Factories/
    IExerciseFactory.cs
    ExerciseFactory.cs
    IWorkoutFactory.cs
    WorkoutFactory.cs
  Services/
    WorkoutService.cs
  Catalogs/
    ExerciseCatalog.cs
    WorkoutCatalog.cs
```

Das Running-Modul sollte anschließend analog sortiert werden:

```text
PaceLetics.RunningModule.CodeBase
  Definitions/
  Repositories/
  Factories/
  Services/
```

## JSON-Steuerung

Workout- und Exercise-Daten sollten in JSON-Dokumente ausgelagert werden:

```text
PaceLetics.Web/wwwroot/data/workouts/exercises.json
PaceLetics.Web/wwwroot/data/workouts/workouts.json
```

Langfristig besser:

```text
PaceLetics.Web/wwwroot/data/workouts/catalog.de.json
PaceLetics.Web/wwwroot/data/workouts/catalog.en.json
```

Ein gemeinsames Katalogdokument reduziert Cross-File-Fehler:

```json
{
  "schemaVersion": 1,
  "exercises": [
    {
      "id": "glute-bridge",
      "variant": "easy",
      "name": "Glute Bridge",
      "description": "Statische Übung für Hüftstreckung, Hüftstabilität und Abdruck",
      "execution": [
        "Lege dich auf den Rücken und stelle die Beine an.",
        "Drücke mit den Fußsohlen gegen den Boden."
      ],
      "duration": 30,
      "imageFile": "glute_bridge_base.png",
      "level": "Easy",
      "switchLeftRight": false,
      "switchTime": 0
    }
  ],
  "workouts": [
    {
      "id": "stabi-handout-easy",
      "baseId": "stabi-handout",
      "name": "Stabi Handout",
      "description": "Das Basisprogramm für den Rumpf",
      "level": "Easy",
      "preparationTime": 10,
      "restTime": 10,
      "switchTime": 5,
      "exercises": [
        "glute-bridge:easy"
      ]
    }
  ]
}
```

Empfehlung: IDs sollten technische Slugs sein, Anzeigenamen bleiben übersetzbar. Varianten sollten nicht über freie Text-IDs wie `"Glute Bridge Easy"` kodiert werden.

## Validierungsregeln

Vor dem Erzeugen von Runtime-Objekten muss der JSON-Katalog validiert werden:

- `schemaVersion` ist vorhanden und unterstützt.
- Jede `id` ist eindeutig.
- Jede Workout-Referenz zeigt auf eine existierende Exercise-Definition.
- `level` ist ein gültiger Enum-Wert.
- `duration`, `preparationTime`, `restTime`, `switchTime` sind nicht negativ.
- `imageFile` existiert im erwarteten Asset-Verzeichnis oder wird als fehlendes Asset gemeldet.
- Workouts enthalten mindestens eine Exercise.
- Für jede Workout-Variante ist `baseId` gesetzt, damit Preview-Gruppierung nicht über Anzeigenamen läuft.

Ungültige Daten sollten beim Start oder beim Laden sichtbar fehlschlagen. Stilles Überspringen wie im aktuellen `TrainingPlanProvider` ist für produktive Katalogdaten riskant; besser ist ein `CatalogValidationException` mit Datei, Pfad und Fehlerliste.

## Migrationsplan

### Phase 1: Begriffe stabilisieren

- `DefinitionFactory.CreateExerciseExamples()` und `CreateWorkoutExamples()` als Legacy markieren.
- `WorkoutFactory` entweder entfernen oder zu einer echten `IWorkoutFactory` ausbauen.
- Begriffe dokumentieren: Repository lädt JSON, Factory baut Runtime, Catalog bietet Lookups, Service orchestriert Use Cases.
- Tests für aktuelles Verhalten ergänzen, bevor Daten verschoben werden.

Akzeptanzkriterium:

- Es gibt eine dokumentierte Namensregel.
- Neue Klassen folgen der Regel.
- Bestehende Tests bleiben grün.

### Phase 2: JSON-Katalog für Workouts einführen

- JSON-Struktur für Exercises und Workouts definieren.
- Inhalte aus `DefinitionFactory` nach `wwwroot/data/workouts/catalog.de.json` migrieren.
- `JsonWorkoutCatalogRepository` implementieren.
- Validierung mit sprechenden Fehlermeldungen ergänzen.
- `ExerciseProvider` und `WorkoutProvider` zunächst intern auf den JSON-Katalog umstellen, ohne Public API sofort zu ändern.

Akzeptanzkriterium:

- Alle bisherigen Workouts und Exercises kommen aus JSON.
- `DefinitionFactory` wird im normalen DI-Pfad nicht mehr verwendet.
- Fehlende Exercise-Referenzen werden durch Tests erkannt.

### Phase 3: Factory-Methoden bereinigen

- `WorkoutFactory.Create(...)` als zentrale Stelle für `Workout`-Erstellung nutzen.
- Doppelte Konstruktorlogik in `Workout` reduzieren, z. B. über `WorkoutBuildOptions`:

```csharp
public sealed record WorkoutBuildOptions(int Sets = 1, int Rounds = 1);
```

- `Workout` bekommt idealerweise bereits aufgelöste `Exercise`-Instanzen oder eine explizite Build-Sequenz, statt selbst über `IExerciseProvider` zu suchen.
- `ExerciseFactory` baut `Exercise` aus `ExerciseDefinition`.

Akzeptanzkriterium:

- Factories lesen keine Dateien.
- Domain-Objekte hängen nicht mehr direkt an Katalog-/Provider-Abfragen.
- Sets/Rounds-Verhalten ist durch Tests abgedeckt.

### Phase 4: Provider zu Catalog/Service trennen

- `ExerciseProvider` in `ExerciseCatalog` überführen.
- `WorkoutProvider` aufteilen:
  - `WorkoutCatalog` für Previews, IDs, Varianten und Definitionen.
  - `WorkoutService` für aktives Workout, Auswahl, Startoptionen.
- Bestehende Interfaces schrittweise als Adapter erhalten, damit UI-Code nicht auf einmal umgebaut werden muss.

Akzeptanzkriterium:

- Read-only Katalogzugriff und aktive Workout-Session sind getrennt.
- DI-Registrierung ist nach Lebensdauer nachvollziehbar: Kataloge singleton, UI-/Session-State scoped.

### Phase 5: ViewModels einführen

- Für komplexe Seiten eigene ViewModels einführen, z. B.:
  - `WorkoutSelectionViewModel`
  - `ActiveWorkoutViewModel`
  - `TrainingPlanPageViewModel`
- Blazor-Komponenten konsumieren ViewModels statt direkt mehrere Provider/Services.
- ViewModels enthalten UI-State, Commands und Fehlerzustände, aber keine JSON-Ladelogik.

Akzeptanzkriterium:

- Pages/Components enthalten weniger Orchestrierungscode.
- ViewModels sind ohne Blazor Renderer unit-testbar.

### Phase 6: Running-Modul angleichen

- `RunningSessionFactory.Load(filePath)` aufteilen in:
  - `JsonRunningSessionRepository.Load(...)`
  - `RunningSessionFactory.Create(...)`
- `TrainingPlanProvider` in `JsonTrainingPlanRepository` oder `TrainingPlanCatalog` umbenennen.
- Fehlerbehandlung vereinheitlichen: nicht still schlucken, sondern optional sammeln und melden.

Akzeptanzkriterium:

- Running- und Workout-Modul verwenden dieselben Architekturbegriffe.
- JSON-Parsing, Validierung und Runtime-Erzeugung sind getrennt.

## Empfohlene DI-Struktur

```csharp
builder.Services.Configure<WorkoutCatalogOptions>(
    builder.Configuration.GetSection("WorkoutCatalog"));

builder.Services.AddSingleton<IWorkoutCatalogRepository, JsonWorkoutCatalogRepository>();
builder.Services.AddSingleton<IExerciseFactory, ExerciseFactory>();
builder.Services.AddSingleton<IWorkoutFactory, WorkoutFactory>();
builder.Services.AddSingleton<IExerciseCatalog, ExerciseCatalog>();
builder.Services.AddSingleton<IWorkoutCatalog, WorkoutCatalog>();
builder.Services.AddScoped<IWorkoutService, WorkoutService>();
```

Kataloge können Singleton sein, wenn sie immutable geladen werden. Aktives Workout und UI-Zustand müssen scoped sein, weil Blazor Server pro Benutzerkreis eigenen Zustand braucht.

## Testplan

- Repository-Tests mit gültigem JSON.
- Repository-Tests mit ungültigem JSON:
  - doppelte IDs
  - fehlende Exercise-Referenz
  - unbekannter Enum-Wert
  - negative Zeiten
- Factory-Tests:
  - Workout-Sequenz mit Preparation, Exercise und Rest
  - keine Rest-Phase nach letzter Exercise
  - Sets/Rounds erzeugen erwartete Reihenfolge
- Catalog-Tests:
  - Lookup per ID
  - Gruppierung per `baseId`
  - Preview-Level werden korrekt aggregiert
- Service-Tests:
  - aktives Workout ist initial `null`
  - Auswahl unbekannter ID wirft sprechende Exception
  - Auswahl gültiger ID erzeugt aktives Workout

## Risiken und Gegenmaßnahmen

- **Risiko:** JSON-Migration verändert IDs und bricht bestehende UI-Links.
  **Gegenmaßnahme:** Legacy-ID-Mapping für eine Übergangsphase.

- **Risiko:** Lokalisierte Texte in JSON konkurrieren mit `.resx`.
  **Gegenmaßnahme:** Entscheidung treffen: Katalogtexte im JSON pro Kultur oder technische Definition im JSON plus UI-Texte in `.resx`.

- **Risiko:** Singleton-Katalog enthält mutable Runtime-Objekte.
  **Gegenmaßnahme:** Kataloge speichern nur Definitionen oder immutable Previews; Runtime-Objekte entstehen pro Anfrage/Workout.

- **Risiko:** Zu großer Big-Bang-Umbau.
  **Gegenmaßnahme:** Adapter-Interfaces behalten und intern schrittweise auf neue Klassen routen.

## Priorisierte Umsetzung

1. JSON-Katalog und Repository für Workouts/Exercises einführen.
2. `DefinitionFactory` aus dem DI-Pfad entfernen.
3. `WorkoutFactory` zu echter Runtime-Factory machen.
4. `WorkoutProvider` in `WorkoutCatalog` und `WorkoutService` trennen.
5. ViewModels für Workout-Auswahl und aktive Workout-Ausführung einführen.
6. Running-Modul auf dieselben Begriffe und Schichten angleichen.

## Ergebnis

Nach der Optimierung ist der Datenfluss eindeutig:

```text
JSON-Dokument
  -> JsonRepository
  -> validierte Definitionen
  -> Catalog
  -> Service
  -> Factory
  -> Runtime-Domain-Objekte
  -> ViewModel
  -> Blazor UI
```

Damit werden neue Workouts und Exercises über JSON pflegbar, Factory-Methoden verlieren ihre Rolle als Datenspeicher, und Services, Provider, ViewModels und Factories haben klar unterscheidbare Verantwortlichkeiten.
