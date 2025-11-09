namespace Exercise04_QueryOptimization.Models;

public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public int CustomerId { get; set; }

    // virtual abilita il Lazy Loading (carica Customer solo quando accedi alla proprietà)
    public virtual Customer Customer { get; set; } = null!;
}
