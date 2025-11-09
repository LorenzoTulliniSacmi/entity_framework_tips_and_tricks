using Microsoft.EntityFrameworkCore;

// Configurazione e test della connessione
var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

// Configurazione PostgreSQL (richiede Docker - vedi README)
optionsBuilder.UseNpgsql("Host=localhost;Database=ef_lab_setup;Username=efuser;Password=efpass");

// Alternativa SQLite (se non hai Docker, commenta PostgreSQL e decommenta questa riga)
// optionsBuilder.UseSqlite("Data Source=app.db");

using var context = new AppDbContext(optionsBuilder.Options);

// Crea il database
context.Database.EnsureCreated();
Console.WriteLine("Database creato con successo!");

// Testa la connessione
bool canConnect = context.Database.CanConnect();
Console.WriteLine($"Connessione al database: {(canConnect ? "OK" : "ERRORE")}");

// Classe DbContext
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
}
