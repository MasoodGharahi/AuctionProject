using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Entities;
using SearchService.Services.Interfaces;
using System.Text.Json;

namespace SearchService.Data
{
    public class DbInitializer
    {
        public static async Task InitDb(WebApplication app)
        {
            await DB.InitAsync("SearchDb", MongoClientSettings
            .FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));
            await DB.Index<Item>()
                .Key(x => x.Make, KeyType.Text)
                .Key(x => x.Model, KeyType.Text)
                .Key(x => x.Color, KeyType.Text)
                .CreateAsync();
            var count = await DB.CountAsync<Item>();
            if (count < 1)
            {
                using var scope = app.Services.CreateScope();
                var httpClient = scope.ServiceProvider.GetRequiredService<IAuctionServiceHttpClient>();
                var items = await httpClient.GetItemsForSearchDb();
                if (items != null && items.Count > 0) await DB.SaveAsync<Item>(items);
            }
            
        }
    }
}
