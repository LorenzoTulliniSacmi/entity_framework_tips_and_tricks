using Exercise04_QueryOptimization.Data;
using Exercise04_QueryOptimization.Helpers;
using Exercise04_QueryOptimization.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

// Helper per stampare messaggi colorati (per distinguerli dal log di EF)
void PrintColored(string message, ConsoleColor color = ConsoleColor.Cyan)
{
    Console.ForegroundColor = color;
    Console.WriteLine(message);
    Console.ResetColor();
}

// Configurazione quantità dati di test
const int NUM_CUSTOMERS = 100;
const int NUM_ORDERS = 1000;
const int MEMORY_TEST_SIZE = 100; // Numero di ordini da caricare nel test di confronto memoria

#region Setup Database

// Configurazione DbContext (gestita in AppDbContext.OnConfiguring)
// Prepara database con dati di test
PrintColored("Preparazione database...", ConsoleColor.Yellow);
using (var setupContext = new AppDbContext())
{
    setupContext.Database.EnsureDeleted();
    // Applica automaticamente le migrations al database
    setupContext.Database.Migrate();

    // Crea clienti
    var customers = new List<Customer>();
    for (int i = 1; i <= NUM_CUSTOMERS; i++)
    {
        customers.Add(new Customer
        {
            Name = $"Cliente {i}",
            Email = $"cliente{i}@example.com"
        });
    }
    setupContext.Customers.AddRange(customers);
    setupContext.SaveChanges();

    // Crea ordini
    var orders = new List<Order>();
    var random = new Random(42);
    for (int i = 1; i <= NUM_ORDERS; i++)
    {
        orders.Add(new Order
        {
            OrderDate = DateTime.UtcNow.AddDays(-random.Next(365)),
            TotalAmount = (decimal)(random.NextDouble() * 1000),
            CustomerId = random.Next(1, NUM_CUSTOMERS + 1)
        });
    }
    setupContext.Orders.AddRange(orders);
    setupContext.SaveChanges();
}

PrintColored($"\nDatabase preparato: {NUM_CUSTOMERS} clienti, {NUM_ORDERS} ordini\n", ConsoleColor.Green);
PrintColored("=== DIMOSTRAZIONE PROBLEMA N+1 CON LAZY LOADING ===\n", ConsoleColor.Magenta);

#endregion

#region Versione 1: Anti-Pattern - N+1 Problem con Lazy Loading

// ========== VERSIONE CATTIVA (N+1 Problem) ==========
PrintColored("VERSIONE 1: ANTI-PATTERN - N+1 Problem con Lazy Loading", ConsoleColor.Red);
PrintColored("=".Repeat(60), ConsoleColor.Red);

using (var context = new AppDbContext(enableLogging: true))
{
    var sw = Stopwatch.StartNew();
    int queryCount = 0;

    PrintColored("\nCaricamento ordini senza Include()...", ConsoleColor.Yellow);
    // ANTI-PATTERN: Carica solo gli ordini (1 query)
    var orders = context.Orders.Take(10).ToList();
    queryCount++;

    PrintColored("\nAccesso alle navigation properties (Lazy Loading attivo):", ConsoleColor.Yellow);
    foreach (var order in orders)
    {
        // PROBLEMA: Ogni accesso a Customer.Name genera una query separata tramite Lazy Loading!
        PrintColored($"Ordine #{order.Id} - Cliente: {order.Customer.Name} - €{order.TotalAmount:F2}", ConsoleColor.DarkYellow);
        queryCount++;
    }

    sw.Stop();
    PrintColored($"\nATTENZIONE - Query eseguite: {queryCount} (1 query ordini + 10 query clienti = 11 query totali!)", ConsoleColor.Red);
    PrintColored($"Tempo impiegato: {sw.ElapsedMilliseconds}ms", ConsoleColor.Red);
}

PrintColored("\n" + "=".Repeat(60) + "\n", ConsoleColor.Gray);

#endregion

#region Versione 2: Soluzione - Include() per Eager Loading

// ========== VERSIONE BUONA (Include) ==========
PrintColored("VERSIONE 2: SOLUZIONE - Include() per Eager Loading", ConsoleColor.Green);
PrintColored("=".Repeat(60), ConsoleColor.Green);

using (var context = new AppDbContext(enableLogging: true))
{
    var sw = Stopwatch.StartNew();

    PrintColored("\nCaricamento ordini CON Include()...", ConsoleColor.Cyan);
    // SOLUZIONE: Usa Include() per caricare i clienti in un'unica query
    var orders = context.Orders
        .Include(o => o.Customer) // Eager Loading - carica tutto in una query
        .Take(10)
        .ToList();

    PrintColored("\nAccesso alle navigation properties (già caricate):", ConsoleColor.Cyan);
    foreach (var order in orders)
    {
        // NESSUNA query aggiuntiva: i dati Customer sono già stati caricati!
        PrintColored($"Ordine #{order.Id} - Cliente: {order.Customer.Name} - €{order.TotalAmount:F2}", ConsoleColor.Cyan);
    }

    sw.Stop();
    PrintColored($"\nQuery eseguite: 1 (singola query con JOIN)", ConsoleColor.Green);
    PrintColored($"Tempo impiegato: {sw.ElapsedMilliseconds}ms", ConsoleColor.Green);
}

PrintColored("\n" + "=".Repeat(60) + "\n", ConsoleColor.Gray);

#endregion

#region Versione 3: Ottimizzazione - Select() + AsNoTracking()

// ========== VERSIONE OTTIMIZZATA (Select + AsNoTracking) ==========
PrintColored("VERSIONE 3: OTTIMIZZAZIONE - Select() + AsNoTracking()", ConsoleColor.Blue);
PrintColored("=".Repeat(60), ConsoleColor.Blue);

using (var context = new AppDbContext(enableLogging: true))
{
    var sw = Stopwatch.StartNew();

    PrintColored("\nCaricamento con proiezione (solo campi necessari)...", ConsoleColor.Cyan);
    // OTTIMIZZAZIONE: Carica solo i campi necessari
    var orders = context.Orders
        .Select(o => new
        {
            OrderId = o.Id,
            o.OrderDate,
            o.TotalAmount,
            CustomerName = o.Customer.Name // Include automatico nella proiezione
        })
        .AsNoTracking() // No tracking overhead
        .Take(10)
        .ToList();

    PrintColored("\nDati proiettati (anonymous type):", ConsoleColor.Cyan);
    foreach (var order in orders)
    {
        PrintColored($"Ordine #{order.OrderId} - Cliente: {order.CustomerName} - €{order.TotalAmount:F2}", ConsoleColor.Cyan);
    }

    sw.Stop();
    PrintColored($"\nQuery eseguite: 1 (solo campi richiesti)", ConsoleColor.Blue);
    PrintColored($"Tempo impiegato: {sw.ElapsedMilliseconds}ms", ConsoleColor.Blue);
    PrintColored($"Memoria: ridotta (no tracking, no entità complete)", ConsoleColor.Blue);
}

PrintColored("\n" + "=".Repeat(60) + "\n", ConsoleColor.Gray);

#endregion

#region Versione 4: Paginazione con Include() e AsNoTracking()

// ========== PAGINAZIONE ==========
PrintColored("VERSIONE 4: PAGINAZIONE con Include() e AsNoTracking()", ConsoleColor.Magenta);
PrintColored("=".Repeat(60), ConsoleColor.Magenta);

using (var context = new AppDbContext())
{
    int pageSize = 10;
    int pageNumber = 1;

    var orders = context.Orders
        .Include(o => o.Customer) // Evita N+1 Problem
        .OrderBy(o => o.OrderDate)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .AsNoTracking()
        .ToList();

    PrintColored($"\nPagina {pageNumber} (10 ordini per pagina):", ConsoleColor.Cyan);
    foreach (var order in orders)
    {
        PrintColored($"Ordine #{order.Id} del {order.OrderDate:dd/MM/yyyy} - Cliente: {order.Customer.Name}", ConsoleColor.Cyan);
    }

    var totalOrders = context.Orders.Count();
    var totalPages = (int)Math.Ceiling(totalOrders / (double)pageSize);
    PrintColored($"\nPagina {pageNumber} di {totalPages} - Totale ordini: {totalOrders}", ConsoleColor.Magenta);
}

PrintColored("\n" + "=".Repeat(60) + "\n", ConsoleColor.Gray);

#endregion

#region Versione 5: Confronto Memoria - Lazy vs Eager vs Select

// ========== CONFRONTO MEMORIA: LAZY vs EAGER vs SELECT ==========
PrintColored("VERSIONE 7: CONFRONTO MEMORIA - Lazy vs Eager vs Select", ConsoleColor.Magenta);
PrintColored("=".Repeat(60), ConsoleColor.Magenta);

PrintColored($"\nCaricamento di {MEMORY_TEST_SIZE} ordini con 3 approcci diversi...\n", ConsoleColor.Cyan);

// Test 1: Lazy Loading (con tracking)
long memoryLazy, memoryEager, memorySelect;
int trackedEntitiesLazy, trackedEntitiesEager, trackedEntitiesSelect;

PrintColored("TEST 1: Lazy Loading (N+1 Problem)", ConsoleColor.Yellow);
GC.Collect();
GC.WaitForPendingFinalizers();
GC.Collect();
var memoryBefore = GC.GetTotalMemory(false);

using (var context = new AppDbContext())
{
    var orders = context.Orders.Take(MEMORY_TEST_SIZE).ToList();

    // Accesso alle navigation properties (trigger lazy loading)
    foreach (var order in orders)
    {
        _ = order.Customer.Name; // Trigger query separate per ogni ordine!
    }

    trackedEntitiesLazy = context.ChangeTracker.Entries().Count();
}

memoryLazy = GC.GetTotalMemory(false) - memoryBefore;
PrintColored($"  Memoria utilizzata: {memoryLazy / 1024.0:F2} KB", ConsoleColor.Yellow);
PrintColored($"  Entità tracciate: {trackedEntitiesLazy}", ConsoleColor.Yellow);
PrintColored($"  Query eseguite: {1 + MEMORY_TEST_SIZE} (1 + {MEMORY_TEST_SIZE})", ConsoleColor.Red);

// Test 2: Eager Loading con Include (con tracking)
PrintColored("\nTEST 2: Eager Loading con Include() + Tracking", ConsoleColor.Yellow);
GC.Collect();
GC.WaitForPendingFinalizers();
GC.Collect();
memoryBefore = GC.GetTotalMemory(false);

using (var context = new AppDbContext())
{
    var orders = context.Orders
        .Include(o => o.Customer)
        .Take(MEMORY_TEST_SIZE)
        .ToList();

    // Accesso alle navigation properties (già caricate)
    foreach (var order in orders)
    {
        _ = order.Customer.Name; // Nessuna query aggiuntiva
    }

    trackedEntitiesEager = context.ChangeTracker.Entries().Count();
}

memoryEager = GC.GetTotalMemory(false) - memoryBefore;
PrintColored($"  Memoria utilizzata: {memoryEager / 1024.0:F2} KB", ConsoleColor.Yellow);
PrintColored($"  Entità tracciate: {trackedEntitiesEager}", ConsoleColor.Yellow);
PrintColored($"  Query eseguite: 1 (con JOIN)", ConsoleColor.Green);

// Test 3: Select con AsNoTracking (proiezione)
PrintColored("\nTEST 3: Select() + AsNoTracking() (proiezione)", ConsoleColor.Cyan);
GC.Collect();
GC.WaitForPendingFinalizers();
GC.Collect();
memoryBefore = GC.GetTotalMemory(false);

using (var context = new AppDbContext())
{
   var ordersquery = context.Orders
       .Select(o => new
       {
          o.Id,
          o.OrderDate,
          o.TotalAmount,
          CustomerName = o.Customer.Name
       })
       .AsNoTracking()
       .Take(MEMORY_TEST_SIZE);
   var orders = ordersquery.ToList();

    foreach (var order in orders)
    {
        _ = order.CustomerName;
    }

    trackedEntitiesSelect = context.ChangeTracker.Entries().Count();
}

memorySelect = GC.GetTotalMemory(false) - memoryBefore;
PrintColored($"  Memoria utilizzata: {memorySelect / 1024.0:F2} KB", ConsoleColor.Cyan);
PrintColored($"  Entità tracciate: {trackedEntitiesSelect}", ConsoleColor.Cyan);
PrintColored($"  Query eseguite: 1 (solo campi necessari)", ConsoleColor.Green);

// Confronto finale
PrintColored("\nCONFRONTO FINALE:", ConsoleColor.Magenta);
PrintColored("=".Repeat(60), ConsoleColor.Magenta);

var baselineMemory = Math.Min(Math.Min(memoryLazy, memoryEager), memorySelect);
PrintColored($"\n1. Lazy Loading:", ConsoleColor.Yellow);
PrintColored($"   Memoria: {memoryLazy / 1024.0:F2} KB ({(memoryLazy / (double)baselineMemory):F2}x baseline)", ConsoleColor.Yellow);
PrintColored($"   Entità tracciate: {trackedEntitiesLazy}", ConsoleColor.Yellow);
PrintColored($"   Performance: PESSIMA ({1 + MEMORY_TEST_SIZE} query)", ConsoleColor.Red);

PrintColored($"\n2. Include() + Tracking:", ConsoleColor.Yellow);
PrintColored($"   Memoria: {memoryEager / 1024.0:F2} KB ({(memoryEager / (double)baselineMemory):F2}x baseline)", ConsoleColor.Yellow);
PrintColored($"   Entità tracciate: {trackedEntitiesEager}", ConsoleColor.Yellow);
PrintColored($"   Performance: BUONA (1 query)", ConsoleColor.Green);

PrintColored($"\n3. Select() + AsNoTracking():", ConsoleColor.Cyan);
PrintColored($"   Memoria: {memorySelect / 1024.0:F2} KB ({(memorySelect / (double)baselineMemory):F2}x baseline)", ConsoleColor.Cyan);
PrintColored($"   Entità tracciate: {trackedEntitiesSelect} (no tracking)", ConsoleColor.Cyan);
PrintColored($"   Performance: OTTIMA (1 query + meno memoria)", ConsoleColor.Green);

PrintColored("\n" + "=".Repeat(60) + "\n", ConsoleColor.Gray);

#endregion
