using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddDbContext<AuctionDbContext>(Opt =>
{ Opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")); });

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

//app.UseAuthorization();
try
{
    DbInitializer.InitDb(app);
}
catch { }

//get all auctions
app.MapGet("/api/auctions", async (AuctionDbContext repo, IMapper mapper) =>
{
    var auctions = await repo.Auctions.Include(x => x.Item).OrderBy(x => x.Item.Make).ToListAsync();
    var auctionDtos = mapper.Map<List<AuctionDTO>>(auctions);
    return Results.Ok(auctionDtos);
});


//get by id
app.MapGet("/api/auctions/{id}", async (Guid id, IMapper mapper, AuctionDbContext repo) =>
{
    var auction = await repo.Auctions.FindAsync(id);
    if (auction == null)
        return Results.NotFound();
    var auctionDto = mapper.Map<AuctionDTO>(auction);

    return Results.Ok(auctionDto);
}).WithName("GetAuctionById");
//create auction
app.MapPost("/api/auctions", async (CreateAuctionDTO createAuctionDTO, IMapper mapper, AuctionDbContext repo) =>
{
    var auction = mapper.Map<Auction>(createAuctionDTO);
    auction.Seller = "Temp seller";
    await repo.Auctions.AddAsync(auction);
    bool result = await repo.SaveChangesAsync() > 0;
    if (!result)
        return Results.BadRequest("Could not create the record.");
    return Results.CreatedAtRoute(routeName: "GetAuctionById", routeValues: new { id = auction.Id }, value: mapper.Map<AuctionDTO>(auction));
});

//update
app.MapPut("/api/auctions/{id}", async (Guid id, UpdateAuctionDTO updateAuction, IMapper mapper, AuctionDbContext repo) =>
{
    var auction = await repo.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);
    if (auction == null) return Results.NotFound();
    auction.Item.Make = updateAuction.Make ?? auction.Item.Make;
    auction.Item.Model = updateAuction.Model ?? auction.Item.Model;
    auction.Item.Color = updateAuction.Color ?? auction.Item.Color;
    auction.Item.Mileage = updateAuction.Mileage ?? auction.Item.Mileage;
    auction.Item.Year = updateAuction.Year ?? auction.Item.Year;
    var result = await repo.SaveChangesAsync() > 0;
    if (!result) return Results.BadRequest();
    return Results.Ok();
});
app.MapDelete("/api/auctions/{id}", async (Guid id, AuctionDbContext repo) =>
{
    var auction = await repo.Auctions.FindAsync(id);
    if (auction != null)
    {
        repo.Auctions.Remove(auction);
        var result = await repo.SaveChangesAsync() > 0;

        return Results.Ok();
    }
    return Results.BadRequest();
});

app.Run();