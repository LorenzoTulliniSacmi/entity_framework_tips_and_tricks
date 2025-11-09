# Exercise 01 - Setup e Connessione Database

## Obiettivo

Configurare un `DbContext` funzionante e verificare la connessione al database.

## Task

1. **Creare la classe `AppDbContext`** che eredita da `DbContext`
   - Implementare il costruttore con `DbContextOptions<AppDbContext>`

2. **Configurare la connessione al database** in `Program.cs`
   - Usare `DbContextOptionsBuilder` per configurare il provider
   - PostgreSQL: `UseNpgsql("Host=localhost;Database=ef_lab_setup;Username=efuser;Password=efpass")`
   - Alternativa SQLite: `UseSqlite("Data Source=app.db")`

3. **Testare la connessione**
   - Applicare le migration con `context.Database.Migrate()`
   - Verificare con `context.Database.CanConnect()`
   - Stampare il risultato

## Concetti Chiave

- `DbContext` come punto di accesso al database
- `DbContextOptions` per la configurazione
- Connection string e provider (Npgsql, SQLite)
- Metodi `Migrate()` e `CanConnect()`

## Esecuzione

```bash
dotnet run
```

Output atteso: conferma della connessione al database.
