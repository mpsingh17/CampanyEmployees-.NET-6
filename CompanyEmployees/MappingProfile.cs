﻿using AutoMapper;
using Entities.DataTransferObjects;
using Entities.Models;

namespace CompanyEmployees
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Company, CompanyDTO>()
                .ForMember(c => c.FullAddress,
                    opt => opt.MapFrom(
                        x => string.Join(' ', x.Address, x.Country)
                    )
                );
        }
    }
}