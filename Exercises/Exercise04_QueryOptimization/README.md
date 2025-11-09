# Exercise 04 - Query Optimization e N+1 Problem

## Obiettivo

Identificare il problema N+1 con Lazy Loading e implementare soluzioni ottimizzate.

## Task

### 1. Setup Database
- Generare dati di test: 100 clienti e 1000 ordini
- Ogni ordine è associato a un cliente

### 2. Dimostrazione Problema N+1
Implementare la query **senza** `Include()`:
```csharp
var orders = context.Orders.Take(10).ToList();
foreach (var order in orders)
{
    Console.WriteLine($"Cliente: {order.Customer.Name}");
}
```
Osservare nel log: **11 query** (1 per ordini + 10 per clienti)

### 3. Soluzione con Include()
Implementare la stessa query **con** Eager Loading:
```csharp
var orders = context.Orders
    .Include(o => o.Customer)
    .Take(10)
    .ToList();
```
Osservare nel log: **1 query** con JOIN

### 4. Ottimizzazione con Select()
Implementare una proiezione per caricare solo i campi necessari:
```csharp
var orders = context.Orders
    .Select(o => new { o.Id, o.TotalAmount, CustomerName = o.Customer.Name })
    .AsNoTracking()
    .Take(10)
    .ToList();
```

### 5. Paginazione
Implementare paginazione efficiente con `Skip()`, `Take()`, `Include()` e `AsNoTracking()`.

### 6. Confronto Performance
Misurare e confrontare:
- Numero di query eseguite
- Memoria utilizzata
- Entità tracciate dal ChangeTracker

## Concetti Chiave

- **N+1 Problem**: 1 query principale + N query per relazioni
- **Lazy Loading**: Caricamento automatico al primo accesso (inefficiente)
- **Eager Loading**: `Include()` per caricare tutto in una query
- **Proiezioni**: `Select()` per ottimizzare memoria e performance
- **AsNoTracking()**: Disabilitare tracking per query read-only
- Paginazione efficiente con `Skip()` e `Take()`

## Esecuzione

```bash
dotnet run
```

Output atteso: comparazione dettagliata delle diverse strategie con log SQL e metriche di performance.
