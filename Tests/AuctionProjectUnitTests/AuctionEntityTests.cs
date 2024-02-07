using AuctionService.Entities;

namespace AuctionProjectUnitTests
{
    public class AuctionEntityTests
    {
        [Fact]
        //Test naming format => Method_Scenario_Result
        public void HasReservrPrice_ReservePriceGreaterThanZero_True()
        {
            //arrange
            var auction = new Auction { Id = Guid.NewGuid(), ReservePrice = 10 };

            //act
            var result = auction.HasReservePrice();

            //assert
            Assert.True(result);
        }

        [Fact]
        //Test naming format => Method_Scenario_Result
        public void HasReservrPrice_ReservePriceZero_True()
        {
            //arrange
            var auction = new Auction { Id = Guid.NewGuid(), ReservePrice = 0 };

            //act
            var result = auction.HasReservePrice();

            //assert
            Assert.True(result);
        }
    }
}