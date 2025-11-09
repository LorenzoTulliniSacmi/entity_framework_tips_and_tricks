namespace Exercise04_QueryOptimization.Models;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // virtual abilita il Lazy Loading (carica Orders solo quando accedi alla proprietà)
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
