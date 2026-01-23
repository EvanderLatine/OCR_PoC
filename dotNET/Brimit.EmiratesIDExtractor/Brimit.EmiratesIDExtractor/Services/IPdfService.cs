namespace Brimit.EmiratesIDExtractor.Services;

/// <summary>
/// Service for PDF operations
/// </summary>
public interface IPdfService
{
    /// <summary>
    /// Creates a two-page PDF from front and back images
    /// </summary>
    /// <param name="frontImage">Front image bytes</param>
    /// <param name="backImage">Back image bytes</param>
    /// <returns>PDF document bytes</returns>
    Task<byte[]> CreateTwoPagePdfAsync(byte[] frontImage, byte[] backImage);
}