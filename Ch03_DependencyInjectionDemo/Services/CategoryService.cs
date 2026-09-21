using Ch03_DependencyInjectionDemo.Entities;
using Ch03_DependencyInjectionDemo.Repositories;

namespace Ch03_DependencyInjectionDemo.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;

    public CategoryService(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public Task<List<Category>> GetAllAsync()
        => _repository.GetAllAsync();

    public Task<Category?> GetByIdAsync(int id)
        => _repository.GetByIdAsync(id);

    public Task AddAsync(Category category)
        => _repository.AddAsync(category);

    public Task UpdateAsync(Category category)
        => _repository.UpdateAsync(category);

    public Task DeleteAsync(int id)
        => _repository.DeleteAsync(id);
}