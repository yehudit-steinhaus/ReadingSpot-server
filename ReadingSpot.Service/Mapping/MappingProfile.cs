using AutoMapper;
using ReadingSpot.Core.DTOs;
using ReadingSpot.Core.Entities;

namespace ReadingSpot.Service.Mapping;

// כל המיפויים במערכת – במקום אחד.
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, AuthResponseDto>()
            .ForMember(d => d.UserId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.Token, o => o.Ignore());

        CreateMap<Amenity, AmenityDto>();
        CreateMap<StudySpot, SpotDto>();
        CreateMap<SpotSaveDto, StudySpot>();

        CreateMap<TimeSlot, TimeSlotDto>()
            .ForMember(d => d.Capacity, o => o.MapFrom(s => s.Spot.Capacity))
            .ForMember(d => d.AvailableSeats, o => o.MapFrom(s => s.Spot.Capacity - s.BookedCount))
            .ForMember(d => d.IsMine, o => o.Ignore());
        CreateMap<TimeSlotCreateDto, TimeSlot>();

        CreateMap<Booking, BookingDto>()
            .ForMember(d => d.UserFullName, o => o.MapFrom(s => s.User.FullName))
            .ForMember(d => d.StudySpotId, o => o.MapFrom(s => s.TimeSlot.StudySpotId))
            .ForMember(d => d.SpotName, o => o.MapFrom(s => s.TimeSlot.Spot.Name))
            .ForMember(d => d.StartUtc, o => o.MapFrom(s => s.TimeSlot.StartUtc))
            .ForMember(d => d.EndUtc, o => o.MapFrom(s => s.TimeSlot.EndUtc));
    }
}
