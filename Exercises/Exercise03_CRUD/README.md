# Exercise 03 - Operazioni CRUD

## Obiettivo

Implementare operazioni Create, Read, Update e Delete applicando le best practices.

## Task

### 1. CREATE
- Inserire multiple categorie con `AddRange()`
- Inserire multiple prodotti associati alle categorie
- **Best Practice**: Chiamare `SaveChanges()` una sola volta dopo tutti gli inserimenti

### 2. READ
- **Find()**: Cercare un prodotto per ID
- **Where() + AsNoTracking()**: Filtrare prodotti per prezzo
- **Include()**: Caricare prodotti con le categorie correlate (Eager Loading)
- **Select()**: Proiettare solo i campi necessari (Id, Name, Price)

Confrontare l'approccio **con** e **senza** `Include()` per osservare il problema N+1.

### 3. UPDATE
- **Update con tracking**: Modificare un'entità già caricata nel context
- **Update esplicito**: Usare `context.Update()` per aggiornare tutte le colonne

### 4. DELETE
- Rimuovere un prodotto con `Remove()`

## Concetti Chiave

- `AddRange()` per inserimenti batch
- `SaveChanges()` fuori dai loop
- `AsNoTracking()` per query read-only
- `Include()` per Eager Loading ed evitare N+1
- `Select()` per proiezioni efficienti
- Differenza tra tracked update e explicit update

## Esecuzione

```bash
dotnet run
```

Output atteso: visualizzazione delle operazioni CRUD con log delle query SQL.
