using MoneyManagement.Application.Common;
using MoneyManagement.Application.Features.Categories.Dto;
using MoneyManagement.Application.Features.Categories.Interfaces;
using MoneyManagement.Application.Features.Categories.Requests;
using MoneyManagement.Application.Interfaces.UnitOfWork;
using MoneyManagement.Domain.Entities;
using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.Features.Categories.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categories;
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(ICategoryRepository categories, IUnitOfWork unitOfWork)
    {
        _categories = categories;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<CategoryDto>>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var categories = await _categories.GetAllForUserAsync(userId, cancellationToken);

        if (categories.Count == 0)
        {
            var seeded = BuildDefaultCategories(userId);
            await _categories.AddRangeAsync(seeded, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            categories = seeded;
        }

        var tree = categories
            .Where(c => c.ParentId is null)
            .OrderBy(c => c.SortOrder)
            .Select(c => MapToDto(c, categories))
            .ToList();

        return Result<IReadOnlyList<CategoryDto>>.Success(tree);
    }

    public async Task<Result<CategoryDto>> CreateAsync(Guid userId, CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();

        if (request.ParentId is Guid parentId)
        {
            var parent = await _categories.GetByIdForUserAsync(userId, parentId, cancellationToken);
            if (parent is null)
                return Result<CategoryDto>.Failure("Parent category not found.", ErrorStatus.NotFound);

            if (parent.ParentId is not null)
                return Result<CategoryDto>.Failure("Sub-categories can only be one level deep.", ErrorStatus.ValidationError);

            if (parent.Type != request.Type)
                return Result<CategoryDto>.Failure("A sub-category must have the same type as its parent.", ErrorStatus.ValidationError);
        }

        var duplicate = await _categories.NameExistsAsync(userId, request.ParentId, name, cancellationToken: cancellationToken);
        if (duplicate)
            return Result<CategoryDto>.Failure("A category with this name already exists.", ErrorStatus.Duplicate);

        var siblings = await _categories.GetSiblingsAsync(userId, request.ParentId, cancellationToken);
        var sortOrder = siblings.Count == 0 ? 0 : siblings.Max(c => c.SortOrder) + 1;

        var category = new Category
        {
            UserId = userId,
            ParentId = request.ParentId,
            Name = name,
            Type = request.Type,
            Color = request.Color,
            Icon = request.Icon,
            SortOrder = sortOrder,
            CreatedBy = userId,
        };

        await _categories.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CategoryDto>.Success(MapToDto(category, [category]), "Category created.");
    }

    public async Task<Result<CategoryDto>> UpdateAsync(Guid userId, Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await _categories.GetByIdForUserAsync(userId, id, cancellationToken);
        if (category is null)
            return Result<CategoryDto>.Failure("Category not found.", ErrorStatus.NotFound);

        var name = request.Name.Trim();

        var duplicate = await _categories.NameExistsAsync(userId, category.ParentId, name, id, cancellationToken);
        if (duplicate)
            return Result<CategoryDto>.Failure("A category with this name already exists.", ErrorStatus.Duplicate);

        category.Name = name;
        category.Color = request.Color;
        category.Icon = request.Icon;
        category.UpdatedBy = userId;
        category.UpdatedAtUtc = DateTime.UtcNow;

        _categories.Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CategoryDto>.Success(MapToDto(category, [category]), "Category updated.");
    }

    public async Task<Result<CategoryDto>> ArchiveAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _categories.GetByIdForUserAsync(userId, id, cancellationToken);
        if (category is null)
            return Result<CategoryDto>.Failure("Category not found.", ErrorStatus.NotFound);

        if (category.Active)
        {
            category.Active = false;
            category.UpdatedBy = userId;
            category.UpdatedAtUtc = DateTime.UtcNow;
            _categories.Update(category);

            var children = await _categories.GetActiveChildrenAsync(id, cancellationToken);
            foreach (var child in children)
            {
                child.Active = false;
                child.UpdatedBy = userId;
                child.UpdatedAtUtc = DateTime.UtcNow;
                _categories.Update(child);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result<CategoryDto>.Success(MapToDto(category, [category]), "Category archived.");
    }

    public async Task<Result<CategoryDto>> RestoreAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _categories.GetByIdForUserAsync(userId, id, cancellationToken);
        if (category is null)
            return Result<CategoryDto>.Failure("Category not found.", ErrorStatus.NotFound);

        if (!category.Active)
        {
            var duplicate = await _categories.NameExistsAsync(userId, category.ParentId, category.Name, id, cancellationToken);
            if (duplicate)
                return Result<CategoryDto>.Failure("Another active category already uses this name.", ErrorStatus.Duplicate);

            category.Active = true;
            category.UpdatedBy = userId;
            category.UpdatedAtUtc = DateTime.UtcNow;
            _categories.Update(category);

            if (category.ParentId is Guid parentId)
            {
                var parent = await _categories.GetByIdForUserAsync(userId, parentId, cancellationToken);
                if (parent is not null && !parent.Active)
                {
                    parent.Active = true;
                    parent.UpdatedBy = userId;
                    parent.UpdatedAtUtc = DateTime.UtcNow;
                    _categories.Update(parent);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result<CategoryDto>.Success(MapToDto(category, [category]), "Category restored.");
    }

    private static CategoryDto MapToDto(Category category, IReadOnlyList<Category> all)
    {
        var dto = new CategoryDto
        {
            Id = category.Id,
            ParentId = category.ParentId,
            Name = category.Name,
            Type = category.Type,
            Color = category.Color,
            Icon = category.Icon,
            SortOrder = category.SortOrder,
            Active = category.Active,
            DefaultKey = category.DefaultKey,
        };

        if (category.ParentId is null)
        {
            dto.Children = all
                .Where(c => c.ParentId == category.Id)
                .OrderBy(c => c.SortOrder)
                .Select(c => MapToDto(c, all))
                .ToList();
        }

        return dto;
    }

    /// <summary>Matches the default category seed in docs/data-model.md section 7.</summary>
    private static List<Category> BuildDefaultCategories(Guid userId)
    {
        var categories = new List<Category>();
        var topSortOrder = 0;

        void AddTop(string key, string name, CategoryType type, string color, string icon, params (string Key, string Name, string Icon)[] children)
        {
            var parent = new Category
            {
                UserId = userId,
                Name = name,
                Type = type,
                Color = color,
                Icon = icon,
                DefaultKey = key,
                SortOrder = topSortOrder++,
            };
            categories.Add(parent);

            var childSortOrder = 0;
            foreach (var (childKey, childName, childIcon) in children)
            {
                categories.Add(new Category
                {
                    UserId = userId,
                    ParentId = parent.Id,
                    Name = childName,
                    Type = type,
                    Icon = childIcon,
                    DefaultKey = childKey,
                    SortOrder = childSortOrder++,
                });
            }
        }

        AddTop("food", "Food", CategoryType.Expense, "#f97316", "utensils",
            ("food-groceries", "Groceries", "shopping-cart"),
            ("food-dining-out", "Dining out", "utensils-crossed"),
            ("food-coffee-snacks", "Coffee & snacks", "coffee"));

        AddTop("transport", "Transport", CategoryType.Expense, "#3b82f6", "car",
            ("transport-fuel", "Fuel", "fuel"),
            ("transport-public", "Public transport", "bus"),
            ("transport-taxi", "Taxi", "car-taxi-front"));

        AddTop("housing", "Rent & Housing", CategoryType.Expense, "#8b5cf6", "home",
            ("housing-rent", "Rent", "key"),
            ("housing-maintenance", "Maintenance", "wrench"),
            ("housing-furnishing", "Furnishing", "sofa"));

        AddTop("bills", "Bills & Utilities", CategoryType.Expense, "#eab308", "receipt",
            ("bills-electricity", "Electricity", "zap"),
            ("bills-internet", "Internet", "wifi"),
            ("bills-mobile", "Mobile", "smartphone"),
            ("bills-subscriptions", "Subscriptions", "repeat"));

        AddTop("shopping", "Shopping", CategoryType.Expense, "#ec4899", "shopping-bag",
            ("shopping-clothing", "Clothing", "shirt"),
            ("shopping-electronics", "Electronics", "laptop"),
            ("shopping-household", "Household", "package"));

        AddTop("health", "Health", CategoryType.Expense, "#ef4444", "heart-pulse",
            ("health-doctor", "Doctor", "stethoscope"),
            ("health-medicine", "Medicine", "pill"),
            ("health-fitness", "Fitness", "dumbbell"));

        AddTop("entertainment", "Entertainment", CategoryType.Expense, "#06b6d4", "film",
            ("entertainment-movies", "Movies", "clapperboard"),
            ("entertainment-games", "Games", "gamepad-2"),
            ("entertainment-travel", "Travel", "plane"));

        AddTop("fees", "Fees & Interest", CategoryType.Expense, "#6b7280", "percent",
            ("fees-card", "Card fees", "credit-card"),
            ("fees-interest", "Interest", "trending-up"),
            ("fees-bank", "Bank charges", "landmark"));

        AddTop("expense-other", "Other", CategoryType.Expense, "#64748b", "more-horizontal");

        AddTop("salary", "Salary", CategoryType.Income, "#22c55e", "briefcase");
        AddTop("freelance", "Freelance", CategoryType.Income, "#14b8a6", "laptop");
        AddTop("interest-returns", "Interest & Returns", CategoryType.Income, "#10b981", "trending-up");
        AddTop("income-other", "Other Income", CategoryType.Income, "#84cc16", "plus-circle");

        return categories;
    }
}
