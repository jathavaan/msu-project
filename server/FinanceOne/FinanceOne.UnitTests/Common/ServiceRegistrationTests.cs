using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceOne.UnitTests.Common;

// DI in this codebase is convention-driven: AddFinanceOneServices scans the assembly and registers
// by name/interface shape rather than by explicit per-class lines. That makes a misnamed slice fail
// at request time with a DI resolution error rather than at build time, so these tests assert the
// conventions still hold across every slice — not just the ones with their own tests.
public class ServiceRegistrationTests
{
    private static readonly IReadOnlyList<ServiceDescriptor> Descriptors =
        new ServiceCollection().AddFinanceOneServices().ToList();

    private static IEnumerable<Type> ApiTypes =>
        typeof(Response<Guid>).Assembly.GetTypes().Where(t => t is { IsAbstract: false, IsInterface: false });

    [Fact]
    public void Every_Handler_Is_Registered_By_Its_Concrete_Type()
    {
        // Endpoints inject the handler concretely (CreateBudgetHandler, not IRequestHandler<,>),
        // so the concrete registration is the one that actually has to be there.
        var handlers = ApiTypes
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
            .ToList();

        Assert.NotEmpty(handlers);

        var unregistered = handlers
            .Where(h => Descriptors.All(d => d.ServiceType != h))
            .Select(h => h.FullName)
            .ToList();

        Assert.Empty(unregistered);
    }

    [Fact]
    public void Every_Repository_Is_Registered_Against_Its_Interface()
    {
        // The scan pairs <Name>Repository with I<Name>Repository purely by name. A repository whose
        // interface is named anything else silently never gets registered.
        var repositories = ApiTypes.Where(t => t.Name.EndsWith("Repository")).ToList();

        Assert.NotEmpty(repositories);

        var unregistered = repositories
            .Where(r => Descriptors.All(d => d.ImplementationType != r))
            .Select(r => r.FullName)
            .ToList();

        Assert.Empty(unregistered);
    }

    [Fact]
    public void Every_Validator_Is_Registered()
    {
        var validators = ApiTypes
            .Where(t => t.BaseType is { IsGenericType: true } b && b.GetGenericTypeDefinition() == typeof(AbstractValidator<>))
            .ToList();

        Assert.NotEmpty(validators);

        var unregistered = validators
            .Where(v => Descriptors.All(d => d.ImplementationType != v))
            .Select(v => v.FullName)
            .ToList();

        Assert.Empty(unregistered);
    }
}
