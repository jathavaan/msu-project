using FluentValidation;

namespace FinanceOne.Api.Common;

// Scans the assembly and registers handlers/repositories by interface implementation —
// no marker interfaces, no manual per-class registration. Adding a new slice never
// requires touching this file as long as the naming conventions in
// server/FinanceOne/CLAUDE.md are followed.
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFinanceOneServices(this IServiceCollection services)
    {
        var assembly = typeof(Program).Assembly;

        // IRequestHandler<,> implementations
        foreach (var type in assembly.GetTypes().Where(t => !t.IsAbstract && !t.IsInterface))
        {
            var handlerInterfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));
            foreach (var handlerInterface in handlerInterfaces)
            {
                services.AddScoped(handlerInterface, type);
                services.AddScoped(type); // handler also resolvable directly for the endpoint
            }
        }

        // *Repository implementations (interface I<Name>Repository -> class <Name>Repository)
        foreach (var type in assembly.GetTypes().Where(t => !t.IsAbstract && t.Name.EndsWith("Repository")))
        {
            var repoInterface = type.GetInterfaces().FirstOrDefault(i => i.Name == $"I{type.Name}");
            if (repoInterface is not null)
            {
                services.AddScoped(repoInterface, type);
            }
        }

        // FluentValidation validators
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
