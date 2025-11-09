# Entity Framework - Esercizi Lab

Questa cartella contiene gli esercizi pratici per il corso Entity Framework.

## Struttura

Ogni esercizio è un progetto .NET separato con:
- **README.md** - Consegna dettagliata dell'esercizio
- **Progetto configurato** - Con i pacchetti NuGet necessari già installati
- **Template minimo** - Struttura base per iniziare a lavorare

## Esercizi Disponibili

### Exercise01_Setup
**Obiettivo**: Creare un DbContext e collegarlo al database
**Concetti**: DbContext, connection string, verifica connessione

### Exercise02_CodeFirst
**Obiettivo**: Definire entità e generare migration
**Concetti**: Code First, entità, Fluent API, migration, `DbSet`

### Exercise03_CRUD
**Obiettivo**: Implementare operazioni CRUD con best practices
**Concetti**: Create, Read, Update, Delete, `AsNoTracking()`, `Include()`, `Select()`

### Exercise04_QueryOptimization
**Obiettivo**: Identificare e risolvere il problema N+1
**Concetti**: N+1 Problem, Eager Loading, Lazy Loading, ottimizzazione query

## Setup Database PostgreSQL con Docker

Gli esercizi sono configurati per usare PostgreSQL tramite Docker. Se non hai Docker, puoi usare SQLite (vedi sotto).

### Avviare PostgreSQL con Docker

Dalla root del progetto:

```bash
# Avvia PostgreSQL
docker-compose up -d

# Verifica che sia in esecuzione
docker ps

# Ferma PostgreSQL quando hai finito
docker-compose down
```

Il container PostgreSQL sarà disponibile su:
- **Host**: localhost
- **Port**: 5432
- **Username**: efuser
- **Password**: efpass
- **Database**: ef_lab

### Alternativa: Usare SQLite senza Docker

Se non hai Docker:

1. Apri il file `Program.cs` del progetto
2. Commenta la riga con `UseNpgsql`
3. Decommenta la riga con `UseSqlite`

```csharp
// Commenta PostgreSQL
// optionsBuilder.UseNpgsql("Host=localhost;Database=...");

// Decommenta SQLite
optionsBuilder.UseSqlite("Data Source=app.db");
```

## Come Iniziare

1. **Avvia PostgreSQL** con `docker-compose up -d` (oppure configura SQLite)
2. Apri il progetto dell'esercizio che vuoi svolgere
3. Leggi il README.md nella cartella del progetto
4. Completa i task richiesti
5. Confronta la tua soluzione con quella nella cartella `Solutions/`

## Prerequisiti

- .NET 8.0 SDK
- Editor di codice (VS Code, Visual Studio, Rider)
- Entity Framework Core Tools (già incluso nei progetti)
- **Docker** (per PostgreSQL) - oppure usa SQLite come alternativa

## Comandi Utili

```bash
# Compilare il progetto
dotnet build

# Eseguire il progetto
dotnet run

# Aggiungere una migration (Exercise02+)
dotnet ef migrations add NomeMigration

# Applicare migration al database
dotnet ef database update

# Rimuovere l'ultima migration
dotnet ef migrations remove
```

## Suggerimenti

- Leggi sempre il README prima di iniziare
- Testa il codice eseguendo `dotnet run`
- Se ti blocchi, consulta la soluzione corrispondente in `Solutions/`
- Sperimenta modificando il codice e osservando i risultati
- Usa il logging delle query per capire cosa fa Entity Framework

## Risorse

- [Documentazione EF Core](https://docs.microsoft.com/en-us/ef/core/)
- [EF Core GitHub](https://github.com/dotnet/efcore)
- Dispensa del corso: `../dispensa.md`
