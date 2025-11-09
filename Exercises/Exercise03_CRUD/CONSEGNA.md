# Esercizio 03 - Operazioni CRUD

## Obiettivo
Implementare tutte le operazioni CRUD (Create, Read, Update, Delete) usando Entity Framework, applicando le best practices per le performance.

## Pre-requisiti
Parti dal modello dell'esercizio 02 (Category e Product) o ricrealo se necessario.

## Requisiti

### 1. CREATE - Inserire dati

#### Task 1.1: Inserimento singolo
- Crea una nuova categoria "Elettronica" con descrizione
- Salva nel database
- Stampa l'Id generato

#### Task 1.2: Inserimento multiplo (BEST PRACTICE)
- Crea 5 prodotti diversi nella categoria "Elettronica"
- **IMPORTANTE**: Usa `AddRange()` e chiama `SaveChanges()` UNA SOLA VOLTA fuori dal loop
- Stampa gli Id generati

### 2. READ - Leggere dati

#### Task 2.1: Recupero con Find()
- Usa `Find()` per recuperare un prodotto per Id
- Stampa nome e prezzo

#### Task 2.2: Query con Where()
- Trova tutti i prodotti con prezzo > 100
- Usa `AsNoTracking()` perché è una query read-only
- Stampa i risultati

#### Task 2.3: Query con Include() (Eager Loading)
- Recupera tutti i prodotti includendo la categoria
- Stampa per ogni prodotto: nome, prezzo e nome categoria
- **IMPORTANTE**: Usa `Include()` per evitare il problema N+1!

#### Task 2.4: Proiezione con Select()
- Recupera solo Id, Name e Price dei prodotti (senza caricare tutto)
- Usa `Select()` per proiettare solo i campi necessari
- Stampa i risultati

### 3. UPDATE - Aggiornare dati

#### Task 3.1: Update con Tracking
- Carica un prodotto usando il tracking
- Modifica il prezzo e lo stock
- Salva con `SaveChanges()`
- EF genererà UPDATE solo per le colonne modificate

#### Task 3.2: Update Esplicito
- Crea un oggetto Product con tutti i campi
- Usa `Update()` esplicito
- Osserva che EF aggiorna TUTTE le colonne

### 4. DELETE - Eliminare dati

#### Task 4.1: Delete con Tracking
- Carica un prodotto
- Usa `Remove()` per eliminarlo
- Salva con `SaveChanges()`

## Esempio Output Atteso
```
=== CREATE ===
Categoria creata con Id: 1

Prodotti creati:
- Id: 1, Nome: Laptop
- Id: 2, Nome: Mouse
- Id: 3, Nome: Tastiera
- Id: 4, Nome: Monitor
- Id: 5, Nome: Webcam

=== READ ===
Prodotto trovato: Laptop - €999.99

Prodotti con prezzo > 100:
- Laptop: €999.99
- Monitor: €299.99

Prodotti con categoria:
- Laptop (Elettronica): €999.99
- Mouse (Elettronica): €25.99
...

Proiezione (solo Id, Name, Price):
- 1: Laptop - €999.99
- 2: Mouse - €25.99
...

=== UPDATE ===
Prodotto aggiornato: nuovo prezzo €899.99, stock 50

=== DELETE ===
Prodotto eliminato con successo.
```

## Anti-Pattern da EVITARE

### ❌ SaveChanges() nel Loop
```csharp
// MAL for (int i = 0; i < 100; i++)
{
    context.Products.Add(new Product { ... });
    context.SaveChanges(); // 100 transazioni!
}
```

### ✅ SaveChanges() Fuori dal Loop
```csharp
// BENE
for (int i = 0; i < 100; i++)
{
    context.Products.Add(new Product { ... });
}
context.SaveChanges(); // 1 sola transazione!
```

### ❌ N+1 Query Problem
```csharp
// MALE
var products = context.Products.ToList();
foreach(var p in products)
{
    Console.WriteLine(p.Category.Name); // Query separata per ogni categoria!
}
```

### ✅ Eager Loading con Include()
```csharp
// BENE
var products = context.Products
    .Include(p => p.Category)
    .ToList();
foreach(var p in products)
{
    Console.WriteLine(p.Category.Name); // Dati già caricati!
}
```

## Concetti Chiave
- **AddRange()**: Aggiunge multiple entità in batch
- **AsNoTracking()**: Disabilita il tracking per query read-only (migliori performance)
- **Include()**: Eager loading per evitare N+1 query
- **Select()**: Proiezione per caricare solo i campi necessari
- **Find()**: Cerca prima nel Change Tracker, poi nel DB

## Note
- Usa sempre `AsNoTracking()` per le query read-only
- Evita `SaveChanges()` dentro i loop
- Usa `Include()` quando devi accedere alle navigation properties
- Usa `Select()` quando servono solo alcuni campi
