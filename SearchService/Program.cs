using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var app = builder.Build();
await DB.InitAsync("SearchDb", MongoClientSettings
    .FromConnectionString(builder.Configuration.GetConnectionString("MongoDbConnection")));
await DB.Index<Item>()
    .Key(x => x.Make, KeyType.Text)
    .Key(x => x.Model, KeyType.Text)
    .Key(x => x.Color, KeyType.Text)
    .CreateAsync();

app.Run();

