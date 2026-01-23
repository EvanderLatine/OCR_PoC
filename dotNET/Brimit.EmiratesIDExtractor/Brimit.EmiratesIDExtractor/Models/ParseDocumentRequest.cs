using System.ComponentModel.DataAnnotations;

namespace Brimit.EmiratesIDExtractor.Models;

public class ParseDocumentRequest
{
    [Required]
    public IFormFile Front { get; set; }
    public IFormFile? Back { get; set; }
} 