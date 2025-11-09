namespace Exercise02_CodeFirst.Models;

public class Category
{
   public int Id { get; set; }
   public string Name { get; set; } = string.Empty;
   public string? Description { get; set; }

   // Relazione 1:m con Product
   public ICollection<Product> Products { get; set; } = new List<Product>();
}
