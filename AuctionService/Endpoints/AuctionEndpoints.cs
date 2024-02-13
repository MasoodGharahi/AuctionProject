using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.Repository.Interface;
using AuctionService.Repository;
using AutoMapper;
using Contracts;
using MassTransit.Transports;
using MassTransit;
using Microsoft.AspNetCore.Authorization;


namespace AuctionService.Endpoints
{
    public static class AuctionEndpoints
    {
        public static void MapAuctionEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/auctions", GetByDate);
            app.MapGet("/api/auctions/{id}", GetById).WithName("GetAuctionById");
            app.MapPost("/api/auctions", Create);
            app.MapPut("/api/auctions/{id}", Update);
            app.MapDelete("/api/auctions/{id}", Update);

        }


        public static async Task<IResult> GetByDate(string? date, IAuctionRepository auctionRepository)
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
        public static async Task<IResult> GetById(Guid id, IMapper mapper, AuctionDbContext repo, IAuctionRepository auctionRepository)
        {
            var auction = await auctionRepository.GetAuctionByIdAsync(id);
            if (auction == null)
                return Results.NotFound();

            return Results.Ok(auction);
        }

        //create auction
        [Authorize]
        public static async Task<IResult> Create(CreateAuctionDTO createAuctionDTO,
            IMapper mapper, AuctionDbContext repo,
        IPublishEndpoint publishEndpoint, HttpContext context, IAuctionRepository auctionRepository)
        {
            var auction = mapper.Map<Auction>(createAuctionDTO);
            auction.Seller = context.User.Identity.Name;
            auctionRepository.AddAuction(auction);

            var createdAuctionDto = mapper.Map<AuctionDTO>(auction);
            await publishEndpoint.Publish(mapper.Map<AuctionCreated>(createdAuctionDto));

            bool result = await auctionRepository.SaveChangesAsync();
            if (!result)
                return Results.BadRequest("Could not create the record.");

            return Results.CreatedAtRoute(routeName: "GetAuctionById", routeValues: new
            {
                id = auction.Id
            }, value: createdAuctionDto);
        }


        //update
        [Authorize]
        public static async Task<IResult> Update(Guid id, UpdateAuctionDTO updateAuction,
            IMapper mapper,
    AuctionDbContext repo, IPublishEndpoint publishEndpoint, HttpContext context
    , IAuctionRepository auctionRepository)
        {
            var auction = await auctionRepository.GetAuctionEntityById(id);
            if (auction == null) return Results.NotFound();

            if (auction.Seller != context.User.Identity.Name) return Results.Forbid();
            auction.Item.Make = updateAuction.Make ?? auction.Item.Make;
            auction.Item.Model = updateAuction.Model ?? auction.Item.Model;
            auction.Item.Color = updateAuction.Color ?? auction.Item.Color;
            auction.Item.Mileage = updateAuction.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = updateAuction.Year ?? auction.Item.Year;

            await publishEndpoint.Publish(mapper.Map<AuctionUpdated>(auction));

            var result = await auctionRepository.SaveChangesAsync();
            if (!result) return Results.BadRequest();
            //var updatedAuction = mapper.Map<AuctionUpdated>(updateAuction);

            return Results.Ok();
        }


        //delete
        [Authorize]
        public static async Task<IResult> Delete (Guid id, AuctionDbContext repo,
    IPublishEndpoint publishEndpoint, HttpContext context, IAuctionRepository auctionRepository)
        {
            var auction = await auctionRepository.GetAuctionEntityById(id);
            if (auction != null)
            {
                if (auction.Seller != context.User.Identity.Name) return Results.Forbid();
                auctionRepository.RemoveAuction(auction);
                await publishEndpoint.Publish(new AuctionDeleted
                {
                    Id = auction.Id.ToString()
                });
                var result = await auctionRepository.SaveChangesAsync();

                return Results.Ok();
            }
            return Results.BadRequest();
        }
    }
}
