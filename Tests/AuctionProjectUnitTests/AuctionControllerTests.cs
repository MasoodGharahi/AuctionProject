using AuctionService.DTOs;
using AuctionService.Endpoints;
using AuctionService.Repository.Interface;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;

namespace AuctionProjectUnitTests
{
    public class AuctionControllerTests : WebApplicationFactory<Program>
    {
        private readonly Mock<IAuctionRepository> _auctionRepo;
        private readonly Fixture _fixture;
        public AuctionControllerTests()
        {
            _auctionRepo = new Mock<IAuctionRepository>();
            _fixture = new Fixture();
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
            var response = await AuctionEndpoints.GetByDate(null,_auctionRepo.Object);

            // Assert
            //Assert.Equal(Results.Ok(), response);

            Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Ok<List<AuctionDTO>>>(response); // Ensure successful response

            var actualDtos = ((Microsoft.AspNetCore.Http.HttpResults.Ok<List<AuctionDTO>>)response).Value;
            Assert.Equal(expectedAuctionCount, actualDtos.Count);

            //var actualDtos = await response.Content.ReadFromJsonAsync<List<AuctionDTO>>();
            //Assert.Equal(expectedAuctionCount, actualDtos.Count);
        }


        //    private readonly Mock<IAuctionRepository> _auctionRepo;
        //    private readonly Mock<IPublishEndpoint> _publishEndPoint;
        //    private readonly Fixture _fixture;
        //    //private readonly AuctionController _controller;
        //    private readonly HttpClient _httpClient;
        //    private readonly WebApplicationFactory<Program> _factory;

        //    private readonly IMapper _mapper;



        //    public AuctionControllerTests(WebApplicationFactory<Program> factory)
        //    {
        //        _factory = factory;
        //        _auctionRepo = new Mock<IAuctionRepository>();
        //        _publishEndPoint = new Mock<IPublishEndpoint>();
        //        _fixture = new Fixture();

        //        var mockMapper = new MapperConfiguration(x =>
        //        {
        //            x.AddMaps(typeof(MappingProfiles).Assembly);
        //        }
        //            ).CreateMapper().ConfigurationProvider;
        //        _mapper = new Mapper(mockMapper);
        //       _httpClient= _factory.CreateClient();
        //    }
        //    [Fact]
        //    public async Task GetAuctions_WithNoParams_Returns10Auctions()
        //    {
        //        // arrange
        //        var auctions=_fixture.CreateMany<AuctionDTO>(10).ToList();
        //        _auctionRepo.Setup(x => x.GetAuctionsAsync(null)).ReturnsAsync(auctions);
        //        // act
        //        var response = await _httpClient.GetAsync($"/api/auctions/"); // Call the Minimal API endpoint
        //        //because the return type is json we should convert it
        //        var actualDtos = await response.Content.ReadFromJsonAsync<List<AuctionDTO>>();

        //        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        //        Assert.IsType<ActionResult<List<AuctionDTO>>>(actualDtos);
        //        Assert.Equal(10, actualDtos.Count);
        //    }


    }
}
