using AutoMapper;
using Chinook.API.Infrastructure.Persistence.Entities.Catalog;

namespace Chinook.API.Features.Catalog;

public sealed class CatalogMappingProfile : Profile
{
    public CatalogMappingProfile()
    {
        CreateMap<Artist, ArtistDto>()
            .ForCtorParam(nameof(ArtistDto.ArtistId), opt => opt.MapFrom(src => src.ArtistId))
            .ForCtorParam(nameof(ArtistDto.Name), opt => opt.MapFrom(src => src.Name));

        CreateMap<ArtistDto, Artist>()
            .ForMember(dest => dest.ArtistId, opt => opt.MapFrom(src => src.ArtistId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));
    }
}
