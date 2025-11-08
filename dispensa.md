# Entity Framework - Tips & Tricks
## Indice

1. [Introduzione e Concetti Base](#1-introduzione-e-concetti-base)
2. [DB First, Code First e DbContext](#2-db-first-code-first-e-dbcontext)
3. [Migrations](#3-migrations)
4. [Query e Operazioni CRUD](#4-query-e-operazioni-crud)
---

## 1. Introduzione e Concetti Base

### Cos'è Entity Framework?

Quando lavoriamo con i database nelle nostre applicazioni .NET, ci troviamo spesso di fronte a una sfida: da un lato abbiamo il mondo degli oggetti C#, dall'altro il mondo relazionale dei database SQL. Questi due mondi parlano linguaggi diversi e hanno logiche diverse. È qui che entra in gioco Entity Framework.

Entity Framework è un ORM, ovvero un Object-Relational Mapper. In parole semplici, è uno strumento che fa da ponte tra questi due mondi. Ci permette di lavorare con il database usando oggetti C# invece di scrivere query SQL a mano. Il framework si occupa di tradurre le nostre operazioni sugli oggetti in query SQL appropriate, gestisce automaticamente la connessione al database, tiene traccia delle modifiche che apportiamo alle entità e molto altro.

Per capire meglio il valore di Entity Framework, vediamo un esempio concreto. Immaginiamo di voler recuperare tutti i prodotti con un prezzo superiore a 100 euro. Senza Entity Framework, dovremmo scrivere codice come questo:

```csharp
// Senza EF
var connection = new SqlConnection(connectionString);
var command = new SqlCommand("SELECT * FROM Products WHERE Price > @price", connection);
command.Parameters.AddWithValue("@price", 100);
connection.Open();
var reader = command.ExecuteReader();

// Con EF
var expensiveProducts = context.Products
    .Where(p => p.Price > 100)
    .ToList();
```

Come si può notare, la differenza è notevole. Nel primo caso dobbiamo gestire manualmente la connessione, creare il comando SQL, parametrizzarlo correttamente e poi leggere i risultati. Con Entity Framework, invece, possiamo esprimere la stessa logica in modo molto più naturale e conciso, usando direttamente oggetti C# e le espressioni lambda di LINQ.

### I vantaggi e gli svantaggi di un ORM

Come ogni tecnologia, anche l'uso di un ORM come Entity Framework comporta dei trade-off che è importante conoscere per fare scelte consapevoli.

Partiamo dai vantaggi. Il primo e più evidente è la riduzione drastica del codice boilerplate. Non dobbiamo più scrivere sempre le stesse righe per aprire connessioni, creare comandi, gestire DataReader e così via. Questo ci permette di concentrarci sulla logica di business vera e propria, quella che porta valore all'applicazione. Inoltre, lavorando con oggetti C# anziché con SQL, possiamo sfruttare il controllo del compilatore: molti errori che con SQL classico scopriremmo solo a runtime, con Entity Framework vengono intercettati già in fase di compilazione.

Un altro vantaggio importante è il supporto quasi automatico a più tecnologie di database. Se il nostro codice usa le astrazioni di Entity Framework, passare da SQL Server a PostgreSQL o MySQL richiede spesso solo un cambio di configurazione, senza dover riscrivere le query. Entity Framework include anche numerose ottimizzazioni integrate che il framework applica automaticamente, come il batching delle query o la generazione di SQL efficiente.

Naturalmente ci sono anche degli svantaggi da considerare. Le query molto complesse generate da un ORM possono risultare meno ottimizzate rispetto a SQL scritto a mano da un esperto. Il meccanismo di tracking delle entità, che vedremo in dettaglio più avanti, ha un costo in termini di memoria e performance. Inoltre, se non si strutturano bene le query a livello di codice, si rischia di generare accessi al database inutili o inefficienti. Infine, alcune funzionalità specifiche dei database, come le stored procedure complesse, hanno un supporto limitato negli ORM.

### Quando ha senso usare un ORM?

La domanda che sorge naturale è: quando conviene usare Entity Framework e quando invece è meglio evitarlo?

Entity Framework brilla nelle applicazioni con un modello dati complesso, dove le relazioni tra le entità sono numerose e articolate. È perfetto per le applicazioni CRUD classiche, dove la maggior parte delle operazioni consiste nel creare, leggere, aggiornare ed eliminare record. Se stiamo sviluppando un'applicazione secondo i principi del Domain-Driven Design, l'ORM si integra perfettamente con questo approccio. Infine, se prevediamo di dover supportare più database o vogliamo mantenere questa flessibilità per il futuro, Entity Framework ci facilita enormemente il lavoro.

Ci sono però scenari in cui Entity Framework potrebbe non essere la scelta migliore. Se dobbiamo eseguire operazioni massive su milioni o miliardi di record, le performance di un ORM potrebbero non essere sufficienti. Per query molto complesse, magari con Common Table Expressions (CTE) o logiche particolari, scrivere SQL a mano potrebbe essere più efficiente. Se la nostra applicazione è fortemente dipendente dalle performance di query complesse e ogni millisecondo conta, un ORM potrebbe aggiungere un overhead inaccettabile.

È importante sapere, però, che Entity Framework non ci costringe a scegliere tutto o niente. Il framework fornisce metodi come `ExecuteRawQuery` che ci permettono di implementare approcci ibridi, usando l'ORM dove conviene e SQL raw dove è necessario.

---

## 2. DB First, Code First e DbContext

### Due filosofie a confronto: DB First e Code First

Quando iniziamo a lavorare con Entity Framework, una delle prime decisioni da prendere riguarda l'approccio da seguire: partiamo dal database o dal codice? Questa scelta non è puramente tecnica, ma riflette filosofie diverse di sviluppo e ha implicazioni importanti sul nostro modo di lavorare.

### L'approccio DB First

L'approccio DB First parte dal presupposto che il database esista già o che comunque vogliamo definirlo prima di tutto a livello di schema SQL. In questo scenario, creiamo le tabelle, le relazioni, gli indici e tutti gli altri oggetti database usando strumenti SQL tradizionali o un progetto di database. Una volta che lo schema è pronto, Entity Framework entra in gioco per generare automaticamente le classi C# che mappano quel database.

Il comando per generare queste classi è piuttosto semplice:

```bash
dotnet ef dbcontext scaffold "ConnectionString" Npgsql.EntityFrameworkCore.PostgreSQL
```

Questo approccio ha dei vantaggi significativi in determinati contesti. È praticamente l'unica strada percorribile quando dobbiamo integrare Entity Framework con database legacy complessi, che magari sono il risultato di anni o decenni di sviluppo. Riscrivere questi database sarebbe troppo costoso e rischioso, quindi dobbiamo adattarci a quanto esiste. Inoltre, questo approccio lascia pieno controllo al DBA, che può ottimizzare al massimo la struttura del database sfruttando tutte le funzionalità specifiche del DBMS in uso.

Naturalmente ci sono anche degli svantaggi. Lavorando a livello di database, perdiamo parte del controllo che avremmo a livello di codice C#. Il supporto multi-database diventa più complicato, perché potremmo aver usato funzionalità specifiche di un particolare DBMS. Infine, non possiamo sfruttare la generazione automatica del database che Entity Framework offre con l'approccio Code First.

### L'approccio Code First

Code First ribalta completamente la prospettiva: il modello dati viene definito attraverso classi C#, e sarà Entity Framework a occuparsi di convertire queste classi in query SQL per creare le tabelle di persistenza. È l'approccio più naturale quando partiamo da zero con un nuovo progetto.

I vantaggi di questo approccio sono numerosi. Abbiamo controllo totale del modello dati e possiamo sfruttare appieno le potenzialità di astrazione e incapsulamento di C#. Possiamo applicare pattern architetturali tipici della programmazione orientata agli oggetti, come il Domain-Driven Design. Il modello dati vive nel codice, quindi è più facile da testare unitariamente e da versionare con Git. Le migration automatiche ci permettono di evolvere il database in modo controllato e tracciabile.

Gli svantaggi principali sono l'aggiunta di uno strato di complessità al progetto e la difficoltà nell'applicare questo approccio su database legacy molto complessi. Inoltre, dobbiamo imparare a "pensare" in termini di oggetti e lasciare che sia Entity Framework a tradurre in SQL, cosa che all'inizio può richiedere un cambio di mentalità.

### DbContext: il cuore di Entity Framework

Indipendentemente dall'approccio scelto, il punto di ingresso principale per lavorare con Entity Framework è sempre la classe `DbContext`. Questa classe è letteralmente il cuore del framework e gestisce praticamente ogni aspetto dell'interazione con il database.

Il `DbContext` gestisce il ciclo di vita completo delle connessioni al database, aprendole quando necessario e chiudendole quando non servono più. Contiene i `DbSet`, che sono le collezioni di oggetti che mappano le nostre tabelle. Tiene traccia di tutte le modifiche che apportiamo alle entità attraverso il meccanismo del Change Tracking. Fornisce il punto in cui configurare le entità attraverso il metodo `OnModelCreating`. E si occupa anche di chiudere correttamente tutte le connessioni quando il contesto viene distrutto.

Ecco un esempio di DbContext basilare:

```csharp
public class AppDbContext : DbContext 
{
    public DbSet<Product> Products { get; set; } 
    public DbSet<Category> Categories { get; set; } 
    public DbSet<Order> Orders { get; set; }
    
    public AppDbContext(DbContextOptions<AppDbContext> options) 
        : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder) 
    {
        // Qui vanno le configurazioni
    }
}
```

### Le classi Entity

Le classi entity sono le fondamenta del nostro modello dati. Ogni classe entity rappresenta un'entità del dominio e verrà mappata da Entity Framework in una tabella del database. Queste classi sono semplici POCO (Plain Old CLR Objects), quindi non hanno dipendenze particolari e rimangono facilmente testabili.

Le classi entity non mappano solo i dati della singola tabella, ma anche le relazioni con altre entità. Possiamo esprimere relazioni uno-a-uno, uno-a-molti e molti-a-molti direttamente attraverso le proprietà di navigazione.

Ecco un esempio concreto:

```csharp
public class Product 
{ 
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CategoryId { get; set; }
    
    // Relazione 1:1, 1:m
    public Category Category { get; set; }
    
    // Relazione m:m
    public ICollection<OrderItem> OrderItems { get; set; }
}
```

In questo esempio, la proprietà `Category` rappresenta una relazione molti-a-uno (tanti prodotti appartengono a una categoria), mentre `OrderItems` rappresenta una relazione molti-a-molti con gli ordini attraverso una tabella di join.

### Le convenzioni di Entity Framework

Una delle caratteristiche più interessanti di Entity Framework è il suo sistema di convenzioni. Il framework definisce una serie di regole che, se seguite, ci permettono di evitare completamente la configurazione esplicita. Questo approccio si basa sul principio "convention over configuration": invece di configurare tutto esplicitamente, seguiamo convenzioni sensate e configuriamo solo le eccezioni.

Per quanto riguarda le chiavi primarie, se una proprietà si chiama `Id` o `{NomeClasse}Id`, Entity Framework la riconosce automaticamente come chiave primaria e la rende auto-incrementante. Per le tabelle e le colonne, il nome della tabella deriva dal nome del `DbSet` (in inglese viene anche pluralizzato automaticamente), mentre il nome delle colonne corrisponde esattamente al nome delle proprietà. Il tipo di dato SQL viene mappato automaticamente dal tipo C#.

Le foreign key seguono una convenzione molto pratica: se abbiamo una proprietà chiamata `{NomeNavigazione}Id` insieme a una proprietà di navigazione, Entity Framework capisce automaticamente che si tratta di una foreign key. Ad esempio, avere `CategoryId` insieme a una proprietà `Category` è sufficiente perché il framework crei la foreign key corretta.

Per le relazioni, la presenza di una navigation property crea automaticamente la relazione corrispondente. Una proprietà di tipo `ICollection<T>` indica una relazione uno-a-molti, mentre una singola proprietà indica una relazione uno-a-uno o molti-a-uno.

Il comportamento nullable segue le convenzioni C#: i tipi nullable (come `string?` o `int?`) diventano colonne nullable nel database, mentre i tipi non-nullable diventano colonne `NOT NULL`. Da notare che le stringhe sono nullable di default.

Anche i tipi di dati comuni hanno mapping predefiniti: `int` diventa `INT`, `string` diventa `NVARCHAR(MAX)`, `decimal` diventa `DECIMAL(18,2)`, `DateTime` diventa `DATETIME2` e `bool` diventa `BIT`.

Per quanto riguarda il comportamento in caso di cancellazione, Entity Framework imposta di default `CASCADE` per le relazioni obbligatorie e `NO ACTION` per quelle opzionali.

### Configurazione esplicita: Fluent API

Quando le convenzioni non sono sufficienti o vogliamo avere un controllo più preciso, possiamo usare la Fluent API. Questo è un sistema di configurazione potente e flessibile che ci permette di definire praticamente ogni aspetto delle nostre tabelle e relazioni attraverso un'interfaccia fluida e fortemente tipizzata.

La Fluent API si usa tipicamente creando classi di configurazione dedicate che implementano `IEntityTypeConfiguration<T>`:

```csharp
public class ProductConfiguration : IEntityTypeConfiguration<Product> 
{
    public void Configure(EntityTypeBuilder<Product> builder) 
    { 
        builder.ToTable("products");
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("product_name");
            
        builder.Property(e => e.Price)
            .HasColumnType("decimal(18,2)");
            
        builder.HasOne(e => e.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(e => e.CategoryId);
    }
}
```

Questo approccio mantiene il modello dati pulito e separa le preoccupazioni della persistenza dalla logica di dominio.

### Configurazione alternativa: Data Annotations

Un'alternativa alla Fluent API sono le Data Annotations, che sono attributi che possiamo applicare direttamente alle classi e alle proprietà. Questo approccio è meno verboso della Fluent API, ma ha lo svantaggio di "sporcare" il modello dati con dettagli di persistenza.

```csharp
[Table("products")]
public class Product 
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    [Column("product_name")]
    public string Name { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }
    
    [ForeignKey("Category")]
    public int CategoryId { get; set; }
    
    public Category Category { get; set; }
}
```

La scelta tra Fluent API e Data Annotations è spesso una questione di preferenze personali e degli standard del team. La Fluent API è generalmente preferita in progetti più grandi perché mantiene il modello più pulito e offre maggiore flessibilità.

---

## 3. Migrations

### Le Migration: da nemiche ad amiche

Se c'è un aspetto di Entity Framework che inizialmente può spaventare o frustrare gli sviluppatori, sono proprio le migration. Eppure, una volta compreso il loro funzionamento e la loro logica, le migration diventano uno strumento potentissimo e indispensabile. Il loro scopo è semplice ma fondamentale: rappresentare la storia dell'evoluzione del nostro modello dati.

### Cosa sono le Migration

Le migration di Entity Framework sono file di codice C# che descrivono come il nostro database deve evolversi nel tempo. Ogni migration rappresenta un "passo" nell'evoluzione dello schema: l'aggiunta di una tabella, la modifica di una colonna, la creazione di un indice. Questi file vengono generati automaticamente da Entity Framework analizzando le differenze tra il modello dati corrente e quello precedente.

Il framework esegue queste migration in maniera sequenziale, una dopo l'altra, per portare il database dallo stato iniziale allo stato desiderato. Questo approccio ci dà una tracciabilità completa: guardando le migration possiamo vedere esattamente come e quando il database è cambiato nel tempo. È come avere un Git per lo schema del database.

C'è una regola d'oro che va tatuata nella mente di ogni sviluppatore che usa Entity Framework: ogni volta che viene fatta una modifica al modello dati, è necessario aggiornare le migration. Non è un'opzione, non è qualcosa da fare "quando si ha tempo". È parte integrante del flusso di sviluppo, tanto quanto committare il codice.

### Il Model Snapshot

Ogni volta che generiamo una nuova migration, Entity Framework fa due cose importanti. Prima di tutto, crea un nuovo file con un nome che include un timestamp, qualcosa come `20241108123045_AddProductDescription.cs`. Questo file contiene le istruzioni per applicare la modifica (metodo `Up`) e per annullarla (metodo `Down`). Con l'accumularsi delle migration, si forma uno storico incrementale di tutte le modifiche apportate al database.

La seconda cosa che Entity Framework fa è aggiornare il file di snapshot, tipicamente chiamato `ModelSnapshot.cs`. Questo file è particolarmente importante perché rappresenta lo stato attuale del modello dati, quello che abbiamo dopo aver applicato tutte le migration. Quando andremo a creare la migration successiva, Entity Framework confronterà il modello C# corrente con questo snapshot per capire cosa è cambiato e cosa deve essere aggiornato nel database.

### Anatomia di una Migration

Vediamo com'è fatto concretamente un file di migration. Prendiamo come esempio l'aggiunta di un campo `CreatedTimestamp` a una tabella `Blog`:

```csharp
using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MyProject.Migrations
{
    public partial class AddBlogCreatedTimestamp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedTimestamp",
                table: "Blog",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
        
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedTimestamp",
                table: "Blog");
        }
    }
}
```

Il metodo `Up` contiene le operazioni da eseguire per applicare la migration, mentre `Down` contiene le operazioni per annullarla. Questo ci dà la possibilità di fare rollback se qualcosa va storto.

### La Tabella __EFMigrationsHistory

Quando Entity Framework lavora con un database, crea automaticamente una tabella speciale chiamata `__EFMigrationsHistory`. Questa tabella è il registro di tutte le migration che sono state applicate a quel database. Ogni volta che eseguiamo una migration, Entity Framework controlla questa tabella per capire quali migration sono già state applicate e quali mancano ancora. È così che il framework sa se il database è aggiornato o meno.

### I problemi più comuni con le Migration

Lavorare con le migration non è sempre semplice, e ci sono alcuni problemi che capitano così frequentemente che vale la pena conoscerli in anticipo per non farsi prendere dal panico quando si presentano.

#### Primo problema: Pending Changes

È probabilmente l'errore più comune. Stiamo lavorando al codice, avviamo l'applicazione e ci troviamo davanti un errore che dice "Pending Model Changes Detected". Cosa significa? Semplicemente che abbiamo modificato il modello dati nel codice C#, ma non abbiamo ancora generato o applicato la migration corrispondente. Entity Framework se ne accorge confrontando il modello corrente con lo snapshot e ci avverte che c'è una discrepanza.

La soluzione è diretta: dobbiamo generare la migration con `dotnet ef migrations add NomeMigration` e poi applicarla con `dotnet ef database update`. È importante farlo subito, non rimandare. Lasciare il modello e il database non sincronizzati è una ricetta sicura per problemi futuri.

#### Secondo problema: Migration Vuota

A volte generiamo una migration e ci ritroviamo con un file praticamente vuoto, o Entity Framework ci dice "No changes detected". Questo può essere frustrante perché siamo sicuri di aver fatto delle modifiche. Le cause più comuni sono tre.

La prima è che la build sia fallita. Entity Framework analizza gli assembly compilati, quindi se il progetto non compila correttamente, il framework non riesce a vedere le modifiche. La soluzione è verificare con `dotnet build` che tutto compili senza errori.

La seconda causa è banale ma succede più spesso di quanto si pensi: non abbiamo salvato tutti i file. A volte l'IDE non salva automaticamente e le modifiche che vediamo nell'editor non sono ancora su disco. Una bella salvata generale risolve il problema.

La terza causa, che è anche la più frustrante, è che abbiamo già creato la migration in precedenza e ora Entity Framework non vede più differenze. In questo caso dobbiamo rimuovere la migration vuota con `dotnet ef migrations remove` e rigenerarla.

#### Terzo problema: Colonna NOT NULL su Tabella Popolata

Questo è un problema classico che capita quando aggiungiamo un campo obbligatorio a una tabella che contiene già dati. Immaginiamo di avere una tabella `Users` con 1000 utenti e decidiamo di aggiungere un campo `Email` obbligatorio. Entity Framework genera una migration che cerca di creare questa colonna NOT NULL, ma il database giustamente protesta: che valore dovrebbero avere i 1000 utenti esistenti?

Ci sono due approcci per risolvere questo problema. Il primo è specificare un valore di default nella migration:

```csharp
migrationBuilder.AddColumn<string>(
    name: "Email",
    table: "Users",
    nullable: false,
    defaultValue: "noemail@example.com");
```

Il secondo approccio è più raffinato ma richiede più passaggi: prima creiamo la colonna come nullable, poi scriviamo uno script o una migration personalizzata per popolare i dati esistenti, e infine modifichiamo la colonna per renderla NOT NULL. Questo approccio è preferibile quando vogliamo assicurarci che ogni record abbia un valore significativo piuttosto che un placeholder.

#### Quinto problema: I Conflitti

I conflitti nelle migration sono probabilmente l'aspetto più frustrante, specialmente quando si lavora in team. Sono anche inevitabili se il team è attivo e più sviluppatori lavorano in parallelo sul modello dati. Cerchiamo di capire come si verificano e come gestirli.

### Come nascono i conflitti

Immaginiamo uno scenario tipico. Il developer A crea un branch, aggiunge una colonna al modello `Product` e genera una migration chiamata `20241108120000_AddColumnA`. Nel frattempo, il developer B sul suo branch aggiunge una colonna diversa e genera `20241108121000_AddColumnB`. Entrambe le migration modificano lo snapshot del modello. Quando uno dei due fa merge del codice dell'altro, Git si trova davanti a modifiche incompatibili sullo stesso file di snapshot e segnala un conflitto.

Il problema è che lo snapshot non è un file che possiamo risolvere manualmente facilmente. Contiene codice C# generato automaticamente che descrive l'intero modello dati. Cercare di risolvere il conflitto a mano editando questo file è rischioso e quasi sempre porta a problemi.

### Strategia 1: Ricreare la Migration

L'approccio più semplice e sicuro è ricreare la migration da zero dopo il merge. Ecco come procedere. Prima facciamo il merge del branch principale nel nostro branch. A questo punto avremo conflitti nello snapshot e probabilmente anche nel nostro file di migration. Non cerchiamo di risolverli manualmente. Invece, rimuoviamo completamente la nostra migration con `dotnet ef migrations remove`. Questo comando elimina sia il file di migration che ripristina lo snapshot allo stato precedente. Poi rigeneriam o la migration con `dotnet ef migrations add NomeMigration`. Entity Framework a questo punto parte dallo snapshot aggiornato (che include già le modifiche dell'altro developer) e genera una migration che contiene solo le nostre modifiche.

```bash
git merge main
dotnet ef migrations remove
dotnet ef migrations add NuovaMigration
```

### Strategia 2: Rebase

Un approccio alternativo, particolarmente utile se vogliamo mantenere una storia Git più lineare, è usare il rebase. L'idea è simile: invece di fare merge, facciamo rebase del nostro branch sul branch principale. Anche qui, quando incontriamo i conflitti, rimuoviamo la nostra migration e la rigeneriamo dopo che il rebase è completato. Il vantaggio del rebase è che evita i commit di merge e mantiene la storia più pulita.

### Strategia 3: Pull Request con Lock

In team più strutturati, può essere utile implementare una strategia più rigorosa. Le pull request richiedono sempre una review approvata prima del merge. Una volta che una PR viene mergiata su main, nessun altro può pushare direttamente finché le migration non sono state applicate. Gli altri developer devono fare rebase obbligatorio prima di poter procedere. Questo approccio richiede più disciplina ma riduce drasticamente i conflitti.

### Strategia 4: Pipeline CI

La soluzione più professionale, anche se richiede un po' di setup iniziale, è implementare una pipeline CI che verifica automaticamente lo stato delle migration. La pipeline può controllare se ci sono pending changes nel modello e bloccare il merge se il database non è sincronizzato. Questo forza tutti gli sviluppatori a mantenere le migration allineate prima di poter fare merge. Può sembrare rigido, ma in team grandi è spesso l'unica via per mantenere l'ordine.

La scelta della strategia dipende molto dalla dimensione e dalla maturità del team. Per team piccoli, la strategia 1 (ricreare la migration) è spesso sufficiente. Per team più grandi o progetti mission-critical, vale la pena investire in approcci più strutturati come la pipeline CI.

---

## 4. Query e Operazioni CRUD

### LINQ: il linguaggio delle query in C#

Una volta che abbiamo il nostro modello dati definito e le migration in ordine, è il momento di utilizzare effettivamente Entity Framework per interrogare e manipolare i dati. Per farlo, Entity Framework si basa pesantemente su LINQ, che sta per Language Integrated Query.

LINQ è una delle caratteristiche più potenti di C#. Ci permette di scrivere query contro diverse fonti dati usando una sintassi unificata e fortemente tipizzata. Che stiamo interrogando una lista in memoria, un database, un file XML o qualsiasi altra cosa, la sintassi rimane coerente. Questo non solo rende il codice più leggibile, ma ci permette anche di sfruttare il sistema di tipi di C# e l'IntelliSense dell'IDE.

Vediamo un esempio concreto per capire il valore di LINQ. Immaginiamo di voler filtrare una lista di prodotti per trovare quelli con prezzo superiore a 100 euro. Senza LINQ, dovremmo scrivere un loop manuale:

```csharp
// Senza LINQ
List<Prodotto> risultato = new List<Prodotto>();
foreach(var p in prodotti) 
{
    if(p.Prezzo > 100)
        risultato.Add(p);
}

// Con LINQ
var risultato = prodotti.Where(p => p.Prezzo > 100);
```

La differenza è evidente. Con LINQ esprimiamo cosa vogliamo ottenere (prodotti dove il prezzo è maggiore di 100) invece di spiegare come ottenerlo (crea una lista, fai un loop, controlla la condizione, aggiungi alla lista). Questo approccio dichiarativo rende il codice molto più leggibile e manutenibile.

### Gli operatori essenziali di LINQ

LINQ fornisce un set di operatori che coprono praticamente tutte le operazioni che potremmo voler fare su una collezione di dati. Non serve impararli tutti a memoria, ma è importante conoscere quelli più comuni perché li useremo continuamente.

L'operatore `Where()` è fondamentale: serve per filtrare i dati secondo una condizione. `Select()` ci permette di proiettare i dati, ovvero di trasformarli o di selezionare solo alcuni campi. Per ordinare i risultati abbiamo `OrderBy()` per l'ordinamento crescente e `OrderByDescending()` per quello decrescente.

Quando vogliamo recuperare un singolo elemento abbiamo diverse opzioni. `First()` restituisce il primo elemento che soddisfa una condizione e lancia un'eccezione se non lo trova. `FirstOrDefault()` fa la stessa cosa ma restituisce `null` invece di lanciare l'eccezione. `Single()` e `SingleOrDefault()` sono simili ma si assicurano che ci sia esattamente un elemento che soddisfa la condizione.

Per la paginazione, che è fondamentale nelle applicazioni web, usiamo `Skip()` per saltare un certo numero di elementi e `Take()` per prenderne un numero specificato. Per le aggregazioni abbiamo `Count()`, `Sum()`, `Average()`, `Max()` e `Min()`.

### IQueryable: la query che non è ancora una query

Quando lavoriamo con Entity Framework, è cruciale capire la differenza tra `IQueryable<T>` e `IEnumerable<T>`. Questa distinzione può fare la differenza tra un'applicazione performante e una lenta.

Un `IQueryable<T>` rappresenta una query che non è ancora stata eseguita. È come una ricetta: contiene tutte le istruzioni su cosa fare, ma non ha ancora fatto nulla. Vediamo un esempio:

```csharp
IQueryable<Product> query = context.Products; 
query = query.Where(p => p.Price > 100);
query = query.OrderBy(p => p.Name); 
query = query.Skip(10).Take(5); 

// Solo qui viene eseguita la query!
List<Product> results = query.ToList();
```

In questo codice, le prime quattro righe non eseguono nessuna query sul database. Stanno semplicemente costruendo l'espressione di query. Solo quando chiamiamo `ToList()`, Entity Framework prende tutta l'espressione, la traduce in SQL e la esegue. Il bello è che Entity Framework genera un'unica query SQL ottimizzata:

```sql
SELECT [p].[Id], [p].[Name], [p].[Price], [p].[Category], [p].[Description]
FROM [Products] AS [p]
WHERE [p].[Price] > 100
ORDER BY [p].[Name] 
OFFSET 10 ROWS
FETCH NEXT 5 ROWS ONLY;
```

Questa caratteristica è estremamente potente perché ci permette di comporre le query in modo modulare. Possiamo scrivere metodi che aggiungono filtri o ordinamenti alla query e poi combinarli, e Entity Framework genererà comunque un'unica query SQL efficiente.

### IQueryable vs IEnumerable: una differenza cruciale

La differenza tra `IQueryable<T>` e `IEnumerable<T>` non è solo una sottigliezza teorica. Ha implicazioni pratiche enormi sulle performance delle nostre applicazioni.

Un `IQueryable<T>`, come abbiamo visto, contiene la query ma non il risultato. Non carica dati in memoria e non esegue operazioni sul database fino a quando non glielo chiediamo esplicitamente. È pigro (lazy) per natura.

Un `IEnumerable<T>`, invece, contiene i dati veri e propri, non la query. Questo significa che quando abbiamo un `IEnumerable<T>`, Entity Framework ha già eseguito la query e ha caricato tutto in memoria. Ogni operazione successiva viene eseguita in memoria, non sul database.

Il problema sorge quando facciamo questo errore comune:

```csharp
// ❌ Cattiva pratica
var products = context.Products.ToList(); // Carica TUTTI i prodotti in memoria
var expensiveProducts = products.Where(p => p.Price > 100); // Filtra in memoria
```

In questo codice, la chiamata a `ToList()` causa l'esecuzione immediata della query e carica tutti i prodotti in memoria. Il filtro successivo viene eseguito in memoria. Se la tabella Products contiene milioni di record, abbiamo appena caricato milioni di oggetti in memoria solo per poi filtrarne una piccola parte.

La versione corretta è:

```csharp
// ✅ Buona pratica
var expensiveProducts = context.Products
    .Where(p => p.Price > 100)
    .ToList(); // Solo ora viene eseguita la query, già filtrata
```

Qui il filtro fa parte dell'espressione `IQueryable`, quindi viene tradotto in SQL e il database ci restituisce solo i record che ci servono.

La regola d'oro è: non si fa mai `.ToList()` a cuor leggero. Dobbiamo essere consapevoli che chiamare `ToList()`, `ToArray()`, `First()`, `Single()`, `Count()`, `Any()` o `All()` causa l'esecuzione immediata della query. Dobbiamo assicurarci di aver applicato tutti i filtri e le proiezioni necessarie prima di materializzare i risultati.

### Navigare le relazioni tra entità

Una delle caratteristiche più potenti di Entity Framework è la capacità di navigare le relazioni tra entità in modo naturale. Se abbiamo un ordine, possiamo accedere al cliente semplicemente scrivendo `order.Customer`. Dietro le quinte, però, ci sono due approcci molto diversi per caricare questi dati correlati, e la scelta tra i due ha implicazioni enormi sulle performance.

### Eager Loading: caricare tutto subito

L'eager loading significa caricare subito tutte le relazioni necessarie insieme all'entità principale. Entity Framework genera un'unica query SQL con tutti i JOIN necessari per recuperare tutti i dati in un colpo solo.

Il vantaggio principale di questo approccio è che carica completamente il grafo di oggetti correlati, rendendo tutti i dati immediatamente disponibili senza ulteriori accessi al database. Eseguiamo una sola query, ben ottimizzata, ed evitiamo il rischio di fare troppi accessi al database.

Lo svantaggio è la scalabilità: se non stiamo attenti, rischiamo di caricare troppi dati. Se includiamo molte relazioni in profondità, la query può diventare molto grande e portare in memoria una quantità di dati non necessaria.

### Lazy Loading: caricare solo quando serve

Il lazy loading ribalta l'approccio: data un'entità, carica le entità correlate solo se e quando vengono effettivamente richieste dal codice. Se non accediamo mai alla proprietà `order.Customer`, Entity Framework non caricherà mai quei dati.

Il vantaggio è che l'accesso alla persistenza si limita ai dati strettamente necessari. In scenari con modelli dati complessi o con tabelle molto grandi, questo può fare una grande differenza.

Il problema, però, è che il lazy loading può facilmente portare al temuto "N+1 Query Problem", un anti-pattern che vedremo tra poco e che può devastare le performance di un'applicazione.

### Include e Select: due modi per caricare le relazioni

Entity Framework ci fornisce principalmente due metodi per specificare quali relazioni vogliamo caricare: `Include()` e `Select()`. Sembrano simili ma hanno differenze importanti.

Il metodo `Include()` ci permette di specificare esplicitamente quali relazioni vogliamo caricare insieme all'entità principale. Le entità caricate con `Include()` sono tracciate dal Change Tracker, quindi possiamo modificarle e poi salvare le modifiche:

```csharp
var orders = context.Orders
    .Include(o => o.Customer)
    .Include(o => o.OrderItems)
    .Where(o => o.OrderDate >= startDate)
    .ToList();

// Possiamo modificare e salvare
orders[0].Status = "Shipped";
orders[0].Customer.LastOrderDate = DateTime.Now;
context.SaveChanges();
```

Il metodo `Select()`, invece, ci permette di creare una proiezione personalizzata. Possiamo scegliere esattamente quali proprietà caricare, anche da entità correlate. La differenza fondamentale è che `Select()` non carica le entità con tracking, quindi sono read-only:

```csharp
var orders = context.Orders
    .Select(o => new 
    {
        o.Customer,
        o.OrderItems
    })
    .Where(o => o.OrderDate >= startDate)
    .ToList();
```

Qual è meglio usare? Dipende dal caso d'uso. Se dobbiamo modificare i dati, usiamo `Include()`. Se i dati sono read-only (ad esempio per un'API GET o un report), `Select()` è generalmente più performante perché non ha l'overhead del tracking.

### Il problema N+1: un nemico silenzioso

Il problema N+1 è uno degli anti-pattern più comuni e pericolosi quando si lavora con un ORM. Si chiama così perché se abbiamo N record, finiremo per eseguire N+1 query al database: una per caricare i record principali e poi una query aggiuntiva per ogni record quando accediamo alle sue relazioni.

Vediamo un esempio concreto. Immaginiamo di voler visualizzare una lista di ordini con i nomi dei clienti:

```csharp
var orders = context.Orders.ToList();
foreach(var order in orders) 
{
    Console.WriteLine($"Ordine #{order.Id}");
    Console.WriteLine($"Cliente: {order.Customer.Name}");
}
```

Questo codice sembra innocente, ma nasconde un problema grave. Vediamo cosa succede realmente a livello di database. La prima riga carica tutti gli ordini con una query. Poi, nel loop, ogni volta che accediamo a `order.Customer.Name`, Entity Framework esegue una query separata per caricare quel cliente specifico. Se abbiamo 100 ordini, eseguiamo 101 query:

```sql
SELECT * FROM Orders;
SELECT * FROM Customers WHERE Id = 1;
SELECT * FROM Customers WHERE Id = 2;
SELECT * FROM Customers WHERE Id = 3;
-- ... altre 97 query ...
SELECT * FROM Customers WHERE Id = 100;
```

Immaginate l'impatto su un'applicazione web. Ogni richiesta HTTP che mostra una lista di ordini eseguirebbe decine o centinaia di query al database. I tempi di risposta diventano inaccettabili e il database viene bombardato di richieste.

La soluzione è usare l'eager loading con `Include()`:

```csharp
var orders = context.Orders
    .Include(o => o.Customer)
    .ToList();

foreach(var order in orders) 
{
    Console.WriteLine($"Ordine #{order.Id}");
    Console.WriteLine($"Cliente: {order.Customer.Name}");
}
```

Ora Entity Framework genera un'unica query con un JOIN:

```sql
SELECT *
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c]
ON [o].[CustomerId] = [c].[Id]
```

Una singola query, molto più efficiente. Il database fa il lavoro che sa fare bene (i JOIN) e noi otteniamo tutti i dati di cui abbiamo bisogno in un colpo solo.

### Il Change Tracker: potente ma costoso

Il Change Tracker è uno dei meccanismi fondamentali di Entity Framework. È come un registratore che tiene traccia di tutte le modifiche che apportiamo alle entità. Quando carichiamo un prodotto e cambiamo il suo prezzo, il Change Tracker annota questa modifica. Quando poi chiamiamo `SaveChanges()`, Entity Framework consulta il Change Tracker per sapere cosa è cambiato e genera le query UPDATE appropriate.

Questo meccanismo è assolutamente fondamentale per il funzionamento di Entity Framework, ma ha un costo. Ogni entità tracciata occupa memoria. Il framework deve mantenere sia lo stato originale che quello corrente di ogni oggetto per poter generare le query di update. In un'applicazione che carica migliaia di record, questo può diventare un problema di memoria significativo.

La buona notizia è che possiamo disabilitare il tracking quando non ci serve. Il metodo `AsNoTracking()` dice a Entity Framework di non tenere traccia delle entità che stiamo caricando:

```csharp
var products = context.Products
    .AsNoTracking()
    .Where(p => p.Price > 100)
    .ToList();
```

Quando dobbiamo usare il tracking? La risposta è semplice: ogni volta che prevediamo di modificare o eliminare le entità. Se dobbiamo fare update o delete, il tracking è necessario. Anche `SaveChanges()` generalmente richiede il tracking.

Quando invece NON dobbiamo usare il tracking? In tutti gli scenari read-only: API GET che restituiscono liste o dettagli, report, statistiche, ricerche, filtri, export di dati. In generale, qualsiasi operazione di sola lettura dovrebbe usare `AsNoTracking()`.

Una nota importante: le query che usano `Select()` per creare proiezioni hanno già `AsNoTracking()` attivo di default, quindi non serve specificarlo esplicitamente.

---

## CRUD Operations

Entity Framework è estremamente utilizzato per operazioni CRUD:

### CREATE

Utilizzare `Add()` o `AddRange()` per aggiungere entità. Invocare `SaveChanges()` per persistere.

#### ❌ MALE - SaveChanges() nel Loop

```csharp
for (int i = 0; i < 1000; i++)
{
    var product = new Product 
    { 
        Name = $"Product {i}", 
        Price = i * 10 
    };
    context.Products.Add(product);
    context.SaveChanges(); // ❌ 1000 transazioni!
}
```

**SQL generato (1000 transazioni separate):**

```sql
-- ITERAZIONE 1
BEGIN;
INSERT INTO "Products" ("Name", "Price")
VALUES ('Product 0', 0)
RETURNING "Id";
COMMIT;

-- ITERAZIONE 2
BEGIN;
INSERT INTO "Products" ("Name", "Price")
VALUES ('Product 1', 10)
RETURNING "Id";
COMMIT;
-- ... 1000 volte!
```

#### ✅ BENE - SaveChanges() Fuori dal Loop

```csharp
for (int i = 0; i < 1000; i++)
{
    var product = new Product 
    { 
        Name = $"Product {i}", 
        Price = i * 10 
    };
    context.Products.Add(product);
}
context.SaveChanges(); // ✅ 1 sola transazione!
```

**SQL generato (1 transazione batch):**

```sql
BEGIN;
INSERT INTO "Products" ("Name", "Price")
VALUES ('Product 0', 0),
       ('Product 1', 10),
       ('Product 2', 20),
       -- ... tutti i 1000 record
       ('Product 999', 9990)
RETURNING "Id";
COMMIT;
```

### READ

Entity Framework offre diversi metodi per recuperare dati.

#### Find()

```csharp
var product = context.Products.Find(1);
```

- Controlla prima il Change Tracker
- Poi interroga il DB (che potrebbe non essere aggiornato)
- Ritorna `null` se non trovato

#### First() / FirstOrDefault()

```csharp
var product = context.Products.First(p => p.Price > 100);
// First() → Exception se non trova
// FirstOrDefault() → null se non trova
```

#### Single() / SingleOrDefault()

```csharp
var product = context.Products.Single(p => p.Name == "Product X");
// Single() → Exception se 0 o >1 risultati
// SingleOrDefault() → Exception se >1 risultati, null se 0
```

#### Where()

```csharp
var products = context.Products
    .Where(p => p.CategoryId == 5)
    .ToList();
```

### READ - Proiezione con Select()

`Select()` permette di caricare solo i dati necessari, migliorando performance.

```csharp
var products = context.Products
    .Where(p => p.CategoryId == 5)
    .Select(p => new 
    {
        p.Id,
        p.Name,
        p.Price
    })
    .ToList();
```

**SQL generato:**

```sql
SELECT "Id", "Name", "Price"
FROM "Products"
WHERE "CategoryId" = 5;
```

> **💡 Tip:** Usare bene `Select()` caricando solo le colonne necessarie aumenta significativamente le performance!

### UPDATE

#### Update con Tracking

Entity Framework traccia automaticamente le modifiche e genera UPDATE solo per le proprietà modificate.

```csharp
// Carica l'entità con tracking
var product = context.Products.First(p => p.Id == 1);

product.Price = 199.99m;
product.Stock = 50;

// Salva - EF genera UPDATE solo per le colonne modificate
context.SaveChanges();
```

**SQL generato:**

```sql
UPDATE "Products" 
SET "Price" = 199.99, 
    "Stock" = 50
WHERE "Id" = 1;
```

#### Update() Esplicito

```csharp
var product = new Product 
{ 
    Id = 1,
    Name = "Nuovo nome",
    Price = 199.99m,
    Description = "Nuova descrizione",
    Stock = 50,
    // ... tutte le altre proprietà
};

context.Products.Update(product);
context.SaveChanges();
```

**SQL generato (TUTTE le colonne):**

```sql
UPDATE "Products" 
SET "Name" = 'Nuovo nome',
    "Price" = 199.99,
    "Description" = 'Nuova descrizione',
    "Stock" = 50,
    "CategoryId" = NULL,
    -- ... TUTTE le colonne!
WHERE "Id" = 1;
```

> **⚠️ IMPORTANTE:** Anche qui, MAI `SaveChanges()` dentro un loop!

#### ExecuteUpdate() (EF7+)

Da EF Core 7 è disponibile `ExecuteUpdate()` per update massivi senza caricare entità in memoria.

```csharp
// Aumenta il prezzo del 10% per tutti i prodotti di una categoria
context.Products
    .Where(p => p.CategoryId == 5)
    .ExecuteUpdate(setters => setters
        .SetProperty(p => p.Price, p => p.Price * 1.10m)
        .SetProperty(p => p.UpdatedAt, DateTime.UtcNow));
```

**SQL generato:**

```sql
UPDATE "Products"
SET "Price" = "Price" * 1.10,
    "UpdatedAt" = '2025-11-08 10:30:00'
WHERE "CategoryId" = 5;
```

### DELETE

#### Remove() con Tracking

```csharp
var product = context.Products.First(p => p.Id == 1);
context.Products.Remove(product);
context.SaveChanges();
```

**SQL generato:**

```sql
DELETE FROM "Products"
WHERE "Id" = 1;
```

> **⚠️ IMPORTANTE:** Anche qui, MAI `SaveChanges()` dentro un loop!

#### ExecuteDelete() (EF7+)

`ExecuteDelete()` elimina record direttamente sul database senza caricarli in memoria.

```csharp
// Elimina tutti i prodotti senza stock
context.Products
    .Where(p => p.Stock == 0)
    .ExecuteDelete();
```

**SQL generato:**

```sql
DELETE FROM "Products"
WHERE "Stock" = 0;
```

---

### Esercizi Consigliati

1. **Setup Iniziale** - Creare un DbContext e collegarlo al database
2. **Code First** - Definire entità e generare la prima migration
3. **CRUD Base** - Implementare operazioni Create, Read, Update, Delete
4. **Query Optimization** - Risolvere un N+1 Query Problem
---


