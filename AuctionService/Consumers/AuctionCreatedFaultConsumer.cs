using Contracts;
using MassTransit;

namespace AuctionService.Consumers
{
    public class AuctionCreatedFaultConsumer : IConsumer<Fault<AuctionCreated>>
    {
        public async Task Consume(ConsumeContext<Fault<AuctionCreated>> context)
        {
            var exception= context.Message.Exceptions.FirstOrDefault();
            if (exception!=null && exception.ExceptionType=="System.ArgumentException")
            {
                context.Message.Message.Model = "changed model";
                await context.Publish(context.Message.Message); 
            }
        }
    }
}
