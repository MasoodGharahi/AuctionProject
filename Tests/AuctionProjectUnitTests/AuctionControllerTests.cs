using AuctionProjectUnitTests.Utils;
using AuctionService.DTOs;
using AuctionService.Endpoints;
using AuctionService.Entities;
using AuctionService.Repository.Interface;
using AuctionService.RequestHelpers;
using AutoFixture;
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
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
        private readonly IMapper _mapper;
        private readonly HttpContext _httpContext;
        public AuctionControllerTests()
        {
            _auctionRepo = new Mock<IAuctionRepository>();
            _fixture = new Fixture();
            _publishEndpoint = new Mock<IPublishEndpoint>();

            var mockMapper = new MapperConfiguration(mc =>
            {
                mc.AddMaps(typeof(MappingProfiles).Assembly);
            }).CreateMapper().ConfigurationProvider;

            _mapper = new Mapper(mockMapper);
            _httpContext = new DefaultHttpContext { User = Helpers.GetClaimsPrincipal() };
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
            _auctionRepo.Setup(repo => repo.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(value: null);

            //var client = CreateClient();

            // Act
            var response = await AuctionEndpoints.GetById(Guid.NewGuid(), _auctionRepo.Object);

            // Assert
            Assert.IsType<NotFound>(response); // Ensure not found response
        }
        [Fact]
        public async Task CreateAuction_WithInValidCreatedAuctionDto_ReturnsCreatedAtAction()
        {
            // Arrange
            var auction = _fixture.Create<CreateAuctionDTO>();
            _auctionRepo.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));
            _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

            // Act
            var result = await AuctionEndpoints.Create(auction, _mapper, _publishEndpoint.Object, _httpContext, _auctionRepo.Object);
            var createdResult = ((CreatedAtRoute<AuctionDTO>)result).Value;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CreatedAtRoute<AuctionDTO>>(result);
        }
        [Fact]
        public async Task CreateAuction_FailedSave_Returns400BadRequest()
        {
            // Arrange
            var auction = _fixture.Create<CreateAuctionDTO>();
            _auctionRepo.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));
            _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(false);

            // Act
            var result = await AuctionEndpoints.Create(auction, _mapper, _publishEndpoint.Object, _httpContext, _auctionRepo.Object);

            // Assert
            Assert.IsType<BadRequest<string>>(result);
        }

        [Fact]
        public async Task UpdateAuction_WithUpdateAuctionDto_ReturnsOkResponse()
        {
            //arrange
            var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
            auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();
            auction.Seller = "Test";
            var updateDto = _fixture.Create<UpdateAuctionDTO>();

            _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>()))
                .ReturnsAsync(auction);
            _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

            //act
            var result = await AuctionEndpoints.Update(Guid.NewGuid(), updateDto, _mapper,
                _publishEndpoint.Object,_httpContext ,_auctionRepo.Object);

            //assert
            Assert.IsType<Ok>(result);

        }

        [Fact]
        public async Task UpdateAuction_WithInvalidUser_Returns403Forbid()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public async Task UpdateAuction_WithInvalidGuid_ReturnsNotFound()
        {
            //arrange
            var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
            auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();
            auction.Seller = "Test";
            var updateDto = _fixture.Create<UpdateAuctionDTO>();

            _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>()))
                .ReturnsAsync(value:null);

            //act
            var result = await AuctionEndpoints.Update(Guid.NewGuid(), updateDto, _mapper,
                _publishEndpoint.Object, _httpContext, _auctionRepo.Object);

            //assert
            Assert.IsType<NotFound>(result);
        }

        [Fact]
        public async Task DeleteAuction_WithValidUser_ReturnsOkResponse()
        {    
            //arrange
            var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
            auction.Seller = "Test";

            _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>()))
                .ReturnsAsync(auction);

            //act
            var result = await AuctionEndpoints.Delete(Guid.NewGuid(),
                _publishEndpoint.Object, _httpContext, _auctionRepo.Object);

            //assert
            Assert.IsType<Ok>(result);
        }

        [Fact]
        public async Task DeleteAuction_WithInvalidGuid_Returns404Response()
        {
            //arrange
            _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>()))
                .ReturnsAsync(value:null);

            //act
            var result = await AuctionEndpoints.Delete(Guid.NewGuid(),
                _publishEndpoint.Object, _httpContext, _auctionRepo.Object);

            //assert
            Assert.IsType<BadRequest>(result);
        }

        [Fact]
        public async Task DeleteAuction_WithInvalidUser_Returns403Response()
        {
            throw new NotImplementedException();
        }
    }
}
