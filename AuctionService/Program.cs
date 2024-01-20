using AuctionService.Consumers;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using MassTransit.Transports;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddDbContext<AuctionDbContext>(Opt =>
{ Opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")); });

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<AuctionDbContext>(x =>
    {
        x.QueryDelay = TimeSpan.FromSeconds(10);
        x.UsePostgres();
        x.UseBusOutbox();
    });
    x.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});
//Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["IdentityServiceUrl"];
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters.ValidateAudience = false;
        options.TokenValidationParameters.NameClaimType = "username";
    });
builder.Services.AddAuthorization();
var app = builder.Build();
app.UseAuthorization();


try
{
    DbInitializer.InitDb(app);
}
catch { }

//get all auctions
app.MapGet("/api/auctions", async (string? date, AuctionDbContext repo, IMapper mapper) =>
{
    //var auctions = await repo.Auctions.Include(x => x.Item).OrderBy(x => x.Item.Make).ToListAsync();
    var auctions = repo.Auctions.OrderBy(x => x.Item.Make).AsQueryable();
    if (!string.IsNullOrEmpty(date))
    {
        auctions = auctions.Where(x => x.UpdatedDate.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);

    }
    var auctionDtos = mapper.Map<List<AuctionDTO>>(auctions);
    return Results.Ok(
        await auctions.ProjectTo<AuctionDTO>(mapper.ConfigurationProvider).ToListAsync()
        );
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

app.MapPost("/api/auctions", [Authorize] async (CreateAuctionDTO createAuctionDTO, IMapper mapper, AuctionDbContext repo,
    IPublishEndpoint publishEndpoint, HttpContext context) =>
{
    var auction = mapper.Map<Auction>(createAuctionDTO);
    auction.Seller =context.User.Identity.Name;
    await repo.Auctions.AddAsync(auction);

    var createdAuctionDto = mapper.Map<AuctionDTO>(auction);
    await publishEndpoint.Publish(mapper.Map<AuctionCreated>(createdAuctionDto));

    bool result = await repo.SaveChangesAsync() > 0;
    if (!result)
        return Results.BadRequest("Could not create the record.");

    return Results.CreatedAtRoute(routeName: "GetAuctionById", routeValues: new { id = auction.Id }, value: createdAuctionDto);
});

//update
app.MapPut("/api/auctions/{id}",[Authorize] async (Guid id, UpdateAuctionDTO updateAuction, IMapper mapper,
    AuctionDbContext repo, IPublishEndpoint publishEndpoint,HttpContext context) =>
{
    var auction = await repo.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);
    if (auction == null) return Results.NotFound();

    if (auction.Seller != context.User.Identity.Name) return Results.Forbid();
    auction.Item.Make = updateAuction.Make ?? auction.Item.Make;
    auction.Item.Model = updateAuction.Model ?? auction.Item.Model;
    auction.Item.Color = updateAuction.Color ?? auction.Item.Color;
    auction.Item.Mileage = updateAuction.Mileage ?? auction.Item.Mileage;
    auction.Item.Year = updateAuction.Year ?? auction.Item.Year;
    
    await publishEndpoint.Publish(mapper.Map<AuctionUpdated>(auction));

    var result = await repo.SaveChangesAsync() > 0;
    if (!result) return Results.BadRequest();
    //var updatedAuction = mapper.Map<AuctionUpdated>(updateAuction);
    
    return Results.Ok();
});

//delete
app.MapDelete("/api/auctions/{id}", [Authorize] async (Guid id, AuctionDbContext repo,
    IPublishEndpoint publishEndpoint, HttpContext context) =>
{
    var auction = await repo.Auctions.FindAsync(id);
    if (auction != null)
    {
        if (auction.Seller != context.User.Identity.Name) return Results.Forbid();
        repo.Auctions.Remove(auction);
       await publishEndpoint.Publish(new AuctionDeleted { Id=auction.Id.ToString() });
        var result = await repo.SaveChangesAsync() > 0;

        return Results.Ok();
    }
    return Results.BadRequest();
});

app.Run();