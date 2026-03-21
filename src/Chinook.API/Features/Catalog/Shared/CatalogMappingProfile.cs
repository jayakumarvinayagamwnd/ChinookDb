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

        CreateMap<Track, TrackDto>()
            .ForCtorParam(nameof(TrackDto.TrackId), opt => opt.MapFrom(src => src.TrackId))
            .ForCtorParam(nameof(TrackDto.Name), opt => opt.MapFrom(src => src.Name))
            .ForCtorParam(nameof(TrackDto.AlbumId), opt => opt.MapFrom(src => src.AlbumId))
            .ForCtorParam(nameof(TrackDto.MediaTypeId), opt => opt.MapFrom(src => src.MediaTypeId))
            .ForCtorParam(nameof(TrackDto.GenreId), opt => opt.MapFrom(src => src.GenreId))
            .ForCtorParam(nameof(TrackDto.Composer), opt => opt.MapFrom(src => src.Composer))
            .ForCtorParam(nameof(TrackDto.Milliseconds), opt => opt.MapFrom(src => src.Milliseconds))
            .ForCtorParam(nameof(TrackDto.Bytes), opt => opt.MapFrom(src => src.Bytes))
            .ForCtorParam(nameof(TrackDto.UnitPrice), opt => opt.MapFrom(src => src.UnitPrice));

        CreateMap<Genre, GenreDto>()
            .ForCtorParam(nameof(GenreDto.GenreId), opt => opt.MapFrom(src => src.GenreId))
            .ForCtorParam(nameof(GenreDto.Name), opt => opt.MapFrom(src => src.Name));

        CreateMap<MediaType, MediaTypeDto>()
            .ForCtorParam(nameof(MediaTypeDto.MediaTypeId), opt => opt.MapFrom(src => src.MediaTypeId))
            .ForCtorParam(nameof(MediaTypeDto.Name), opt => opt.MapFrom(src => src.Name));

        CreateMap<Artist, SearchResultDto>()
            .ForCtorParam(nameof(SearchResultDto.Type), opt => opt.MapFrom(_ => "artist"))
            .ForCtorParam(nameof(SearchResultDto.Id), opt => opt.MapFrom(src => src.ArtistId))
            .ForCtorParam(nameof(SearchResultDto.Name), opt => opt.MapFrom(src => src.Name));

        CreateMap<Album, SearchResultDto>()
            .ForCtorParam(nameof(SearchResultDto.Type), opt => opt.MapFrom(_ => "album"))
            .ForCtorParam(nameof(SearchResultDto.Id), opt => opt.MapFrom(src => src.AlbumId))
            .ForCtorParam(nameof(SearchResultDto.Name), opt => opt.MapFrom(src => src.Title));

        CreateMap<Track, SearchResultDto>()
            .ForCtorParam(nameof(SearchResultDto.Type), opt => opt.MapFrom(_ => "track"))
            .ForCtorParam(nameof(SearchResultDto.Id), opt => opt.MapFrom(src => src.TrackId))
            .ForCtorParam(nameof(SearchResultDto.Name), opt => opt.MapFrom(src => src.Name));

        CreateMap<ArtistDto, Artist>()
            .ForMember(dest => dest.ArtistId, opt => opt.MapFrom(src => src.ArtistId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

        CreateMap<CreateArtistCommand, Artist>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.ArtistId, opt => opt.Ignore())
            .ForMember(dest => dest.Albums, opt => opt.Ignore());

        CreateMap<CreateAlbumCommand, Album>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.ArtistId, opt => opt.MapFrom(src => src.ArtistId))
            .ForMember(dest => dest.AlbumId, opt => opt.Ignore())
            .ForMember(dest => dest.Artist, opt => opt.Ignore())
            .ForMember(dest => dest.Tracks, opt => opt.Ignore());

        CreateMap<CreateTrackCommand, Track>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.AlbumId, opt => opt.MapFrom(src => src.AlbumId))
            .ForMember(dest => dest.MediaTypeId, opt => opt.MapFrom(src => src.MediaTypeId))
            .ForMember(dest => dest.GenreId, opt => opt.MapFrom(src => src.GenreId))
            .ForMember(dest => dest.Composer, opt => opt.MapFrom(src => src.Composer))
            .ForMember(dest => dest.Milliseconds, opt => opt.MapFrom(src => src.Milliseconds))
            .ForMember(dest => dest.Bytes, opt => opt.MapFrom(src => src.Bytes))
            .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice))
            .ForMember(dest => dest.TrackId, opt => opt.Ignore())
            .ForMember(dest => dest.Album, opt => opt.Ignore())
            .ForMember(dest => dest.MediaType, opt => opt.Ignore())
            .ForMember(dest => dest.Genre, opt => opt.Ignore());

        CreateMap<UpdateArtistCommand, Artist>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.ArtistId, opt => opt.Ignore())
            .ForMember(dest => dest.Albums, opt => opt.Ignore());

        CreateMap<UpdateAlbumCommand, Album>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.ArtistId, opt => opt.MapFrom(src => src.ArtistId))
            .ForMember(dest => dest.AlbumId, opt => opt.Ignore())
            .ForMember(dest => dest.Artist, opt => opt.Ignore())
            .ForMember(dest => dest.Tracks, opt => opt.Ignore());

        CreateMap<UpdateTrackCommand, Track>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.AlbumId, opt => opt.MapFrom(src => src.AlbumId))
            .ForMember(dest => dest.MediaTypeId, opt => opt.MapFrom(src => src.MediaTypeId))
            .ForMember(dest => dest.GenreId, opt => opt.MapFrom(src => src.GenreId))
            .ForMember(dest => dest.Composer, opt => opt.MapFrom(src => src.Composer))
            .ForMember(dest => dest.Milliseconds, opt => opt.MapFrom(src => src.Milliseconds))
            .ForMember(dest => dest.Bytes, opt => opt.MapFrom(src => src.Bytes))
            .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice))
            .ForMember(dest => dest.TrackId, opt => opt.Ignore())
            .ForMember(dest => dest.Album, opt => opt.Ignore())
            .ForMember(dest => dest.MediaType, opt => opt.Ignore())
            .ForMember(dest => dest.Genre, opt => opt.Ignore());
    }
}
