using Brimit.EmiratesIDExtractor.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Brimit.EmiratesIDExtractor.Tests.Services;

public class PdfServiceTests
{
    private readonly Mock<ILogger<PdfService>> _loggerMock;
    private readonly PdfService _pdfService;

    public PdfServiceTests()
    {
        _loggerMock = new Mock<ILogger<PdfService>>();
        _pdfService = new PdfService(_loggerMock.Object);
    }

    [Fact]
    public async Task CreateTwoPagePdfAsync_WithValidImages_ShouldReturnPdfBytes()
    {
        // Arrange
        // Create simple test images (1x1 pixel PNG)
        byte[] frontImage = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==");
        byte[] backImage = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==");

        // Act
        var result = await _pdfService.CreateTwoPagePdfAsync(frontImage, backImage);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Length.Should().BeGreaterThan(0);
        
        // PDF files start with %PDF
        var pdfHeader = System.Text.Encoding.ASCII.GetString(result.Take(4).ToArray());
        pdfHeader.Should().Be("%PDF");
    }

    [Fact]
    public async Task CreateTwoPagePdfAsync_WithEmptyFrontImage_ShouldThrowException()
    {
        // Arrange
        byte[] frontImage = Array.Empty<byte>();
        byte[] backImage = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => 
            await _pdfService.CreateTwoPagePdfAsync(frontImage, backImage));
    }

    [Fact]
    public async Task CreateTwoPagePdfAsync_WithInvalidImageData_ShouldThrowException()
    {
        // Arrange
        byte[] frontImage = new byte[] { 1, 2, 3, 4, 5 }; // Invalid image data
        byte[] backImage = new byte[] { 6, 7, 8, 9, 10 };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () => 
            await _pdfService.CreateTwoPagePdfAsync(frontImage, backImage));
    }
}