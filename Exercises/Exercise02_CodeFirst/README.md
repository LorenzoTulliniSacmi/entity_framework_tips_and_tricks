# Exercise 02 - Code First e Migrations

## Obiettivo

Definire entità POCO, configurarle con Fluent API e generare migration.

## Task

1. **Creare le entità** in `Models/`
   - `Category`: Id, Name, Description
   - `Product`: Id, Name, Description, Price, Stock, CategoryId, CreatedAt
   - Configurare navigation properties (1:m)

2. **Configurare il `DbContext`**
   - Aggiungere `DbSet<Category>` e `DbSet<Product>`
   - Implementare `OnModelCreating()` con Fluent API:
     - Configurare chiavi primarie
     - Configurare la relazione 1:m tra Category e Product
     - Impostare lunghezze massime per stringhe
     - Configurare precisione per `decimal` (es. `Price`)

3. **Generare e applicare migration**
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

## Concetti Chiave

- Code First approach
- Entità POCO (Plain Old CLR Objects)
- Fluent API vs Data Annotations
- Navigation properties per relazioni
- Migration per evoluzione schema database

## Esecuzione

```bash
dotnet run
```

Output atteso: conferma della creazione delle tabelle.
