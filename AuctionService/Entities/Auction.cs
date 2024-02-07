using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

namespace AuctionService.Entities
{
    [Table("Auctions")]
    public class Auction : Base
    {

        public Int64 ReservePrice { get; set; }
        public string Seller { get; set; }
        public string? Winner { get; set; }
        public Int64? SoldAmount { get; set; }
        public Int64? CurrentHighBid { get; set; }
        public DateTime AuctionEnd { get; set; }

        //Relations
        public Status Status { get; set; }
        public Item Item { get; set; }

        public bool HasReservePrice() => ReservePrice > 0;
    }
}
