# Entity Framework Core 8 - Convenzioni Complete

## Indice
- [Primary Keys](#primary-keys)
- [Tabelle e Schema](#tabelle-e-schema)
- [Colonne e Proprietà](#colonne-e-proprietà)
- [Foreign Keys](#foreign-keys)
- [Relazioni](#relazioni)
- [Tipi di Dato](#tipi-di-dato)
- [Nullability](#nullability)
- [Indici](#indici)
- [Nomi Generati](#nomi-generati)
- [Delete Behavior](#delete-behavior)
- [Inheritance (Ereditarietà)](#inheritance-ereditarietà)
- [Shadow Properties](#shadow-properties)
- [Backing Fields](#backing-fields)
- [Conversioni e Comparatori](#conversioni-e-comparatori)
- [Concurrency](#concurrency)
- [Query Filters](#query-filters)
- [Altre Convenzioni](#altre-convenzioni)

---

## Primary Keys

### Rilevamento Automatico

| Convenzione | Comportamento | Esempio |
|-------------|---------------|---------|
| Proprietà chiamata `Id` | Diventa PK | `public int Id { get; set; }` |
| Proprietà chiamata `{ClassName}Id` | Diventa PK | `public int ProductId { get; set; }` in classe `Product` |
| Prima proprietà senza suffisso Id | NON diventa PK automaticamente | `public int Code { get; set; }` → serve `[Key]` |
| Composite PK | Non supportato per convenzione | Serve Fluent API: `HasKey(e => new { e.Id1, e.Id2 })` |

### Generazione Valori

| Tipo PK | Comportamento | Database |
|---------|---------------|----------|
| `int` | Identity/Auto-increment | `IDENTITY(1,1)` (SQL Server), `SERIAL` (PostgreSQL) |
| `long` | Identity/Auto-increment | `BIGINT IDENTITY` (SQL Server), `BIGSERIAL` (PostgreSQL) |
| `short` | Identity/Auto-increment | `SMALLINT IDENTITY` |
| `Guid` | Generato dal client | `Guid.NewGuid()` prima di `SaveChanges()` |
| `string` | Nessuna generazione | Deve essere impostato manualmente |
| `byte[]` | Nessuna generazione | Deve essere impostato manualmente |

### Strategie di Generazione

```csharp
// Identity (default per int/long)
public int Id { get; set; } 
// → ValueGeneratedOnAdd

// Guid (default)
public Guid Id { get; set; } 
// → ValueGeneratedOnAdd (client-side)

// Nessuna generazione
[DatabaseGenerated(DatabaseGeneratedOption.None)]
public int Id { get; set; }

// Generato da trigger/computed
[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
public int Id { get; set; }
```

---

## Tabelle e Schema

### Nomi Tabella

| Convenzione | Risultato | Esempio |
|-------------|-----------|---------|
| Nome `DbSet<T>` | Nome tabella | `DbSet<Product> Products` → tabella `Products` |
| Se non c'è `DbSet<T>` | Nome della classe (pluralizzato) | Classe `Order` → tabella `Orders` |
| Pluralizzazione | Regole inglesi | `Category` → `Categories`, `Person` → `People` |
| Disabilitare pluralizzazione | Non disponibile per convenzione | Serve `ToTable()` o configurare `IModelCachingConventionSetPlugin` |

### Schema

| Convenzione | Comportamento |
|-------------|---------------|
| Default | `dbo` (SQL Server), `public` (PostgreSQL), nessuno (SQLite) |
| Specificare schema | Serve `[Table("Products", Schema = "sales")]` o Fluent API |

### Table-Per-Hierarchy (TPH)

```csharp
// Classe base e derivate nella stessa tabella
public class Animal { public int Id { get; set; } }
public class Dog : Animal { public string Breed { get; set; } }
public class Cat : Animal { public int Lives { get; set; } }

// Convenzione: tutte in una tabella "Animals" con colonna "Discriminator"
```

| Elemento | Convenzione |
|----------|-------------|
| Tabella | Nome della classe base |
| Colonna discriminator | `Discriminator` (tipo `string`) |
| Valore discriminator | Nome della classe derivata |

---

## Colonne e Proprietà

### Inclusione/Esclusione

| Tipo Proprietà | Mappata? | Note |
|----------------|----------|------|
| Pubblica con `{ get; set; }` | ✅ Sì | Standard |
| Pubblica con `{ get; init; }` | ✅ Sì | C# 9+ init-only |
| Pubblica solo `{ get; }` | ❌ No | Considerata computed/navigation |
| Pubblica solo `{ set; }` | ❌ No | Non supportata |
| Privata | ❌ No | Non rilevata (serve Fluent API) |
| Proprietà statica | ❌ No | Mai mappata |
| Campo pubblico | ❌ No | Solo proprietà sono mappate |
| `[NotMapped]` | ❌ No | Esclusa esplicitamente |

### Eccezioni - Proprietà Rilevate Anche Senza Setter

```csharp
// Navigation collection: rilevata anche se readonly
public class Category {
    public ICollection<Product> Products { get; } = new List<Product>();
}

// Navigation reference con backing field
public class Product {
    private Category _category;
    public Category Category => _category;
}
```

### Nomi Colonna

| Convenzione | Risultato |
|-------------|-----------|
| Nome proprietà | Nome colonna identico |
| PascalCase → snake_case | Non automatico, serve configurazione provider |
| Proprietà `FirstName` | Colonna `FirstName` |

### Computed Properties

```csharp
public class Product {
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    
    // NON mappata (solo get)
    public decimal Total => Price * Quantity;
    
    // NON mappata (expression-bodied)
    public bool IsExpensive => Price > 1000;
}
```

---

## Foreign Keys

### Rilevamento Automatico

| Pattern | Rilevata come FK? | Note |
|---------|-------------------|------|
| `{NavigationProperty}Id` | ✅ Sì | `CategoryId` + `Category` |
| `{NavigationProperty}{PrincipalKey}` | ✅ Sì | `CategoryCategoryId` (se Category ha PK `CategoryId`) |
| `Id` + Navigation | ❌ No | `Id` è la PK, non FK |
| Solo Navigation (no Id property) | ✅ Sì (Shadow) | EF crea shadow property |
| Nome casuale + `[ForeignKey]` | ✅ Sì | Attributo esplicito |

### Esempi

```csharp
// 1. FK esplicita standard
public class Product {
    public int CategoryId { get; set; }  // ✅ FK
    public Category Category { get; set; }
}

// 2. FK con nome custom del PK
public class Category {
    public int CategoryCode { get; set; }  // PK custom
}
public class Product {
    public int CategoryCategoryCode { get; set; }  // ✅ FK rilevata
    public Category Category { get; set; }
}

// 3. Shadow FK (no property)
public class Product {
    // No CategoryId property
    public Category Category { get; set; }  // ✅ EF crea "CategoryId" shadow
}

// 4. FK multipli (serve configurazione)
public class Order {
    public int BillingAddressId { get; set; }
    public Address BillingAddress { get; set; }
    
    public int ShippingAddressId { get; set; }
    public Address ShippingAddress { get; set; }
}
// ⚠️ Serve Fluent API per disambiguare
```

### Nullability FK

| Tipo FK | Relazione | Delete Behavior |
|---------|-----------|----------------|
| `int CategoryId` | Required (obbligatoria) | `Cascade` |
| `int? CategoryId` | Optional | `ClientSetNull` |

---

## Relazioni

### One-to-Many (1:N)

```csharp
// Convenzione: collection + reference + FK
public class Category {
    public int Id { get; set; }
    public ICollection<Product> Products { get; set; }  // Collection side
}

public class Product {
    public int Id { get; set; }
    public int CategoryId { get; set; }      // FK
    public Category Category { get; set; }   // Reference side
}
```

**Rilevamento:**
- ✅ Collection (`ICollection<T>`) su un lato
- ✅ Reference su altro lato
- ✅ FK property (o shadow FK se assente)

**Varianti:**

| Variante | Comportamento |
|----------|---------------|
| Solo collection | ✅ Relazione 1:N con shadow FK |
| Solo reference + FK | ✅ Relazione 1:N unidirezionale |
| Collection + reference (no FK) | ✅ Shadow FK creata |

### One-to-One (1:1)

```csharp
// Convenzione: due reference + FK su uno dei due
public class User {
    public int Id { get; set; }
    public UserProfile Profile { get; set; }  // Principal
}

public class UserProfile {
    public int Id { get; set; }
    public int UserId { get; set; }  // FK (PK condivisa o separata)
    public User User { get; set; }   // Dependent
}
```

**Rilevamento:**
- ✅ Due reference (no collections)
- ⚠️ Serve Fluent API per specificare chi è dependent: `HasForeignKey<UserProfile>()`

**Strategie PK in 1:1:**

```csharp
// 1. PK condivisa (recommended)
public class UserProfile {
    public int UserId { get; set; }  // PK e FK insieme
    public User User { get; set; }
}
// Fluent: HasKey(p => p.UserId)

// 2. PK separata
public class UserProfile {
    public int Id { get; set; }      // PK separata
    public int UserId { get; set; }  // FK
    public User User { get; set; }
}
```

### Many-to-Many (N:M)

#### EF Core 5+ - Direct Navigation (Convenzione)

```csharp
public class Student {
    public int Id { get; set; }
    public ICollection<Course> Courses { get; set; }  // Direct navigation
}

public class Course {
    public int Id { get; set; }
    public ICollection<Student> Students { get; set; }
}
```

**Convenzione:**
- ✅ Due `ICollection<T>` che si riferiscono l'uno all'altro
- ✅ EF crea automaticamente tabella join: `CourseStudent`
- ✅ Nessuna classe join necessaria

**Tabella join generata:**
```sql
CREATE TABLE CourseStudent (
    CoursesId INT,
    StudentsId INT,
    PRIMARY KEY (CoursesId, StudentsId)
)
```

#### Tabella Join Esplicita

```csharp
// Quando serve dati extra nella relazione
public class Student {
    public ICollection<Enrollment> Enrollments { get; set; }
}

public class Course {
    public ICollection<Enrollment> Enrollments { get; set; }
}

public class Enrollment {
    public int StudentId { get; set; }
    public Student Student { get; set; }
    
    public int CourseId { get; set; }
    public Course Course { get; set; }
    
    public DateTime EnrolledDate { get; set; }  // Dati aggiuntivi
}
```

**Convenzione:**
- ✅ Composite PK: `HasKey(e => new { e.StudentId, e.CourseId })`
- ✅ Due relazioni 1:N

### Self-Referencing

```csharp
public class Employee {
    public int Id { get; set; }
    public int? ManagerId { get; set; }          // FK nullable
    public Employee Manager { get; set; }        // Reference
    public ICollection<Employee> Subordinates { get; set; }  // Collection
}
```

**Convenzione:**
- ✅ FK `ManagerId` rilevata automaticamente
- ✅ Relazione 1:N su stessa tabella

---

## Tipi di Dato

### Mapping C# → SQL

#### Tipi Numerici

| C# | SQL Server | PostgreSQL | MySQL | SQLite |
|----|-----------|------------|-------|--------|
| `byte` | `TINYINT` | `smallint` | `TINYINT UNSIGNED` | `INTEGER` |
| `sbyte` | `SMALLINT` | `smallint` | `TINYINT` | `INTEGER` |
| `short` | `SMALLINT` | `smallint` | `SMALLINT` | `INTEGER` |
| `ushort` | `INT` | `integer` | `SMALLINT UNSIGNED` | `INTEGER` |
| `int` | `INT` | `integer` | `INT` | `INTEGER` |
| `uint` | `BIGINT` | `bigint` | `INT UNSIGNED` | `INTEGER` |
| `long` | `BIGINT` | `bigint` | `BIGINT` | `INTEGER` |
| `ulong` | `DECIMAL(20,0)` | `numeric(20,0)` | `BIGINT UNSIGNED` | `INTEGER` |
| `float` | `REAL` | `real` | `FLOAT` | `REAL` |
| `double` | `FLOAT(53)` | `double precision` | `DOUBLE` | `REAL` |
| `decimal` | `DECIMAL(18,2)` | `numeric(18,2)` | `DECIMAL(18,2)` | `TEXT` |

#### Tipi Stringa

| C# | SQL Server | PostgreSQL | MySQL | SQLite |
|----|-----------|------------|-------|--------|
| `string` (senza `[MaxLength]`) | `NVARCHAR(MAX)` | `text` | `LONGTEXT` | `TEXT` |
| `string` + `[MaxLength(n)]` | `NVARCHAR(n)` | `varchar(n)` | `VARCHAR(n)` | `TEXT` |
| `char` | `NVARCHAR(1)` | `character(1)` | `VARCHAR(1)` | `TEXT` |

#### Tipi Temporali

| C# | SQL Server | PostgreSQL | MySQL | SQLite |
|----|-----------|------------|-------|--------|
| `DateTime` | `DATETIME2(7)` | `timestamp without time zone` | `DATETIME(6)` | `TEXT` (ISO 8601) |
| `DateTimeOffset` | `DATETIMEOFFSET(7)` | `timestamp with time zone` | `DATETIME(6)` | `TEXT` |
| `DateOnly` (.NET 6+) | `DATE` | `date` | `DATE` | `TEXT` |
| `TimeOnly` (.NET 6+) | `TIME` | `time` | `TIME(6)` | `TEXT` |
| `TimeSpan` | `TIME` | `interval` | `TIME(6)` | `TEXT` |

#### Altri Tipi

| C# | SQL Server | PostgreSQL | MySQL | SQLite |
|----|-----------|------------|-------|--------|
| `bool` | `BIT` | `boolean` | `TINYINT(1)` | `INTEGER` (0/1) |
| `Guid` | `UNIQUEIDENTIFIER` | `uuid` | `CHAR(36)` | `TEXT` |
| `byte[]` | `VARBINARY(MAX)` | `bytea` | `LONGBLOB` | `BLOB` |
| `enum` | `INT` | `integer` | `INT` | `INTEGER` |

### Enumerations

```csharp
public enum OrderStatus {
    Pending = 0,
    Confirmed = 1,
    Shipped = 2,
    Delivered = 3
}

public class Order {
    public OrderStatus Status { get; set; }  // → INT con valore numerico
}
```

**Convenzione:**
- ✅ Salvato come `INT` (valore numerico dell'enum)
- ❌ Non salvato come stringa (serve Value Converter)

```csharp
// Per salvare come stringa:
modelBuilder.Entity<Order>()
    .Property(e => e.Status)
    .HasConversion<string>();  // "Pending", "Confirmed", etc.
```

### Collezioni di Tipi Primitivi (EF Core 8)

```csharp
public class Product {
    public int Id { get; set; }
    public List<string> Tags { get; set; }  // ✅ Supportato in EF Core 8
}
```

**Convenzione EF Core 8:**
- ✅ `List<T>`, `IList<T>`, `T[]` per tipi primitivi
- PostgreSQL: `text[]` (array nativo)
- SQL Server: `NVARCHAR(MAX)` con JSON
- SQLite: `TEXT` con JSON

---

## Nullability

### Nullable Reference Types (C# 8+)

**Prerequisito:**
```xml
<PropertyGroup>
    <Nullable>enable</Nullable>
</PropertyGroup>
```

| C# | SQL | Note |
|----|-----|------|
| `string Name` | `NOT NULL` | Non-nullable reference |
| `string? Description` | `NULL` | Nullable reference |
| `int Stock` | `NOT NULL` | Value type sempre non-nullable |
| `int? SupplierId` | `NULL` | Nullable value type |
| `Category Category` | Dipende | Se required nav → `NOT NULL` FK |
| `Category? Category` | `NULL` | Optional navigation → `NULL` FK |

### Required vs Optional

```csharp
// Required (NOT NULL)
public class Product {
    public string Name { get; set; }              // NOT NULL
    public int CategoryId { get; set; }           // NOT NULL
    public Category Category { get; set; }        // Required navigation
}

// Optional (NULL)
public class Product {
    public string? Description { get; set; }      // NULL
    public int? SupplierId { get; set; }          // NULL
    public Supplier? Supplier { get; set; }       // Optional navigation
}
```

### Attributi Sovrascrittura

```csharp
// Forza NOT NULL anche su nullable
[Required]
public string? Name { get; set; }  // → NOT NULL nel DB

// Permette NULL anche su non-nullable (C# pre-8)
public string Description { get; set; }  // → NULL se Nullable disabled
```

---

## Indici

### Creazione Automatica

| Elemento | Indice Automatico? | Tipo | Nome |
|----------|-------------------|------|------|
| Primary Key | ✅ Sì | Clustered (SQL Server) / B-Tree | `PK_{TableName}` |
| Foreign Key | ✅ Sì | Non-clustered | `IX_{TableName}_{FKColumn}` |
| Unique constraint | ✅ Sì | Unique | `AK_{TableName}_{Column}` |
| Proprietà normale | ❌ No | - | Serve configurazione |

### Indici Espliciti

```csharp
// EF Core 7+: [Index] attribute
[Index(nameof(Email), IsUnique = true)]
public class User {
    public string Email { get; set; }
}

// Composite index
[Index(nameof(LastName), nameof(FirstName))]
public class Person {
    public string LastName { get; set; }
    public string FirstName { get; set; }
}

// Fluent API
modelBuilder.Entity<User>()
    .HasIndex(u => u.Email)
    .IsUnique();
```

---

## Nomi Generati

### Constraints

| Constraint | Pattern | Esempio |
|-----------|---------|---------|
| Primary Key | `PK_{TableName}` | `PK_Products` |
| Foreign Key | `FK_{DependentTable}_{PrincipalTable}_{FKProperty}` | `FK_Products_Categories_CategoryId` |
| Unique | `AK_{TableName}_{Column}` | `AK_Users_Email` |
| Index | `IX_{TableName}_{Column(s)}` | `IX_Products_Name` |
| Check | `CK_{TableName}_{ColumnOrDescription}` | `CK_Products_Price_Range` |

### Tabelle

| Scenario | Nome Generato |
|----------|---------------|
| DbSet definito | Nome del DbSet |
| Nessun DbSet | Nome classe (pluralizzato) |
| Join table N:M | `{Entity1}{Entity2}` (alfabetico) |
| TPH inheritance | Nome classe base |

**Esempio N:M:**
```csharp
Student + Course → CourseStudent  // Alfabetico: C prima di S
Order + Product → OrderProduct
```

---

## Delete Behavior

### Convenzioni Default

| Tipo Relazione | FK | Delete Behavior | Cosa Succede |
|----------------|----|-----------------|--------------| 
| Required (1:N) | `int` | `Cascade` | Elimina figli quando elimini padre |
| Optional (1:N) | `int?` | `ClientSetNull` | Setta FK a NULL solo in memoria |
| Required (1:1) | `int` | `Cascade` | Elimina dependent quando elimini principal |
| Optional (1:1) | `int?` | `ClientSetNull` | Setta FK a NULL |
| N:M (join table) | - | `Cascade` | Elimina righe join table |
| Self-reference | Dipende | `Restrict` o `NoAction` | Previene eliminazione ciclica |

### Differenze tra Behaviors

| Behavior | Client-Side | Database | Quando Usare |
|----------|-------------|----------|--------------|
| `Cascade` | Marca per delete | `ON DELETE CASCADE` | Elimina figli automaticamente |
| `Restrict` | Exception | `ON DELETE RESTRICT` | Blocca delete se ci sono figli |
| `SetNull` | Setta null | `ON DELETE SET NULL` | FK optional, mantieni figli orfani |
| `NoAction` | Niente | Nessun constraint | Errore DB se ci sono figli |
| `ClientSetNull` | Setta null | Nessun constraint | Solo in memoria, niente nel DB |
| `ClientCascade` | Marca per delete | Nessun constraint | Solo in memoria |
| `ClientNoAction` | Niente | Nessun constraint | Nessuna azione |

```csharp
// Override convenzione
modelBuilder.Entity<Product>()
    .HasOne(p => p.Category)
    .WithMany(c => c.Products)
    .OnDelete(DeleteBehavior.Restrict);  // Blocca delete se ci sono prodotti
```

---

## Inheritance (Ereditarietà)

### Table-Per-Hierarchy (TPH) - Default

```csharp
public abstract class Animal {
    public int Id { get; set; }
    public string Name { get; set; }
}

public class Dog : Animal {
    public string Breed { get; set; }
}

public class Cat : Animal {
    public int Lives { get; set; }
}
```

**Convenzione:**
- ✅ Tutte le classi in una tabella `Animals`
- ✅ Colonna `Discriminator` (tipo `string`)
- ✅ Valore discriminator = nome classe (`"Dog"`, `"Cat"`)

**Tabella generata:**
```sql
CREATE TABLE Animals (
    Id INT PRIMARY KEY,
    Name NVARCHAR(MAX),
    Discriminator NVARCHAR(MAX),  -- "Dog" o "Cat"
    Breed NVARCHAR(MAX),           -- Solo per Dog (NULL per Cat)
    Lives INT                      -- Solo per Cat (NULL per Dog)
)
```

### Table-Per-Type (TPT)

```csharp
// Serve configurazione esplicita
modelBuilder.Entity<Dog>().ToTable("Dogs");
modelBuilder.Entity<Cat>().ToTable("Cats");
```

**Non è convenzione**, serve opt-in esplicito.

### Table-Per-Concrete-Type (TPC) - EF Core 7+

```csharp
modelBuilder.Entity<Animal>().UseTpcMappingStrategy();
```

**Non è convenzione**, serve opt-in esplicito.

---

## Shadow Properties

### Cos'è una Shadow Property?

Proprietà che esiste nel modello EF ma non nella classe C#.

### Creazione Automatica (Convenzione)

| Scenario | Shadow Property Creata |
|----------|------------------------|
| Navigation senza FK property | `{NavigationName}Id` |
| FK inferita da convenzione | Nome basato su navigation |
| Discriminator in TPH | `Discriminator` (string) |

```csharp
// Esempio: Shadow FK
public class Product {
    // Nessuna proprietà CategoryId
    public Category Category { get; set; }
}
// EF crea shadow property "CategoryId"

// Accesso in query
context.Products
    .Where(p => EF.Property<int>(p, "CategoryId") == 5);
```

### Convenzioni Shadow Properties

| Proprietà | Tipo | Quando Creata |
|-----------|------|---------------|
| FK per navigation | Tipo della PK principale | Sempre se FK assente |
| `Discriminator` | `string` | TPH inheritance |

---

## Backing Fields

### Rilevamento Automatico

EF Core rileva backing fields automaticamente se seguono pattern:

| Pattern | Rilevato? |
|---------|-----------|
| `_fieldName` + `public FieldName` | ✅ Sì |
| `_FieldName` + `public FieldName` | ✅ Sì |
| `m_fieldName` + `public FieldName` | ✅ Sì |
| `fieldName` + `public FieldName` | ❌ No (convenzione C#) |

```csharp
public class Product {
    private string _name;
    public string Name {
        get => _name;
        set => _name = value?.Trim();
    }
    
    // EF usa il backing field per leggere/scrivere
}
```

**Convenzione:**
- ✅ EF legge/scrive direttamente nel backing field (bypassa setter)
- ✅ Utile per encapsulation e validazione

---

## Conversioni e Comparatori

### Value Converters (NO convenzione automatica)

```csharp
// Non c'è convenzione automatica per conversioni
// Serve configurazione esplicita

// Esempio: Enum → String
modelBuilder.Entity<Order>()
    .Property(e => e.Status)
    .HasConversion<string>();  // Esplicito!

// JSON
modelBuilder.Entity<Product>()
    .Property(e => e.Tags)
    .HasConversion(
        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null)
    );
```

**Nota:** EF Core 8 ha supporto nativo per collezioni primitive (PostgreSQL arrays, JSON per altri).

---

## Concurrency

### Concurrency Tokens

| Convenzione | Comportamento |
|-------------|---------------|
| Proprietà `byte[] RowVersion` | Timestamp concurrency token (SQL Server) |
| `[Timestamp]` attribute | Concurrency token |
| Proprietà `xmin` (PostgreSQL) | System column per concurrency |

```csharp
public class Product {
    public int Id { get; set; }
    
    [Timestamp]
    public byte[] RowVersion { get; set; }  // Auto-gestito da SQL Server
}
```

**Convenzione SQL Server:**
- ✅ `byte[] RowVersion` → `ROWVERSION`/`TIMESTAMP` automatico

---

## Query Filters

### Convenzioni Soft Delete (NO automatico)

Non c'è convenzione automatica per soft delete. Serve configurazione:

```csharp
public class Product {
    public bool IsDeleted { get; set; }  // NO filtro automatico
}

// Serve configurazione esplicita
modelBuilder.Entity<Product>()
    .HasQueryFilter(p => !p.IsDeleted);
```

---

## Altre Convenzioni

### Owned Types

```csharp
// NO convenzione automatica
public class Order {
    public Address ShippingAddress { get; set; }  // Non owned di default
}

// Serve [Owned] o Fluent API
[Owned]
public class Address {
    public string Street { get; set; }
    public string City { get; set; }
}
```

### Complex Types (EF Core 8)

```csharp
// Complex types: value objects senza identità
public class Money {
    public decimal Amount { get; set; }
    public string Currency { get; set; }
}

public class Product {
    public Money Price { get; set; }  // Owned di default se semplice
}
```

**Convenzione EF Core 8:**
- ✅ Tipi complessi senza PK → owned by convention (se abilitato)
- Serve opt-in: `modelBuilder.Entity<Product>().ComplexProperty(p => p.Price)`

### Sequences

**NO convenzione**. Serve configurazione:

```csharp
modelBuilder.HasSequence<int>("OrderNumbers")
    .StartsAt(1000)
    .IncrementsBy(1);
```

### Temporal Tables (EF Core 6+, SQL Server)

**NO convenzione**. Serve opt-in:

```csharp
modelBuilder.Entity<Product>()
    .ToTable(tb => tb.IsTemporal());
```

### JSON Columns (EF Core 7+)

```csharp
public class Product {
    public Metadata Details { get; set; }  // NO JSON di default
}

// Serve configurazione
modelBuilder.Entity<Product>()
    .OwnsOne(p => p.Details, 
        builder => builder.ToJson());
```

---

## Riepilogo Generale

### ✅ Convenzioni che Funzionano Automaticamente

1. **Primary Keys**: `Id` o `{ClassName}Id`
2. **Foreign Keys**: `{NavigationProperty}Id`
3. **Relazioni**: Collection + Reference = 1:N
4. **Nullability**: Basata su nullable reference types
5. **Tipi di dato**: Mapping standard C# → SQL
6. **Indici**: PK e FK indicizzati automaticamente
7. **Delete Behavior**: Cascade per required, ClientSetNull per optional
8. **Inheritance**: TPH di default
9. **Shadow FK**: Creata se navigation senza FK property
10. **Backing Fields**: Pattern `_fieldName` rilevato

### ❌ NON Convenzioni (Servono Configurazioni)

1. **Composite Primary Keys**
2. **Nomi tabella/colonna custom**
3. **Lunghezza stringhe specifica**
4. **Precisione decimali custom**
5. **Relazioni 1:1 complesse**
6. **Indici compositi o unique custom**
7. **Value converters** (enum → string, JSON, etc.)
8. **Owned types espliciti**
9. **Query filters** (soft delete)
10. **Temporal tables**
11. **JSON columns**
12. **Check constraints**
13. **Computed columns**
14. **Default values custom**

---

## Best Practices

### ✅ DA FARE

1. **Segui le convenzioni** quando possibile
2. **Usa nullable reference types** (`<Nullable>enable</Nullable>`)
3. **FK esplicite** per chiarezza: `CategoryId` + `Category`
4. **Nomi chiari**: `Product`, non `Prod` o `tblProduct`
5. **Fluent API in classi separate** per progetti grandi
6. **`ICollection<T>` per navigation collections**, non `List<T>`
7. **Init collections**: `= new List<T>();`

### ❌ DA EVITARE

1. **Non combattere le convenzioni** senza motivo
2. **Non usare nomi ambigui** che confondono EF
3. **Non ignorare nullability** (crea bug)
4. **Non usare `IEnumerable<T>` per navigation** (solo `ICollection<T>`)
5. **Non dimenticare `virtual`** per lazy loading (se abilitato)

---

## Riferimenti

- [EF Core Conventions Documentation](https://learn.microsoft.com/en-us/ef/core/modeling/)
- [EF Core 8 What's New](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-8.0/whatsnew)

---

**Documento aggiornato per Entity Framework Core 8.0**  
**Ultima revisione: Novembre 2024**