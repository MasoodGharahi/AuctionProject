﻿namespace AuctionService.DTOs
{
    public class AuctionDTO
    {
        public Guid Id { get; set; }
        public Int64 ReservePrice { get; set; }
        public string Seller { get; set; }
        public string Winner { get; set; }
        public Int64 SoldAmount { get; set; }
        public Int64 CurrentHighBid { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public DateTime AuctionEnd { get; set; }
        public string Status { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string Color { get; set; }
        public int Mileage { get; set; }
        public string Image { get; set; }
    }
}
