using FinanceOne.Api.Features.Categories.CreateCategory;
using FinanceOne.Api.Features.Categories.DeleteCategory;
using FinanceOne.Api.Features.Categories.GetCategories;
using FinanceOne.Api.Features.Categories.GetCategoryById;
using FinanceOne.Api.Features.Categories.UpdateCategory;

namespace FinanceOne.Api.Features.Categories;

public static class CategoriesEndpoints
{
    public static void MapCategoriesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories").WithTags("Categories");

        group.MapCreateCategory();
        group.MapGetCategories();
        group.MapGetCategoryById();
        group.MapUpdateCategory();
        group.MapDeleteCategory();
    }
}
