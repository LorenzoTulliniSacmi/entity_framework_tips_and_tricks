# Esercizio 02 - Code First e Migrations

## Obiettivo
Definire entità usando l'approccio Code First e generare la prima migration per creare il database.

## Scenario
Devi creare un sistema per gestire un catalogo prodotti con le seguenti entità:
- **Category**: Categoria di prodotti
- **Product**: Singolo prodotto

## Requisiti

### 1. Definire le Entità

#### Entità Category
Crea la classe `Category` con le seguenti proprietà:
- `Id` (int) - chiave primaria
- `Name` (string, max 100 caratteri, obbligatorio)
- `Description` (string, nullable)
- `Products` (ICollection<Product>) - relazione 1:m con Product

#### Entità Product
Crea la classe `Product` con le seguenti proprietà:
- `Id` (int) - chiave primaria
- `Name` (string, max 200 caratteri, obbligatorio)
- `Description` (string, nullable)
- `Price` (decimal, con precisione 18,2)
- `Stock` (int)
- `CreatedAt` (DateTime)
- `CategoryId` (int) - foreign key
- `Category` (Category) - navigation property

### 2. Creare il DbContext
- Crea la classe `AppDbContext` che eredita da `DbContext`
- Aggiungi i DbSet per `Category` e `Product`
- Nel metodo `OnModelCreating`, configura usando Fluent API:
  - Nome tabella per Category: "Categories"
  - Nome tabella per Product: "Products"
  - Lunghezza massima per i campi Name
  - Tipo decimale per Price
  - Relazione 1:m tra Category e Product

### 3. Generare la Migration
Usa i comandi EF Core per:
1. Creare la prima migration chiamata `InitialCreate`
2. Applicare la migration al database

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 4. Verificare
Nel `Program.cs`, testa che:
- Il database sia stato creato
- Le tabelle esistano
- La connessione funzioni

## Esempio Output Atteso
```
Migration applicata con successo!
Database creato: OK
Tabelle Categories e Products create correttamente.
```

## Concetti Chiave
- **Code First**: Il modello dati è definito nel codice, EF genera il database
- **Navigation Properties**: Le proprietà `Category` e `Products` permettono di navigare le relazioni
- **Fluent API**: Configurazione esplicita del modello tramite `OnModelCreating`
- **Migrations**: File C# che rappresentano l'evoluzione dello schema database

## Note
- Ricorda: ogni modifica al modello richiede una nuova migration!
- Il file `ModelSnapshot.cs` rappresenta lo stato attuale del modello
- La tabella `__EFMigrationsHistory` traccia quali migration sono state applicate
