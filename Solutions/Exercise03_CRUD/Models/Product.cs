namespace Exercise03_CRUD.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CategoryId { get; set; }

    // virtual per abilitare Lazy Loading
    public virtual Category Category { get; set; } = null!;
}
