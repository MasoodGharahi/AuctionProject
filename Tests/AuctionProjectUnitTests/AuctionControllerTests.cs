using AuctionService.DTOs;
using AuctionService.Endpoints;
using AuctionService.Repository.Interface;
using AutoFixture;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;

namespace AuctionProjectUnitTests
{
    public class AuctionControllerTests : WebApplicationFactory<Program>
    {
        private readonly Mock<IAuctionRepository> _auctionRepo;
        private readonly Fixture _fixture;
        private readonly Mock<IPublishEndpoint> _publishEndpoint;
        public AuctionControllerTests()
        {
            _auctionRepo = new Mock<IAuctionRepository>();
            _fixture = new Fixture();
            _publishEndpoint = new Mock<IPublishEndpoint>();
        }
        [Fact]
        public async Task AuctionsWithDate_WithNoParams_Returns10Auctions()
        {
            // Arrange
            var expectedAuctionCount = 10;
            var expectedDtos = _fixture.CreateMany<AuctionDTO>(expectedAuctionCount).ToList();
            _auctionRepo.Setup(repo => repo.GetAuctionsAsync(null)).ReturnsAsync(expectedDtos);

            var client = CreateClient();

            // Act
            var response = await AuctionEndpoints.GetByDate(null, _auctionRepo.Object);

            // Assert
            Assert.IsType<Ok<List<AuctionDTO>>>(response); // Ensure successful response

            var actualDtos = ((Ok<List<AuctionDTO>>)response).Value;
            Assert.Equal(expectedAuctionCount, actualDtos.Count);
        }

        [Fact]
        public async Task GetById_WithValidGUID_ReturnsAuction()
        {
            // Arrange
            var auction = _fixture.Create<AuctionDTO>();
            _auctionRepo.Setup(repo => repo.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);

            //var client = CreateClient();

            // Act
            var response = await AuctionEndpoints.GetById(auction.Id, _auctionRepo.Object);

            // Assert
            Assert.IsType<Ok<AuctionDTO>>(response); // Ensure successful response

            var actualAuction = ((Ok<AuctionDTO>)response).Value;
            Assert.Equal(auction.Make, actualAuction?.Make);
        }

        [Fact]
        public async Task GetById_WithInValidGUID_ReturnsNotFound()
        {
            // Arrange
            _auctionRepo.Setup(repo => repo.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(value:null);

            //var client = CreateClient();

            // Act
            var response = await AuctionEndpoints.GetById(Guid.NewGuid(), _auctionRepo.Object);

            // Assert
            Assert.IsType<NotFound>(response); // Ensure not found response
        }
    }
}
