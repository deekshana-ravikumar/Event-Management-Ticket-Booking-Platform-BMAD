using EventManagement.Application.Common.Interfaces;
using EventManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventManagement.IntegrationTests.Helpers;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public FakeEmailService FakeEmail { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set required env var
        builder.UseSetting("JWT_SECRET", "test-jwt-secret-that-is-long-enough-32chars!!");

        builder.ConfigureServices(services =>
        {
            // Remove real DbContext options
            var dbDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (dbDescriptor != null)
                services.Remove(dbDescriptor);

            // Add isolated in-memory database per factory instance
            services.AddDbContext<AppDbContext>(opts =>
                opts.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));

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
