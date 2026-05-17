using AutoMapper;
using TelegramBot.Application.DTOs.Requests;
using TelegramBot.Application.DTOs.Responses;
using TelegramBot.Domain.Entities;

namespace TelegramBot.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Advertisement, AdResponse>()
            .ForMember(d => d.TotalLeads, opt => opt.MapFrom(s => s.Leads.Count));

        CreateMap<Hotel, HotelResponse>();

        CreateMap<CreateAdRequest, Advertisement>()
            .ForMember(d => d.Hotels, opt => opt.MapFrom(s => s.Hotels))
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.Leads, opt => opt.Ignore())
            .ForMember(d => d.Analytics, opt => opt.Ignore());

        CreateMap<HotelDto, Hotel>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.AdvertisementId, opt => opt.Ignore())
            .ForMember(d => d.SortOrder, opt => opt.Ignore())
            .ForMember(d => d.Advertisement, opt => opt.Ignore());

        CreateMap<Lead, LeadResponse>();
    }
}
