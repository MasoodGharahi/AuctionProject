using SearchService.Entities;

namespace SearchService.Services.Interfaces
{
    public interface IAuctionServiceHttpClient
    {
       Task<List<Item>> GetItemsForSearchDb();
            
    }
}
