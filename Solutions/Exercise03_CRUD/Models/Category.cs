namespace Exercise03_CRUD.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // virtual per abilitare Lazy Loading
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
