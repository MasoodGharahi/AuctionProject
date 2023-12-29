using AutoMapper;
using Contracts;
using SearchService.Entities;

namespace SearchService.RequestHeplers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<AuctionCreated, Item>();
        }
    }
}
