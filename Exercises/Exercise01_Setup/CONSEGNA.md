# Esercizio 01 - Setup Iniziale

## Obiettivo
Creare un DbContext e collegarlo al database SQLite, testando che la connessione funzioni correttamente.

## Requisiti

### 1. Creare il DbContext
- Crea una classe `AppDbContext` che eredita da `DbContext`
- Aggiungi un costruttore che accetta `DbContextOptions<AppDbContext>`
- Per ora non aggiungere ancora DbSet, li vedremo nel prossimo esercizio

### 2. Configurare la connessione
- Nel file `Program.cs`, configura il DbContext per usare SQLite
- Usa come connection string: `"Data Source=app.db"`
- Ricorda di usare `DbContextOptionsBuilder` per configurare SQLite

### 3. Testare la connessione
- Istanzia il DbContext
- Usa il metodo `Database.EnsureCreated()` per creare il database
- Usa il metodo `Database.CanConnect()` per verificare che la connessione funzioni
- Stampa un messaggio di conferma

## Esempio Output Atteso
```
Database creato con successo!
Connessione al database: OK
```

## Note
- SQLite crea automaticamente il file database se non esiste
- Il file `app.db` verrà creato nella directory del progetto
- `EnsureCreated()` è utile per testing, ma nelle applicazioni reali useremo le Migration (prossimi esercizi)

## Suggerimenti
Se hai problemi, controlla:
1. Di aver installato i pacchetti NuGet corretti
2. Che il connection string sia corretto
3. Che il DbContext sia configurato correttamente nel Program.cs
