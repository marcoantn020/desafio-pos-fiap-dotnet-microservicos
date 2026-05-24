using FCG.Monolith.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FCG.Monolith.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IGameService, GameService>();
        services.AddScoped<ILibraryService, LibraryService>();
        services.AddScoped<IPromotionService, PromotionService>();
        return services;
    }
}
