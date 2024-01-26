using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Entities;

namespace SearchService.Consumers
{
    public class BidPlacedConsumer : IConsumer<BidPlaced>
    {
        public BidPlacedConsumer()
        {
                
        }
        public async Task Consume(ConsumeContext<BidPlaced> context)
        {
            var auction = await DB.Find<Item>().OneAsync(context.Message.AuctionId);
            if (context.Message.BidStatus.Contains("Accepted") && context.Message.Amount > auction.CurrentHighBid)
            { 
            auction.CurrentHighBid = context.Message.Amount;
                await auction.SaveAsync();
            }
        }
    }
}
