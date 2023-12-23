namespace AuctionService.Entities
{
    public class Base
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; }= DateTime.UtcNow;

    }
}
