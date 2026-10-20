using System.Linq.Expressions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ReadingSpot.Core.Entities;
using ReadingSpot.Core.Enums;
using ReadingSpot.Core.DTOs;
using ReadingSpot.Core.Exceptions;
using ReadingSpot.Core.Interfaces;
using ReadingSpot.Service.Bookings;
using ReadingSpot.Tests.Helpers;

namespace ReadingSpot.Tests;

public class BookingServiceTests
{
    private readonly Mock<ITimeSlotRepository> _slots = new();
    private readonly Mock<IBookingRepository> _bookings = new();
    private readonly BookingService _service;

    public BookingServiceTests()
        => _service = new BookingService(_slots.Object, _bookings.Object, TestData.Mapper, NullLogger<BookingService>.Instance);

    private static readonly BookingCreateDto Request = new() { TimeSlotId = TestData.SlotId };

    private void GivenSlot(TimeSlot slot)
        => _slots.Setup(r => r.GetForBookingAsync(slot.Id, It.IsAny<CancellationToken>())).ReturnsAsync(slot);

    private void GivenUserAlreadyBooked(bool value)
        => _bookings.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Booking, bool>>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(value);

    private void GivenBooking(Booking booking)
        => _bookings.Setup(r => r.GetWithDetailsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
                    .ReturnsAsync(booking);

    private void VerifySaved(Times times)
        => _bookings.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), times);

    [Fact]
    public async Task Create_WhenSeatAvailable_CreatesBookingAndIncrementsCount()
    {
        var slot = TestData.Slot(capacity: 2, booked: 1);
        GivenSlot(slot);
        GivenUserAlreadyBooked(false);
        GivenBooking(TestData.Booking(TestData.MemberId, slot));

        var result = await _service.CreateAsync(Request, TestData.Member, default);

        Assert.Equal(2, slot.BookedCount);
        Assert.Equal(TestData.SlotId, result.TimeSlotId);
        _bookings.Verify(r => r.Add(It.Is<Booking>(b => b.UserId == TestData.MemberId && b.TimeSlot == slot)), Times.Once);
        VerifySaved(Times.Once());
    }

    [Fact]
    public async Task Create_WhenSlotIsFull_ThrowsConflict_AndDoesNotSave()
    {
        GivenSlot(TestData.Slot(capacity: 2, booked: 2));

        await Assert.ThrowsAsync<ConflictException>(() => _service.CreateAsync(Request, TestData.Member, default));
        VerifySaved(Times.Never());
    }

    [Fact]
    public async Task Create_WhenRepositoryReportsConcurrencyConflict_PropagatesConflict()
    {
        GivenSlot(TestData.Slot(capacity: 2, booked: 1));
        GivenUserAlreadyBooked(false);
        _bookings.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                 .ThrowsAsync(new ConflictException("המקום נתפס על ידי משתמש אחר."));

        await Assert.ThrowsAsync<ConflictException>(() => _service.CreateAsync(Request, TestData.Member, default));
    }

    [Fact]
    public async Task Create_WhenUserAlreadyBookedSlot_ThrowsConflict()
    {
        GivenSlot(TestData.Slot());
        GivenUserAlreadyBooked(true);

        await Assert.ThrowsAsync<ConflictException>(() => _service.CreateAsync(Request, TestData.Member, default));
        VerifySaved(Times.Never());
    }

    [Fact]
    public async Task Create_WhenSpotInactive_ThrowsValidation()
    {
        GivenSlot(TestData.Slot(active: false));

        await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(Request, TestData.Member, default));
    }

    [Fact]
    public async Task Create_WhenSlotInPast_ThrowsValidation()
    {
        GivenSlot(TestData.Slot(hoursFromNow: -1));

        await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(Request, TestData.Member, default));
    }

    [Fact]
    public async Task Create_WhenSlotNotFound_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateAsync(Request, TestData.Member, default));
    }

    [Fact]
    public async Task Cancel_BookingOfAnotherUser_ThrowsForbidden()
    {
        GivenBooking(TestData.Booking(TestData.OtherMemberId, TestData.Slot(booked: 1)));

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.CancelAsync(100, TestData.Member, default));
        VerifySaved(Times.Never());
    }

    [Fact]
    public async Task Cancel_OwnBooking_CancelsAndDecrementsCount()
    {
        var slot = TestData.Slot(booked: 1);
        var booking = TestData.Booking(TestData.MemberId, slot);
        GivenBooking(booking);

        await _service.CancelAsync(booking.Id, TestData.Member, default);

        Assert.Equal(BookingStatus.Cancelled, booking.Status);
        Assert.Equal(0, slot.BookedCount);
        VerifySaved(Times.Once());
    }
}
