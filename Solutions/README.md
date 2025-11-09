# Entity Framework - Soluzioni Lab

Questa cartella contiene le soluzioni complete degli esercizi.

## 🐳 Setup Database PostgreSQL con Docker

Le soluzioni usano PostgreSQL tramite Docker. **Prima di eseguire qualsiasi soluzione**, avvia il database:

```bash
# Dalla root del progetto
docker-compose up -d

# Verifica che PostgreSQL sia in esecuzione
docker ps
```

**Credenziali PostgreSQL:**
- Host: `localhost`
- Port: `5432`
- Username: `efuser`
- Password: `efpass`
- Databases: `ef_lab_setup`, `ef_lab_codefirst`, `ef_lab_crud`, `ef_lab_orders`

### Alternativa SQLite (senza Docker)

Se non hai Docker installato:

1. Apri il file `Program.cs` della soluzione
2. Commenta la riga con `UseNpgsql()`
3. Decommenta la riga con `UseSqlite()`

```csharp
// Commenta PostgreSQL
// optionsBuilder.UseNpgsql("Host=localhost;Database=...");

// Decommenta SQLite
optionsBuilder.UseSqlite("Data Source=app.db");
```

### Fermare PostgreSQL

Quando hai finito:
```bash
docker-compose down
```

## Esercizi Risolti

### Exercise01_Setup
**Soluzione**: DbContext configurato con SQLite, test di connessione funzionante

**Concetti chiave**:
- Configurazione `DbContextOptionsBuilder`
- Connection string per SQLite
- Metodi `EnsureCreated()` e `CanConnect()`

### Exercise02_CodeFirst
**Soluzione**: Modello dati completo con Category e Product, configurazione Fluent API

**Concetti chiave**:
- Definizione entità con navigation properties
- Configurazione relazioni 1:m
- Fluent API nel metodo `OnModelCreating`
- Configurazione lunghezza campi e tipi decimal

### Exercise03_CRUD
**Soluzione**: Implementazione completa di tutte le operazioni CRUD con best practices

**Highlights**:
- ✅ `AddRange()` + `SaveChanges()` fuori dal loop
- ✅ `AsNoTracking()` per query read-only
- ✅ `Include()` per evitare N+1 Problem
- ✅ `Select()` per proiezioni ottimizzate
- ✅ Update con tracking vs update esplicito

### Exercise04_QueryOptimization
**Soluzione**: Dimostrazione completa del problema N+1 e tutte le soluzioni

**Dimostrazioni**:
- ❌ Versione problematica (N+1 queries)
- ✅ Soluzione con `Include()` (Eager Loading)
- ✅ Soluzione ottimizzata con `Select()` + `AsNoTracking()`
- ✅ Paginazione corretta con `Skip()` e `Take()`
- 📊 Confronto performance con logging query

## Come Usare le Soluzioni

### 1. Dopo aver completato l'esercizio
```bash
# Esegui la tua soluzione
cd Exercises/Exercise01_Setup
dotnet run

# Confronta con la soluzione
cd ../../Solutions/Exercise01_Setup
dotnet run
```

### 2. Se sei bloccato
1. Rileggi la consegna nell'esercizio
2. Prova a cercare online o nella documentazione
3. Guarda solo la parte specifica della soluzione che ti serve
4. Torna al tuo codice e prova a implementare

### 3. Studio delle best practices
- Leggi i commenti nel codice
- Nota i pattern utilizzati (✅ vs ❌)
- Esegui le soluzioni e osserva l'output
- Sperimenta modificando il codice

## Eseguire le Soluzioni

```bash
# Naviga nella cartella della soluzione
cd Solutions/Exercise03_CRUD

# Esegui
dotnet run
```

## Note Tecniche

### Database Generati
Ogni soluzione crea il proprio database SQLite:
- `Exercise01_Setup`: `app.db`
- `Exercise02_CodeFirst`: `catalog.db`
- `Exercise03_CRUD`: `crud_demo.db`
- `Exercise04_QueryOptimization`: `orders.db`

I database vengono creati automaticamente all'esecuzione.

### Logging delle Query
Exercise04 include il logging delle query SQL per mostrare esattamente cosa fa Entity Framework.

### Migration
Exercise02 è configurato per le migration. Puoi generarle con:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Differenze tra Esercizi e Soluzioni

| Aspetto | Exercises | Solutions |
|---------|-----------|-----------|
| Completezza | Template minimo | Codice completo |
| Commenti | TODO e suggerimenti | Spiegazioni dettagliate |
| Best Practices | Da implementare | Già applicate |
| Output | Da generare | Esempi funzionanti |

## Suggerimenti per lo Studio

1. **Non copiare**: Prova prima da solo
2. **Sperimenta**: Modifica le soluzioni per capire meglio
3. **Confronta**: Nota le differenze tra il tuo approccio e la soluzione
4. **Pratica**: Ripeti gli esercizi per memorizzare i pattern

## Best Practices Evidenziate

- ✅ `SaveChanges()` fuori dai loop
- ✅ `AsNoTracking()` per query read-only
- ✅ `Include()` per evitare N+1
- ✅ `Select()` per caricare solo dati necessari
- ✅ `AddRange()` per inserimenti multipli
- ✅ Configurazione Fluent API separata dalle entità
- ✅ Navigation properties per relazioni

## Prossimi Passi

Dopo aver completato tutti gli esercizi:
1. Rileggi la dispensa per consolidare i concetti
2. Prova a creare un progetto personale con EF Core
3. Applica questi pattern nei tuoi progetti reali
4. Esplora funzionalità avanzate di EF Core

Buono studio! 🚀
