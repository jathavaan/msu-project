using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Persistence;

// Cross-cutting infrastructure shared by every vertical slice under Features/.
// DbSets and entity relations (via IEntityTypeConfiguration<T> in Configurations/)
// are added in a follow-up step.
public sealed class FinanceOneDbContext(DbContextOptions<FinanceOneDbContext> options)
    : DbContext(options);
