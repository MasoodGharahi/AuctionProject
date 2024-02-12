using AuctionService.Consumers;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.Repository;
using AuctionService.Repository.Interface;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using MassTransit.Transports;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddDbContext<AuctionDbContext>(Opt =>
{ 
    Opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

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
        cfg.Host(builder.Configuration["RabbitMq:Host"], "/", host =>
        {
            host.Username(builder.Configuration.GetValue("RabbitMq:Username", "guest"));
            host.Password(builder.Configuration.GetValue("RabbitMq:Password", "guest")); 
        });
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
//Add repositories
builder.Services.AddScoped<IAuctionRepository, AuctionRepository>();

var app = builder.Build();
app.UseAuthorization();


try
{
    DbInitializer.InitDb(app);
}
catch { }

//get all auctions
app.MapGet("/api/auctions", AuctionsWithDate);
 static async Task<IResult>  AuctionsWithDate(string? date, IAuctionRepository auctionRepository)
{
    try
    {
        var auctions = await auctionRepository.GetAuctionsAsync(date);
        return Results.Ok(auctions);
    }
    catch (Exception e)
    {
        throw new Exception(e.Message);
    }
}

//get by id
app.MapGet("/api/auctions/{id}", async (Guid id, IMapper mapper, AuctionDbContext repo,IAuctionRepository auctionRepository) =>
{
    var auction = await auctionRepository.GetAuctionByIdAsync(id);
    if (auction == null)
        return Results.NotFound();

    return Results.Ok(auction);
}).WithName("GetAuctionById");

//create auction

app.MapPost("/api/auctions", [Authorize] async (CreateAuctionDTO createAuctionDTO, IMapper mapper, AuctionDbContext repo,
    IPublishEndpoint publishEndpoint, HttpContext context,IAuctionRepository auctionRepository) =>
{
    var auction = mapper.Map<Auction>(createAuctionDTO);
    auction.Seller =context.User.Identity.Name;
     auctionRepository.AddAuction(auction);

    var createdAuctionDto = mapper.Map<AuctionDTO>(auction);
    await publishEndpoint.Publish(mapper.Map<AuctionCreated>(createdAuctionDto));

    bool result = await auctionRepository.SaveChangesAsync() ;
    if (!result)
        return Results.BadRequest("Could not create the record.");

    return Results.CreatedAtRoute(routeName: "GetAuctionById", routeValues: new { id = auction.Id }, value: createdAuctionDto);
});

//update
app.MapPut("/api/auctions/{id}",[Authorize] async (Guid id, UpdateAuctionDTO updateAuction, IMapper mapper,
    AuctionDbContext repo, IPublishEndpoint publishEndpoint,HttpContext context
    ,IAuctionRepository auctionRepository) =>
{
    var auction =await auctionRepository.GetAuctionEntityById(id);
    if (auction == null) return Results.NotFound();

    if (auction.Seller != context.User.Identity.Name) return Results.Forbid();
    auction.Item.Make = updateAuction.Make ?? auction.Item.Make;
    auction.Item.Model = updateAuction.Model ?? auction.Item.Model;
    auction.Item.Color = updateAuction.Color ?? auction.Item.Color;
    auction.Item.Mileage = updateAuction.Mileage ?? auction.Item.Mileage;
    auction.Item.Year = updateAuction.Year ?? auction.Item.Year;
    
    await publishEndpoint.Publish(mapper.Map<AuctionUpdated>(auction));

    var result = await auctionRepository.SaveChangesAsync() ;
    if (!result) return Results.BadRequest();
    //var updatedAuction = mapper.Map<AuctionUpdated>(updateAuction);
    
    return Results.Ok();
});

//delete
app.MapDelete("/api/auctions/{id}", [Authorize] async (Guid id, AuctionDbContext repo,
    IPublishEndpoint publishEndpoint, HttpContext context,IAuctionRepository auctionRepository) =>
{
    var auction = await auctionRepository.GetAuctionEntityById(id);
    if (auction != null)
    {
        if (auction.Seller != context.User.Identity.Name) return Results.Forbid();
        auctionRepository.RemoveAuction(auction);
       await publishEndpoint.Publish(new AuctionDeleted { Id=auction.Id.ToString() });
        var result = await auctionRepository.SaveChangesAsync();

        return Results.Ok();
    }
    return Results.BadRequest();
});

app.Run();

// Make the implicit Program class public so test projects can access it
public partial class Program { }