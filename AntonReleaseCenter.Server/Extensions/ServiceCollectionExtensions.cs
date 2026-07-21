namespace AntonReleaseCenter.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddReleaseCenterServices(this IServiceCollection services)
    {
        services.AddScoped<IReleaseService, ReleaseService>();
        services.AddScoped<IAdminService, AdminService>();
        return services;
    }
}
