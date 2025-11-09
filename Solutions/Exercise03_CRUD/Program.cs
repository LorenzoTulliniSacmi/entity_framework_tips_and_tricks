using Exercise03_CRUD.Data;
using Exercise03_CRUD.Models;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("=== OPERAZIONI CRUD - ENTITY FRAMEWORK ===\n");

// Configurazione DbContext (gestita in AppDbContext.OnConfiguring)
using var context = new AppDbContext();
// Applica automaticamente le migrations al database
context.Database.Migrate();

// ========== CREATE ==========
Console.WriteLine("=== CREATE ===");

// 1.1 Inserimento multiplo di categorie (per dimostrare meglio il problema N+1)
var categories = new List<Category>
{
    new Category { Name = "Elettronica", Description = "Prodotti elettronici e tecnologici" },
    new Category { Name = "Libri", Description = "Libri e pubblicazioni" },
    new Category { Name = "Abbigliamento", Description = "Vestiti e accessori" },
    new Category { Name = "Casa", Description = "Articoli per la casa" },
    new Category { Name = "Sport", Description = "Attrezzature sportive" }
};
context.Categories.AddRange(categories);
context.SaveChanges();
Console.WriteLine($"{categories.Count} categorie create\n");

// 1.2 Inserimento multiplo prodotti (BEST PRACTICE) - ognuno con categoria diversa
var products = new List<Product>
{
    new Product { Name = "Laptop", Description = "Laptop da gaming", Price = 999.99m, Stock = 10, CategoryId = categories[0].Id, CreatedAt = DateTime.UtcNow },
    new Product { Name = "Il Signore degli Anelli", Description = "Trilogia completa", Price = 35.99m, Stock = 50, CategoryId = categories[1].Id, CreatedAt = DateTime.UtcNow },
    new Product { Name = "T-Shirt", Description = "T-shirt cotone", Price = 19.99m, Stock = 100, CategoryId = categories[2].Id, CreatedAt = DateTime.UtcNow },
    new Product { Name = "Lampada LED", Description = "Lampada da scrivania", Price = 45.99m, Stock = 30, CategoryId = categories[3].Id, CreatedAt = DateTime.UtcNow },
    new Product { Name = "Pallone da calcio", Description = "Pallone professionale", Price = 29.99m, Stock = 25, CategoryId = categories[4].Id, CreatedAt = DateTime.UtcNow }
};

context.Products.AddRange(products); // Usa AddRange
context.SaveChanges(); // SaveChanges UNA SOLA VOLTA!

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
    Console.WriteLine($"Prodotto trovato con Find(): {foundProduct.Name} - €{foundProduct.Price}");
}

// 2.2 Where() con AsNoTracking()
Console.WriteLine("\nProdotti con prezzo > €100:");
var expensiveProductsQuery = context.Products
    .AsNoTracking() // Read-only, no tracking
    .Where(p => p.Price > 100);

var expensiveProducts = expensiveProductsQuery.ToList();

foreach (var p in expensiveProducts)
{
    Console.WriteLine($"  - {p.Name}: €{p.Price}");
}

// 2.3a ANTI-PATTERN: N+1 Problem (Lazy Loading automatico)
Console.WriteLine("\nANTI-PATTERN - N+1 Problem con Lazy Loading:");
Console.WriteLine("Creazione di un nuovo context per dimostrare il problema N+1...\n");

using var contextForN1 = new AppDbContext();
var productsWithN1Problem = contextForN1.Products.ToList(); // Query 1: carica SOLO i prodotti

foreach (var p in productsWithN1Problem)
{
    // Quando accedi a p.Category, il Lazy Loading fa automaticamente una query separata!
    // Se hai 5 prodotti con 5 categorie diverse = 1 query prodotti + 5 query categorie = 6 query totali
    Console.WriteLine($"  - {p.Name} ({p.Category.Name}): €{p.Price}");
}
Console.WriteLine("ATTENZIONE: Questo ha eseguito 1 + N query (N+1 Problem)! Guarda il log sopra.");

// 2.3b SOLUZIONE: Include() - Eager Loading (evita N+1!)
Console.WriteLine("\nSOLUZIONE - Eager Loading con Include():");
var productsWithCategoryQuery = context.Products
    .Include(p => p.Category) // Carica la categoria in un'unica query
    .AsNoTracking();

var productsWithCategory = productsWithCategoryQuery.ToList();

foreach (var p in productsWithCategory)
{
    Console.WriteLine($"  - {p.Name} ({p.Category.Name}): €{p.Price}");
}
Console.WriteLine("Questo ha eseguito solo 1 query con JOIN!");

// 2.4 Select() - Proiezione
Console.WriteLine("\nProiezione (solo Id, Name, Price):");
var projection = context.Products
    .Select(p => new { p.Id, p.Name, p.Price }) // Carica solo questi campi
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
context.SaveChanges(); // EF aggiorna SOLO le colonne modificate
Console.WriteLine($"Prodotto aggiornato: nuovo prezzo €{productToUpdate.Price}, stock {productToUpdate.Stock}");

// 3.2 Update Esplicito (aggiorna TUTTE le colonne)
var productExplicit = new Product
{
    Id = 2,
    Name = "Il Signore degli Anelli - Edizione Speciale",
    Description = "Trilogia completa con contenuti extra",
    Price = 49.99m,
    Stock = 40,
    CategoryId = categories[1].Id, // Categoria Libri
    CreatedAt = DateTime.UtcNow
};
context.Products.Update(productExplicit); // ATTENZIONE: Aggiorna TUTTE le colonne
context.SaveChanges();
Console.WriteLine($"Update esplicito completato per: {productExplicit.Name}");

// ========== DELETE ==========
Console.WriteLine("\n=== DELETE ===");

var productToDelete = context.Products.First(p => p.Name == "Pallone da calcio");
context.Products.Remove(productToDelete);
context.SaveChanges();
Console.WriteLine($"Prodotto eliminato: {productToDelete.Name}");

// ========== VERIFICA FINALE ==========
Console.WriteLine("\n=== VERIFICA FINALE ===");
var finalCount = context.Products.Count();
Console.WriteLine($"Prodotti rimanenti nel database: {finalCount}");

Console.WriteLine("\nTutte le operazioni CRUD completate con successo!");
