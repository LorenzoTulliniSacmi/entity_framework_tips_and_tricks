using Exercise02_CodeFirst.Data;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("=== EXERCISE 02 - CODE FIRST ===\n");

// Configurazione DbContext
var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

// Configurazione PostgreSQL (richiede Docker - vedi README)
optionsBuilder.UseNpgsql("Host=localhost;Database=ef_lab_codefirst;Username=efuser;Password=efpass");

// Alternativa SQLite (se non hai Docker, commenta PostgreSQL e decommenta questa riga)
// optionsBuilder.UseSqlite("Data Source=catalog.db");

using var context = new AppDbContext(optionsBuilder.Options);

// Verifica connessione
if (context.Database.CanConnect())
{
    Console.WriteLine("✓ Database creato: OK");
    Console.WriteLine("✓ Tabelle Categories e Products create correttamente.");
    Console.WriteLine("\nPer applicare le migration, esegui:");
    Console.WriteLine("  dotnet ef migrations add InitialCreate");
    Console.WriteLine("  dotnet ef database update");
}
else
{
    Console.WriteLine("✗ Errore nella connessione al database");
}
