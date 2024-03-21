using AuctionService.Data;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;

namespace AuctionProjectIntegrationTests.Fixtures
{
    internal class CustomWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();
        public async Task InitializeAsync()
        {
            await _postgreSqlContainer.StartAsync();
        }
       // Overriding custom services so we don't have to read from program.cs
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<AuctionDbContext>));

                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<AuctionDbContext>(options=>options.UseNpgsql(_postgreSqlContainer.GetConnectionString()));

                services.AddMassTransit();

                // Adding and applying migrations to our test database

                var sp = services.BuildServiceProvider();
                
                using var scope=sp.CreateScope();
                var scopedServices=scope.ServiceProvider;
                var db=scopedServices.GetRequiredService<AuctionDbContext>(); 

                db.Database.Migrate();
            });
        }
        Task IAsyncLifetime.DisposeAsync() => _postgreSqlContainer.DisposeAsync().AsTask();
    }
}
