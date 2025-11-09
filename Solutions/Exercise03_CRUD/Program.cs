using Exercise03_CRUD.Data;
using Exercise03_CRUD.Models;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("=== OPERAZIONI CRUD - ENTITY FRAMEWORK ===\n");

// Configurazione DbContext
var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

// Configurazione PostgreSQL (richiede Docker - vedi README)
optionsBuilder.UseNpgsql("Host=localhost;Database=ef_lab_crud;Username=efuser;Password=efpass");

// Alternativa SQLite (se non hai Docker, commenta PostgreSQL e decommenta questa riga)
// optionsBuilder.UseSqlite("Data Source=crud_demo.db");

using var context = new AppDbContext(optionsBuilder.Options);
context.Database.EnsureCreated();

// ========== CREATE ==========
Console.WriteLine("=== CREATE ===");

// 1.1 Inserimento singolo
var electronics = new Category
{
    Name = "Elettronica",
    Description = "Prodotti elettronici e tecnologici"
};
context.Categories.Add(electronics);
context.SaveChanges();
Console.WriteLine($"✓ Categoria creata con Id: {electronics.Id}\n");

// 1.2 Inserimento multiplo (BEST PRACTICE)
var products = new List<Product>
{
    new Product { Name = "Laptop", Description = "Laptop da gaming", Price = 999.99m, Stock = 10, CategoryId = electronics.Id, CreatedAt = DateTime.Now },
    new Product { Name = "Mouse", Description = "Mouse wireless", Price = 25.99m, Stock = 50, CategoryId = electronics.Id, CreatedAt = DateTime.Now },
    new Product { Name = "Tastiera", Description = "Tastiera meccanica", Price = 89.99m, Stock = 30, CategoryId = electronics.Id, CreatedAt = DateTime.Now },
    new Product { Name = "Monitor", Description = "Monitor 27 pollici", Price = 299.99m, Stock = 15, CategoryId = electronics.Id, CreatedAt = DateTime.Now },
    new Product { Name = "Webcam", Description = "Webcam HD", Price = 49.99m, Stock = 25, CategoryId = electronics.Id, CreatedAt = DateTime.Now }
};

context.Products.AddRange(products); // ✅ Usa AddRange
context.SaveChanges(); // ✅ SaveChanges UNA SOLA VOLTA!

Console.WriteLine("Prodotti creati:");
foreach (var p in products)
{
    Console.WriteLine($"  - Id: {p.Id}, Nome: {p.Name}, Prezzo: €{p.Price}");
}

// ========== READ ==========
Console.WriteLine("\n=== READ ===");

// 2.1 Find()
var foundProduct = context.Products.Find(1);
if (foundProduct != null)
{
    Console.WriteLine($"✓ Prodotto trovato con Find(): {foundProduct.Name} - €{foundProduct.Price}");
}

// 2.2 Where() con AsNoTracking()
Console.WriteLine("\nProdotti con prezzo > €100:");
var expensiveProducts = context.Products
    .AsNoTracking() // ✅ Read-only, no tracking
    .Where(p => p.Price > 100)
    .ToList();

foreach (var p in expensiveProducts)
{
    Console.WriteLine($"  - {p.Name}: €{p.Price}");
}

// 2.3 Include() - Eager Loading (evita N+1!)
Console.WriteLine("\nProdotti con categoria (Eager Loading):");
var productsWithCategory = context.Products
    .Include(p => p.Category) // ✅ Carica la categoria in un'unica query
    .AsNoTracking()
    .ToList();

foreach (var p in productsWithCategory)
{
    Console.WriteLine($"  - {p.Name} ({p.Category.Name}): €{p.Price}");
}

// 2.4 Select() - Proiezione
Console.WriteLine("\nProiezione (solo Id, Name, Price):");
var projection = context.Products
    .Select(p => new { p.Id, p.Name, p.Price }) // ✅ Carica solo questi campi
    .AsNoTracking()
    .ToList();

foreach (var p in projection)
{
    Console.WriteLine($"  - {p.Id}: {p.Name} - €{p.Price}");
}

// ========== UPDATE ==========
Console.WriteLine("\n=== UPDATE ===");

// 3.1 Update con Tracking
var productToUpdate = context.Products.First(p => p.Name == "Laptop");
productToUpdate.Price = 899.99m; // Modifica
productToUpdate.Stock = 50; // Modifica
context.SaveChanges(); // ✅ EF aggiorna SOLO le colonne modificate
Console.WriteLine($"✓ Prodotto aggiornato: nuovo prezzo €{productToUpdate.Price}, stock {productToUpdate.Stock}");

// 3.2 Update Esplicito (aggiorna TUTTE le colonne)
var productExplicit = new Product
{
    Id = 2,
    Name = "Mouse Premium",
    Description = "Mouse wireless premium",
    Price = 35.99m,
    Stock = 40,
    CategoryId = electronics.Id,
    CreatedAt = DateTime.Now
};
context.Products.Update(productExplicit); // ⚠️ Aggiorna TUTTE le colonne
context.SaveChanges();
Console.WriteLine($"✓ Update esplicito completato per: {productExplicit.Name}");

// ========== DELETE ==========
Console.WriteLine("\n=== DELETE ===");

var productToDelete = context.Products.First(p => p.Name == "Webcam");
context.Products.Remove(productToDelete);
context.SaveChanges();
Console.WriteLine($"✓ Prodotto eliminato: {productToDelete.Name}");

// ========== VERIFICA FINALE ==========
Console.WriteLine("\n=== VERIFICA FINALE ===");
var finalCount = context.Products.Count();
Console.WriteLine($"Prodotti rimanenti nel database: {finalCount}");

Console.WriteLine("\n✓ Tutte le operazioni CRUD completate con successo!");
