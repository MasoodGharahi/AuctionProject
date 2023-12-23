using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using Polly;
using Polly.Extensions.Http;
using SearchService.Data;
using SearchService.Entities;
using SearchService.RequestHeplers;
using SearchService.Services;
using SearchService.Services.Interfaces;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient<IAuctionServiceHttpClient,AuctionServiceHttpClient>().AddPolicyHandler(GetPolicy());


var app = builder.Build();
app.Lifetime.ApplicationStarted.Register(async () =>
{
    try
    {
        await DbInitializer.InitDb(app);
    }
    catch (Exception ex)
    {
        throw new Exception($"Error while initializing MongoDb : {ex.Message}");
    }
});

app.MapGet("api/search", async ([FromBody] SearchParams? searchParams) =>
    {
        var query = DB.PagedSearch<Item, Item>();
        query.Sort(x => x.Ascending(x => x.Make));
        if (!string.IsNullOrEmpty(searchParams.SearchTerm))
        {
            query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
        }
        query = searchParams.OrderBy switch
        {
            "make" => query.Sort(x => x.Ascending(a => a.Make)),
            "new" => query.Sort(x => x.Descending(a => a.CreatedDate)),
            _ => query.Sort(x => x.Ascending(a => a.AuctionEnd))
        };
        query = searchParams.FilterBy switch
        {
            "finished" => query.Match(x => x.AuctionEnd < DateTime.UtcNow),
            "endingSoon" => query.Match(x => x.AuctionEnd < DateTime.UtcNow.AddHours(6) 
            && x.AuctionEnd > DateTime.UtcNow),
            _=>query.Match(x=>x.AuctionEnd>DateTime.UtcNow)
        };
        if(!string.IsNullOrEmpty( searchParams.Seller))
        {
            query.Match(x => x.Seller == searchParams.Seller);
        }
        if (!string.IsNullOrEmpty(searchParams.Winner))
        {
            query.Match(x => x.Winner == searchParams.Winner);
        }
        query.PageNumber(searchParams.PageNumber);
        query.PageSize(searchParams.PageSize);
        var result = await query.ExecuteAsync();
        var pagedResult = new
        {
            results = result.Results,
            pageCount = result.PageCount,
            totalCount = result.TotalCount
        };
        return Results.Ok(pagedResult);
    });
app.Run();

//declaring http requests policies
static IAsyncPolicy<HttpResponseMessage> GetPolicy()
    => HttpPolicyExtensions
    .HandleTransientHttpError().OrResult(msg=>msg.StatusCode== HttpStatusCode.NotFound)
    .WaitAndRetryForeverAsync(_=>TimeSpan.FromSeconds(3));