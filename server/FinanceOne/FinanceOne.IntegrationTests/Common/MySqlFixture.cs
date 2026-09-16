using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore.Extensions;
using Testcontainers.MySql;

namespace FinanceOne.IntegrationTests.Common;

/// <summary>
/// Starts one throwaway MySQL container for the whole test assembly and migrates it with the same
/// FinanceOneDbContext migrations that ship to production. A real engine (rather than the in-memory
/// or SQLite providers) is what makes MySQL-specific behaviour observable: decimal(18,2) rounding,
/// the unique index behind "one budget per category", and the Restrict delete rules on Category.
/// </summary>
public sealed class MySqlFixture : IAsyncLifetime
{
    private readonly MySqlContainer _container = new MySqlBuilder()
        .WithImage("mysql:8.0")
        .WithDatabase("financeone_test")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public FinanceOneDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<FinanceOneDbContext>().UseMySQL(ConnectionString).Options);
}

/// <summary>
/// Every integration test joins this collection, so the containers are started once and tests run
/// sequentially — which is also what lets each test wipe the tables without racing the others.
/// Also provides <see cref="AzuriteFixture"/>: a test class picks up either fixture (or both) just by
/// naming it as a constructor parameter, xUnit resolves them by type regardless of which
/// ICollectionFixture&lt;T&gt; declared them.
/// </summary>
[CollectionDefinition(Name)]
public sealed class DatabaseCollection : ICollectionFixture<MySqlFixture>, ICollectionFixture<AzuriteFixture>
{
    public const string Name = "Database";
}
