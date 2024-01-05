using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Entities;

namespace SearchService.Consumers
{
    public class AuctionDeletedConsumer : IConsumer<AuctionDeleted>
    {
        public async Task Consume(ConsumeContext<AuctionDeleted> context)
        {
          var result=  await DB.DeleteAsync<Item>(context.Message.Id);
            if(!result.IsAcknowledged) { throw new Exception($"MongoDb could not delete item {context.Message.Id}"); }
        }
    }
}
