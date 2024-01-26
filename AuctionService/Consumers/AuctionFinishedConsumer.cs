using AuctionService.Data;
using AuctionService.Entities;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace AuctionService.Consumers
{
    public class AuctionFinishedConsumer:IConsumer<AuctionFinished>
    {
        public AuctionDbContext dbContext { get; }
        public AuctionFinishedConsumer(AuctionDbContext context)
        {
            dbContext = context;
        }
        public async Task Consume(ConsumeContext<AuctionFinished> context)
        {
            var auction = await dbContext.Auctions.FindAsync(context.Message.AuctionId);
            if(context.Message.ItemSold)
            {
                auction.Winner = context.Message.Winner;
                auction.SoldAmount = context.Message.Amount;
            }
            auction.Status = auction.SoldAmount > auction.ReservePrice ? Status.Finished : Status.ReserveNotMet;
            await dbContext.SaveChangesAsync();

        }
      
    }
}
