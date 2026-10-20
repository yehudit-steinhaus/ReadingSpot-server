using ReadingSpot.Core.Enums;

namespace ReadingSpot.Core.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public Role Role { get; set; } = Role.Member;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
