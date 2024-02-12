using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.Repository.Interface;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Repository
{
    public class AuctionRepository : IAuctionRepository
    {
        private readonly AuctionDbContext _repo;
        private readonly IMapper _mapper;

        public AuctionRepository(AuctionDbContext repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }
        public void AddAuction(Auction auction)
        {
            _repo.Auctions.Add(auction);
        }

        public async Task<AuctionDTO> GetAuctionByIdAsync(Guid id)
        {
            return await _repo.Auctions.ProjectTo<AuctionDTO>(_mapper.ConfigurationProvider)

                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Auction> GetAuctionEntityById(Guid id)
        {
            return await _repo.Auctions.Include(x => x.Item)
                  .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<AuctionDTO>> GetAuctionsAsync(string? date)
        {
            var auctions = _repo.Auctions.OrderBy(x => x.Item.Make).AsQueryable();
            if (!string.IsNullOrEmpty(date))
            {
                auctions = auctions.Where(x => x.UpdatedDate
                .CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);

            }
            return await auctions.ProjectTo<AuctionDTO>(_mapper.ConfigurationProvider).ToListAsync();
        }

        public void RemoveAuction(Auction auction)
        {
            _repo.Auctions.Remove(auction);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _repo.SaveChangesAsync() > 0;
        }
    }
}
