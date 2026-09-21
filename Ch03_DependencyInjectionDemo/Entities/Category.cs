using System.ComponentModel.DataAnnotations;

namespace Ch03_DependencyInjectionDemo.Entities;

public class Category
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = default!;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
