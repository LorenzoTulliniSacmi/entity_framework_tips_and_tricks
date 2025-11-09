# Esercizio 04 - Query Optimization e N+1 Problem

## Obiettivo
Identificare e risolvere il problema N+1, uno degli anti-pattern più comuni e pericolosi quando si lavora con un ORM.

## Scenario
Hai un database con Ordini (Orders) e Clienti (Customers). Devi visualizzare una lista di ordini con i nomi dei clienti, ma il codice attuale ha gravi problemi di performance.

## Pre-requisiti
Questo esercizio include un database pre-popolato con:
- 100 clienti
- 1000 ordini

## Parte 1: Identificare il Problema

### Task 1.1: Eseguire il codice problematico
Nella solution troverai un metodo `DisplayOrdersBad()` che ha il problema N+1.
Esegui il codice e osserva:
- Quante query SQL vengono eseguite?
- Quanto tempo impiega?

### Task 1.2: Abilitare il logging delle query
Aggiungi il logging per vedere le query SQL generate:
```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder
        .UseSqlite("Data Source=orders.db")
        .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
}
```

Riesegui e conta quante query vengono generate.

## Parte 2: Risolvere con Eager Loading

### Task 2.1: Implementare DisplayOrdersGood()
Crea un nuovo metodo che risolve il problema usando `Include()`:
- Usa `.Include(o => o.Customer)` per caricare i clienti in un'unica query
- Verifica che venga generata una sola query con JOIN
- Misura il tempo di esecuzione e confrontalo con la versione precedente

## Parte 3: Ottimizzazione ulteriore

### Task 3.1: Usare Select() per proiezione
Se non ti servono tutti i dati, usa `Select()`:
- Crea un metodo `DisplayOrdersOptimized()`
- Usa `Select()` per proiettare solo i campi necessari (OrderId, OrderDate, CustomerName)
- Usa `AsNoTracking()` perché è read-only
- Confronta le performance

### Task 3.2: Paginazione
Implementa la paginazione per caricare solo 10 ordini alla volta:
- Usa `Skip()` e `Take()`
- Mantieni `Include()` per evitare N+1
- Stampa il numero di pagina e i risultati

## Esempio Output Atteso

### Output CATTIVO (N+1 Problem):
```
=== Versione CATTIVA (N+1 Problem) ===
Executing: SELECT * FROM Orders
Executing: SELECT * FROM Customers WHERE Id = 1
Executing: SELECT * FROM Customers WHERE Id = 2
Executing: SELECT * FROM Customers WHERE Id = 3
... (1000 query totali!)

Tempo impiegato: 2500ms
Query eseguite: 1001
```

### Output BUONO (Con Include):
```
=== Versione BUONA (Include) ===
Executing: SELECT o.*, c.*
           FROM Orders o
           LEFT JOIN Customers c ON o.CustomerId = c.Id

Tempo impiegato: 150ms
Query eseguite: 1
Miglioramento: 16x più veloce!
```

### Output OTTIMIZZATO (Con Select):
```
=== Versione OTTIMIZZATA (Select + AsNoTracking) ===
Executing: SELECT o.Id, o.OrderDate, c.Name
           FROM Orders o
           LEFT JOIN Customers c ON o.CustomerId = c.Id

Tempo impiegato: 80ms
Query eseguite: 1
Miglioramento: 31x più veloce!
```

## Modello Dati

### Customer
```csharp
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public ICollection<Order> Orders { get; set; }
}
```

### Order
```csharp
public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; }
}
```

## Concetti Chiave

### Il Problema N+1
Se hai N record e accedi a una navigation property senza Include(), EF esegue:
- 1 query per caricare gli N record principali
- N query separate per caricare i dati correlati
- Totale: N+1 query!

### Eager Loading con Include()
```csharp
// ✅ Genera 1 query con JOIN
var orders = context.Orders
    .Include(o => o.Customer)
    .ToList();
```

### Lazy Loading (pericoloso!)
```csharp
// ❌ Genera N+1 query
var orders = context.Orders.ToList();
foreach(var order in orders)
{
    Console.WriteLine(order.Customer.Name); // Query separata!
}
```

### Proiezione con Select()
```csharp
// ✅ Carica solo i dati necessari
var orders = context.Orders
    .Select(o => new
    {
        o.Id,
        o.OrderDate,
        CustomerName = o.Customer.Name
    })
    .AsNoTracking()
    .ToList();
```

## Domande di Riflessione
1. Perché il problema N+1 è così pericoloso in produzione?
2. Quando è meglio usare `Include()` vs `Select()`?
3. Perché `AsNoTracking()` migliora le performance per query read-only?
4. Come puoi identificare il problema N+1 durante lo sviluppo?

## Challenge Bonus
Prova a implementare:
1. Un metodo che calcola statistiche aggregate (es. totale ordini per cliente) usando `GroupBy()`
2. Un filtro che trova ordini con importo > 500 del cliente "Mario Rossi" in una singola query
3. Una paginazione completa con ordinamento configurabile
