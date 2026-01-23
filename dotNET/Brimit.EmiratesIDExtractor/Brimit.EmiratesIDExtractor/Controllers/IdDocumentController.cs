using Brimit.EmiratesIDExtractor.Factories;
using Brimit.EmiratesIDExtractor.Models;
using Microsoft.AspNetCore.Mvc;

namespace Brimit.EmiratesIDExtractor.Controllers;

[ApiController]
[Route("api/id-document")]
[Produces("application/json")]
public class IdDocumentController : ControllerBase
{
    private readonly IDocumentProcessorFactory _factory;
    private readonly ILogger<IdDocumentController> _logger;

    public IdDocumentController(
        IDocumentProcessorFactory factory,
        ILogger<IdDocumentController> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    /// <summary>
    /// Parses ID document of specified type
    /// </summary>
    /// <param name="documentType">Type of ID document to parse (e.g., emirates-id)</param>
    /// <param name="request">Parse request containing front and optional back files</param>
    /// <returns>Extracted ID document data</returns>
    /// <response code="200">Successfully extracted ID document data</response>
    /// <response code="400">Invalid input for ID document</response>
    /// <response code="422">Not a valid ID document or extraction failed</response>
    /// <response code="500">Internal server error</response>
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    [HttpPost("parse/{documentType}")]
    public async Task<IActionResult> ParseIdDocument(string documentType, [FromForm] ParseDocumentRequest request)
    {
        try
        {
            _logger.LogInformation("Received parse request for {DocumentType}", documentType);

            var processor = _factory.GetProcessor(documentType);

            var response = await processor.ProcessAsync(request);

            return response.Response switch
            {
                200 => Ok(response),
                400 => BadRequest(response),
                422 => UnprocessableEntity(response),
                _ => StatusCode(500, response)
            };
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid document type: {DocumentType}", documentType);
            return BadRequest(ApiResponse<object>.Error(400, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing {DocumentType}", documentType);
            return StatusCode(500, ApiResponse<object>.Error(500, "Internal server error"));
        }
    }
} 