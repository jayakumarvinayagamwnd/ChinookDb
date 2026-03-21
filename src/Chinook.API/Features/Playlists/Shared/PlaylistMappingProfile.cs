using AutoMapper;
using Chinook.API.Infrastructure.Persistence.Entities.Playlists;

namespace Chinook.API.Features.Playlists;

public sealed class PlaylistMappingProfile : Profile
{
    public PlaylistMappingProfile()
    {
        CreateMap<Playlist, PlaylistDto>()
            .ForCtorParam(nameof(PlaylistDto.PlaylistId), opt => opt.MapFrom(src => src.PlaylistId))
            .ForCtorParam(nameof(PlaylistDto.Name), opt => opt.MapFrom(src => src.Name));

        CreateMap<CreatePlaylistCommand, Playlist>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.PlaylistId, opt => opt.Ignore())
            .ForMember(dest => dest.PlaylistTracks, opt => opt.Ignore())
            .ForMember(dest => dest.Tracks, opt => opt.Ignore());

        CreateMap<UpdatePlaylistCommand, Playlist>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.PlaylistId, opt => opt.Ignore())
            .ForMember(dest => dest.PlaylistTracks, opt => opt.Ignore())
            .ForMember(dest => dest.Tracks, opt => opt.Ignore());
    }
}
