﻿using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Contracts;

namespace AuctionService.RequestHelpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Item, AuctionDTO>().ReverseMap();
            CreateMap<Auction, AuctionDTO>().IncludeMembers(x => x.Item).ReverseMap();
            CreateMap<CreateAuctionDTO, Auction>()
           .ForMember(d => d.Item, o => o.MapFrom(s => s));
            CreateMap<CreateAuctionDTO, Item>();
            CreateMap<AuctionDTO, AuctionCreated>();
            CreateMap<Auction, AuctionUpdated>().IncludeMembers(a => a.Item);
            CreateMap<Item, AuctionUpdated>();

        }

    }
}
