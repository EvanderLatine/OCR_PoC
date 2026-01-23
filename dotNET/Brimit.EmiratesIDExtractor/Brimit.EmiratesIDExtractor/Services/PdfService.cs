using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Brimit.EmiratesIDExtractor.Services;

/// <summary>
/// Service implementation for PDF operations
/// MCP context7: Using PdfSharpCore for PDF creation and SixLabors.ImageSharp for image processing
/// </summary>
public class PdfService : IPdfService
{
    private readonly ILogger<PdfService> _logger;

    public PdfService(ILogger<PdfService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Creates a two-page PDF from front and back images
    /// </summary>
    public async Task<byte[]> CreateTwoPagePdfAsync(byte[] frontImage, byte[] backImage)
    {
        _logger.LogInformation("Creating two-page PDF from Emirates ID images");
        
        try
        {
            if (frontImage == null || frontImage.Length == 0)
            {
                _logger.LogError("Front image cannot be empty");
                throw new ArgumentException("Front image cannot be empty");
            }

            using var document = new PdfDocument();
            
            // Add front page
            await AddImageToPageAsync(document, frontImage, "Front");
            
            // Add back page
            await AddImageToPageAsync(document, backImage, "Back");
            
            // Save to memory stream
            using var stream = new MemoryStream();
            document.Save(stream, false);
            
            _logger.LogInformation("Successfully created PDF with {PageCount} pages", document.PageCount);
            
            return stream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating PDF from images");
            throw;
        }
    }

    private async Task AddImageToPageAsync(PdfDocument document, byte[] imageBytes, string pageName)
    {
        _logger.LogDebug("Adding {PageName} image to PDF", pageName);
        
        if (imageBytes == null || imageBytes.Length == 0)
        {
            _logger.LogError("Image bytes for {pageName} cannot be empty", pageName);
            throw new ArgumentException($"Image bytes for {pageName} cannot be empty");
        }
        
        // Load image using ImageSharp
        using var imageStream = new MemoryStream(imageBytes);
        
        Image<Rgba32> image;
        try
        {
            image = await Image.LoadAsync<Rgba32>(imageStream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Invalid image format for {pageName}", pageName);
            throw new InvalidOperationException($"Invalid image format for {pageName}", ex);
        }
        
        using (image)
        {
            // Create PDF page
            var page = document.AddPage();
            page.Size = PdfSharpCore.PageSize.A4;
            
            using var gfx = XGraphics.FromPdfPage(page);
            
            // Convert to compatible format for PdfSharpCore
            using var tempStream = new MemoryStream();
            await image.SaveAsPngAsync(tempStream);
            tempStream.Position = 0;
            
            using var xImage = XImage.FromStream(() => tempStream);
            
            // Calculate dimensions to fit on page while maintaining aspect ratio
            var pageWidth = page.Width.Point;
            var pageHeight = page.Height.Point;
            var margin = 20;
            
            var availableWidth = pageWidth - (2 * margin);
            var availableHeight = pageHeight - (2 * margin);
            
            var imageAspectRatio = (double)xImage.PixelWidth / xImage.PixelHeight;
            var pageAspectRatio = availableWidth / availableHeight;
            
            double drawWidth, drawHeight;
            if (imageAspectRatio > pageAspectRatio)
            {
                // Image is wider than page
                drawWidth = availableWidth;
                drawHeight = drawWidth / imageAspectRatio;
            }
            else
            {
                // Image is taller than page
                drawHeight = availableHeight;
                drawWidth = drawHeight * imageAspectRatio;
            }
            
            // Center the image on the page
            var x = (pageWidth - drawWidth) / 2;
            var y = (pageHeight - drawHeight) / 2;
            
            // Draw the image
            gfx.DrawImage(xImage, x, y, drawWidth, drawHeight);
        }
        
        _logger.LogDebug("Successfully added {PageName} image to PDF", pageName);
    }
}