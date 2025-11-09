using Exercise02_CodeFirst.Data;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("=== EXERCISE 02 - CODE FIRST ===\n");

// Comandi utili:
// - Creare una migration: dotnet ef migrations add NomeMigration
// - Applicare le migration: dotnet ef database update (verranno applicate in automatico)
// - Rimuovere l'ultimo migration: dotnet ef migrations remove
// - Generare lo script .sql di migrazione: dotnet ef migrations script -o migration.sql

// Configurazione DbContext (gestita in AppDbContext.OnConfiguring)
using var context = new AppDbContext();

// Applica automaticamente le migrations al database
context.Database.Migrate();
Console.WriteLine("✓ Database aggiornato con migrations");

// Verifica connessione
if (context.Database.CanConnect())
{
   Console.WriteLine("✓ Tabelle Categories e Products create correttamente.");
   Console.WriteLine("\nNota: Le migrations vengono applicate automaticamente all'avvio.");
   Console.WriteLine("Per creare nuove migration, esegui:");
   Console.WriteLine("  dotnet ef migrations add NomeMigration");
}
else
{
   Console.WriteLine("✗ Errore nella connessione al database");
}
