using EventManagement.Application.Common.Interfaces;
using EventManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventManagement.IntegrationTests.Helpers;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    // Unique name per factory instance — fixed so all HTTP request scopes and
    // direct scope queries share the same in-memory store within one factory
    private readonly string _dbName = $"TestDb_{Guid.NewGuid()}";

    public FakeEmailService FakeEmail { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set required env var
        builder.UseSetting("JWT_SECRET", "test-jwt-secret-that-is-long-enough-32chars!!");

        builder.ConfigureServices(services =>
        {
            // Remove ALL existing AppDbContext and its options registrations
            var toRemove = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>)
                         || d.ServiceType == typeof(AppDbContext))
                .ToList();
            foreach (var d in toRemove)
                services.Remove(d);

            // Re-register with per-factory-instance name so all requests share the same DB
            services.AddDbContext<AppDbContext>(opts =>
                opts.UseInMemoryDatabase(_dbName));

            // Re-register IAppDbContext → AppDbContext bridge
            var iAppDbDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IAppDbContext));
            if (iAppDbDescriptor != null)
                services.Remove(iAppDbDescriptor);
            services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

            // Replace real email service with fake
            var emailDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IEmailService));
            if (emailDescriptor != null)
                services.Remove(emailDescriptor);

            services.AddSingleton<IEmailService>(FakeEmail);
        });

        builder.UseEnvironment("Testing");
    }
}
