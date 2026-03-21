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

        CreateMap<Album, AlbumDto>()
            .ForCtorParam(nameof(AlbumDto.AlbumId), opt => opt.MapFrom(src => src.AlbumId))
            .ForCtorParam(nameof(AlbumDto.Title), opt => opt.MapFrom(src => src.Title))
            .ForCtorParam(nameof(AlbumDto.ArtistId), opt => opt.MapFrom(src => src.ArtistId));

        CreateMap<ArtistDto, Artist>()
            .ForMember(dest => dest.ArtistId, opt => opt.MapFrom(src => src.ArtistId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

        CreateMap<CreateArtistCommand, Artist>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.ArtistId, opt => opt.Ignore())
            .ForMember(dest => dest.Albums, opt => opt.Ignore());

        CreateMap<UpdateArtistCommand, Artist>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.ArtistId, opt => opt.Ignore())
            .ForMember(dest => dest.Albums, opt => opt.Ignore());
    }
}
