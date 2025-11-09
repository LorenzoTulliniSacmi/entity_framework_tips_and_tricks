using Exercise04_QueryOptimization.Data;
using Exercise04_QueryOptimization.Helpers;
using Exercise04_QueryOptimization.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

// Configurazione PostgreSQL (richiede Docker - vedi README)
optionsBuilder.UseNpgsql("Host=localhost;Database=ef_lab_orders;Username=efuser;Password=efpass");

// Alternativa SQLite (se non hai Docker, commenta PostgreSQL e decommenta questa riga)
// optionsBuilder.UseSqlite("Data Source=orders.db");

// Prepara database con dati di test
Console.WriteLine("Preparazione database...");
using (var setupContext = new AppDbContext(optionsBuilder.Options))
{
    setupContext.Database.EnsureDeleted();
    setupContext.Database.EnsureCreated();

    // Crea 100 clienti
    var customers = new List<Customer>();
    for (int i = 1; i <= 100; i++)
    {
        customers.Add(new Customer
        {
            Name = $"Cliente {i}",
            Email = $"cliente{i}@example.com"
        });
    }
    setupContext.Customers.AddRange(customers);
    setupContext.SaveChanges();

    // Crea 1000 ordini
    var orders = new List<Order>();
    var random = new Random(42);
    for (int i = 1; i <= 1000; i++)
    {
        orders.Add(new Order
        {
            OrderDate = DateTime.Now.AddDays(-random.Next(365)),
            TotalAmount = (decimal)(random.NextDouble() * 1000),
            CustomerId = random.Next(1, 101)
        });
    }
    setupContext.Orders.AddRange(orders);
    setupContext.SaveChanges();
}

Console.WriteLine("✓ Database preparato: 100 clienti, 1000 ordini\n");
Console.WriteLine("=== DIMOSTRAZIONE PROBLEMA N+1 ===\n");

// ========== VERSIONE CATTIVA (N+1 Problem) ==========
Console.WriteLine("1️⃣  VERSIONE CATTIVA (N+1 Problem)");
Console.WriteLine("=".Repeat(50));

using (var context = new AppDbContext(optionsBuilder.Options, enableLogging: true))
{
    var sw = Stopwatch.StartNew();
    int queryCount = 0;

    // ❌ Carica solo gli ordini (1 query)
    var orders = context.Orders.Take(10).ToList();
    queryCount++;

    Console.WriteLine("\n--- Ordini con clienti (N+1 Problem) ---");
    foreach (var order in orders)
    {
        // ❌ Ogni accesso a Customer.Name genera una query separata!
        Console.WriteLine($"Ordine #{order.Id} - Cliente: {order.Customer.Name} - €{order.TotalAmount:F2}");
        queryCount++;
    }

    sw.Stop();
    Console.WriteLine($"\n⚠️  Query eseguite: {queryCount} (1 + 10 = 11)");
    Console.WriteLine($"⏱️  Tempo: {sw.ElapsedMilliseconds}ms");
}

Console.WriteLine("\n" + "=".Repeat(50) + "\n");

// ========== VERSIONE BUONA (Include) ==========
Console.WriteLine("2️⃣  VERSIONE BUONA (Include - Eager Loading)");
Console.WriteLine("=".Repeat(50));

using (var context = new AppDbContext(optionsBuilder.Options, enableLogging: true))
{
    var sw = Stopwatch.StartNew();

    // ✅ Usa Include() per caricare i clienti in un'unica query
    var orders = context.Orders
        .Include(o => o.Customer) // ✅ Eager Loading
        .Take(10)
        .ToList();

    Console.WriteLine("\n--- Ordini con clienti (Include) ---");
    foreach (var order in orders)
    {
        // ✅ Nessuna query aggiuntiva, dati già caricati!
        Console.WriteLine($"Ordine #{order.Id} - Cliente: {order.Customer.Name} - €{order.TotalAmount:F2}");
    }

    sw.Stop();
    Console.WriteLine($"\n✅ Query eseguite: 1 (singola query con JOIN)");
    Console.WriteLine($"⏱️  Tempo: {sw.ElapsedMilliseconds}ms");
}

Console.WriteLine("\n" + "=".Repeat(50) + "\n");

// ========== VERSIONE OTTIMIZZATA (Select + AsNoTracking) ==========
Console.WriteLine("3️⃣  VERSIONE OTTIMIZZATA (Select + AsNoTracking)");
Console.WriteLine("=".Repeat(50));

using (var context = new AppDbContext(optionsBuilder.Options, enableLogging: true))
{
    var sw = Stopwatch.StartNew();

    // ✅ Carica solo i campi necessari
    var orders = context.Orders
        .Select(o => new
        {
            OrderId = o.Id,
            o.OrderDate,
            o.TotalAmount,
            CustomerName = o.Customer.Name // ✅ Include automatico nella proiezione
        })
        .AsNoTracking() // ✅ No tracking overhead
        .Take(10)
        .ToList();

    Console.WriteLine("\n--- Ordini con clienti (Select + AsNoTracking) ---");
    foreach (var order in orders)
    {
        Console.WriteLine($"Ordine #{order.OrderId} - Cliente: {order.CustomerName} - €{order.TotalAmount:F2}");
    }

    sw.Stop();
    Console.WriteLine($"\n✅ Query eseguite: 1");
    Console.WriteLine($"⏱️  Tempo: {sw.ElapsedMilliseconds}ms");
    Console.WriteLine($"💾 Memoria: minore (solo dati necessari, no tracking)");
}

Console.WriteLine("\n" + "=".Repeat(50) + "\n");

// ========== PAGINAZIONE ==========
Console.WriteLine("4️⃣  PAGINAZIONE CON INCLUDE");
Console.WriteLine("=".Repeat(50));

using (var context = new AppDbContext(optionsBuilder.Options))
{
    int pageSize = 10;
    int pageNumber = 1;

    var orders = context.Orders
        .Include(o => o.Customer) // ✅ Evita N+1
        .OrderBy(o => o.OrderDate)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .AsNoTracking()
        .ToList();

    Console.WriteLine($"\n--- Pagina {pageNumber} (10 ordini) ---");
    foreach (var order in orders)
    {
        Console.WriteLine($"Ordine #{order.Id} del {order.OrderDate:dd/MM/yyyy} - Cliente: {order.Customer.Name}");
    }

    var totalOrders = context.Orders.Count();
    var totalPages = (int)Math.Ceiling(totalOrders / (double)pageSize);
    Console.WriteLine($"\n📄 Pagina {pageNumber} di {totalPages} - Totale ordini: {totalOrders}");
}

Console.WriteLine("\n" + "=".Repeat(50) + "\n");

// ========== RIEPILOGO ==========
Console.WriteLine("📊 RIEPILOGO");
Console.WriteLine("=".Repeat(50));
Console.WriteLine("❌ N+1 Problem: 1 + N query (MOLTO LENTO)");
Console.WriteLine("✅ Include(): 1 query con JOIN (VELOCE)");
Console.WriteLine("✅ Select() + AsNoTracking(): 1 query ottimizzata (PIÙ VELOCE)");
Console.WriteLine("\n💡 Usa sempre Include() o Select() quando accedi a navigation properties!");
