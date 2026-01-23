using Brimit.EmiratesIDExtractor.Controllers;
using Brimit.EmiratesIDExtractor.Factories;
using Brimit.EmiratesIDExtractor.Interfaces;
using Brimit.EmiratesIDExtractor.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Brimit.EmiratesIDExtractor.Tests.Controllers;

public class IdDocumentControllerTests
{
    private readonly Mock<IDocumentProcessorFactory> _factoryMock;
    private readonly Mock<ILogger<IdDocumentController>> _loggerMock;
    private readonly Mock<IDocumentProcessor> _processorMock;
    private readonly IdDocumentController _controller;

    public IdDocumentControllerTests()
    {
        _factoryMock = new Mock<IDocumentProcessorFactory>();
        _loggerMock = new Mock<ILogger<IdDocumentController>>();
        _processorMock = new Mock<IDocumentProcessor>();

        _factoryMock.Setup(f => f.GetProcessor("emirates-id")).Returns(_processorMock.Object);

        _controller = new IdDocumentController(_factoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ParseIdDocument_ValidTypeAndRequest_ReturnsOk()
    {
        // Arrange
        var request = new ParseDocumentRequest();
        var expectedResponse = ApiResponse<object>.Success(new { Name = "Test" });
        _processorMock.Setup(p => p.ProcessAsync(request)).ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.ParseIdDocument("emirates-id", request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task ParseIdDocument_InvalidType_ReturnsBadRequest()
    {
        // Arrange
        var request = new ParseDocumentRequest();
        _factoryMock.Setup(f => f.GetProcessor("invalid-type")).Throws<ArgumentException>();

        // Act
        var result = await _controller.ParseIdDocument("invalid-type", request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ParseIdDocument_ProcessorReturnsError_ReturnsAppropriateStatus()
    {
        // Arrange
        var request = new ParseDocumentRequest();
        var errorResponse = ApiResponse<object>.Error(422, "Invalid document");
        _processorMock.Setup(p => p.ProcessAsync(request)).ReturnsAsync(errorResponse);

        // Act
        var result = await _controller.ParseIdDocument("emirates-id", request);

        // Assert
        result.Should().BeOfType<UnprocessableEntityObjectResult>();
    }

    [Fact]
    public async Task ParseIdDocument_ExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var request = new ParseDocumentRequest();
        _processorMock.Setup(p => p.ProcessAsync(request)).ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.ParseIdDocument("emirates-id", request);

        // Assert
        var statusResult = result as StatusCodeResult;
        statusResult?.StatusCode.Should().Be(500);
    }
}
