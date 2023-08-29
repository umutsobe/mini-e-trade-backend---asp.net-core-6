using e_trade_api.application;
using e_trade_api.domain;
using e_trade_api.domain.Entities;

namespace e_trade_api.Persistence;

public class CategoryService : ICategoryService
{
    readonly ICategoryReadRepository _categoryReadRepository;
    readonly ICategoryWriteRepository _categoryWriteRepository;

    public CategoryService(
        ICategoryReadRepository categoryReadRepository,
        ICategoryWriteRepository categoryWriteRepository
    )
    {
        _categoryReadRepository = categoryReadRepository;
        _categoryWriteRepository = categoryWriteRepository;
    }

    public async Task CreateCategory(string name)
    {
        //isim kontrolü. isim daha önce yüklenmiş mi
        Category? category = _categoryReadRepository.Table.FirstOrDefault(c => c.Name == name);

        if (category != null)
            throw new Exception("Ürün Daha Önce Eklendi. Farklı Bir İsim Deneyin");
        else
        {
            await _categoryWriteRepository.AddAsync(new() { Name = name });
            await _categoryWriteRepository.SaveAsync();
        }
    }

    public async Task DeleteCategory(string id)
    {
        await _categoryWriteRepository.RemoveAsync(id);
        await _categoryWriteRepository.SaveAsync();
    }

    public async Task<GetAllCategoriesDTO> GetAllCategories(GetAllCategoriesRequestDTO model)
    {
        int totalCategoryCount = _categoryReadRepository.GetAll().Count();

        var categories = _categoryReadRepository
            .GetAll()
            .Skip(model.Page * model.Size)
            .Take(model.Size)
            .Select(
                c =>
                    new
                    {
                        c.Name,
                        c.Id,
                        c.CreatedDate,
                        c.UpdatedDate
                    }
            )
            .ToList();

        GetAllCategoriesDTO categoriesDTO = new();
        categoriesDTO.Categories = new();

        categoriesDTO.totalCategoryCount = totalCategoryCount;

        foreach (var category in categories)
        {
            CategoryDTO categoryDTO = new() { Name = category.Name, Id = category.Id.ToString(), };

            categoriesDTO.Categories.Add(categoryDTO);
        }

        return categoriesDTO;
    }

    public async Task UpdateCategoryName(UpdateCategoryNameDTO model)
    {
        Category category = await _categoryReadRepository.GetByIdAsync(model.Id);
        category.Name = model.NewName;

        await _categoryWriteRepository.SaveAsync();
    }
}
