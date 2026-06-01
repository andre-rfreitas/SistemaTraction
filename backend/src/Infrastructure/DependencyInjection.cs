using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Infrastructure.Pdf;
using SistemaTraction.Infrastructure.Persistence;
using SistemaTraction.Infrastructure.WhatsApp;

namespace SistemaTraction.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IWhatsAppService, NicochatWhatsAppService>();
        services.AddScoped<IPdfParser, PdfPigParser>();

        return services;
    }
}
