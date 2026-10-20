using Moq;
using ReadingSpot.Core.Entities;
using ReadingSpot.Core.Interfaces;
using ReadingSpot.Service.Spots;
using ReadingSpot.Tests.Helpers;

namespace ReadingSpot.Tests;

public class SpotServiceTests
{
    [Fact]
    public async Task Deactivate_SetsIsActiveFalse_InsteadOfDeleting()
    {
        var spot = TestData.Spot();
        var spots = new Mock<ISpotRepository>();
        spots.Setup(r => r.GetWithAmenitiesAsync(spot.Id, It.IsAny<CancellationToken>(), true)).ReturnsAsync(spot);
        var service = new SpotService(spots.Object, Mock.Of<IRepository<Amenity>>(), TestData.Mapper);

        await service.DeactivateAsync(spot.Id, default);

        Assert.False(spot.IsActive);
        spots.Verify(r => r.Remove(It.IsAny<StudySpot>()), Times.Never);
        spots.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
