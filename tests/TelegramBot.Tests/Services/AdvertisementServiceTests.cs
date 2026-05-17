using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TelegramBot.Application.DTOs.Requests;
using TelegramBot.Application.DTOs.Responses;
using TelegramBot.Application.Interfaces.Repositories;
using TelegramBot.Application.Services;
using TelegramBot.Domain.Entities;

namespace TelegramBot.Tests.Services;

public class AdvertisementServiceTests
{
    private readonly Mock<IAdvertisementRepository> _adRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<AdvertisementService>> _loggerMock;
    private readonly AdvertisementService _sut;

    public AdvertisementServiceTests()
    {
        _adRepoMock = new Mock<IAdvertisementRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<AdvertisementService>>();

        _sut = new AdvertisementService(_adRepoMock.Object, _mapperMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_Should_CreateAd()
    {
        // Arrange
        var request = new CreateAdRequest
        {
            Title = "Test Ad",
            CreatedByAdminId = 123,
            Hotels = new List<HotelDto>()
        };

        var newAd = new Advertisement { Id = 1, Title = "Test Ad" };
        var createdAd = new Advertisement { Id = 1, Title = "Test Ad" };

        _mapperMock.Setup(m => m.Map<Advertisement>(request)).Returns(newAd);
        _adRepoMock.Setup(r => r.CreateAsync(newAd, It.IsAny<CancellationToken>())).ReturnsAsync(createdAd);
        _mapperMock.Setup(m => m.Map<AdResponse>(createdAd)).Returns(new AdResponse { Id = 1, Title = "Test Ad" });

        // Act
        var result = await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        _adRepoMock.Verify(r => r.CreateAsync(It.IsAny<Advertisement>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_Should_ThrowNotFound_When_NotExists()
    {
        // Arrange
        _adRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Advertisement?)null);

        // Act & Assert
        await FluentActions.Invoking(() => _sut.GetByIdAsync(1, CancellationToken.None))
            .Should().ThrowAsync<Domain.Exceptions.NotFoundException>();
    }
}
