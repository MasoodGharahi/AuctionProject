using MongoDB.Entities;
using SearchService.Entities;
using SearchService.Services.Interfaces;

namespace SearchService.Services
{
    public class AuctionServiceHttpClient: IAuctionServiceHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AuctionServiceHttpClient(HttpClient httpClient,IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<List<Item>> GetItemsForSearchDb()
        {
            string? lastUpdated=await DB.Find<Item,string>().Sort(x=>x.Descending(a=>a.UpdatedDate))
                .Project(x=>x.UpdatedDate.ToString()).ExecuteFirstAsync();
            return await _httpClient.GetFromJsonAsync<List<Item>>(_configuration["AuctionServiceUrl"]
                + "/api/auctions?date=" + lastUpdated);
        }
    }
}
